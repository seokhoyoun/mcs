window.pixiGame = {
    app: null,
    camera: null,
    locationObjects: new Map(), // 위치 객체들을 저장하는 Map
    robotPathTargets: new Map(), // robotId -> {tx, ty}
    robotPathGraphics: new Map(), // robotId -> PIXI.Graphics (dashed path)
    robotMoveUntil: new Map(), // robotId -> timestamp(ms) until ripple shows
    robotRippleState: new Map(), // robotId -> { gfx, radius }

    // Theme colors resolved from MudBlazor CSS variables
    themeColors: {},

    _cssToInt: function (css) {
        if (!css) return null;
        const s = css.toString().trim();
        if (s.startsWith('#')) {
            let hex = s.substring(1);
            if (hex.length === 3) {
                hex = hex.split('').map(c => c + c).join('');
            }
            const n = parseInt(hex, 16);
            return isNaN(n) ? null : n;
        }
        const m = s.match(/^rgba?\((\d+)\s*,\s*(\d+)\s*,\s*(\d+)/i);
        if (m) {
            const r = (parseInt(m[1]) & 0xff) << 16;
            const g = (parseInt(m[2]) & 0xff) << 8;
            const b = (parseInt(m[3]) & 0xff);
            return r | g | b;
        }
        return null;
    },

    _anyToIntColor: function (val) {
        if (val === undefined || val === null) return null;
        if (typeof val === 'number') return val >>> 0;
        return this._cssToInt(val);
    },

    setTheme: function (themeDto) {
        const t = themeDto || {};
        this.themeColors = {
            primary: this._anyToIntColor(t.primary),
            secondary: this._anyToIntColor(t.secondary),
            info: this._anyToIntColor(t.info),
            success: this._anyToIntColor(t.success),
            warning: this._anyToIntColor(t.warning),
            textPrimary: this._anyToIntColor(t.textPrimary),
            textSecondary: this._anyToIntColor(t.textSecondary),
            surface: this._anyToIntColor(t.surface),
            background: this._anyToIntColor(t.background)
        };
        // Update canvas background when theme changes
        if (this.app && this.app.renderer && this.themeColors.background != null) {
            try {
                if (this.app.renderer.background && typeof this.app.renderer.background === 'object') {
                    this.app.renderer.background.color = this.themeColors.background >>> 0;
                } else if ('backgroundColor' in this.app.renderer) {
                    this.app.renderer.backgroundColor = this.themeColors.background >>> 0;
                }
            } catch (e) { /* ignore */ }
        }
        // If scene exists, refresh visuals
        if (this.camera) {
            this.refreshThemeStyles();
        }
    },

    init: function (canvasId, locations = []) {
        const canvas = document.getElementById(canvasId);

        // Application 생성 (화면 크기 = 브라우저 크기)
        this.app = new PIXI.Application({
            view: canvas,
            resizeTo: window,   // 브라우저 창 크기에 맞게 자동 리사이즈
            backgroundColor: 0xffffff
        });

        window._pixiApp = this.app;

        // Apply initial background from theme if available
        if (this.themeColors && this.themeColors.background != null) {
            try {
                if (this.app && this.app.renderer) {
                    if (this.app.renderer.background && typeof this.app.renderer.background === 'object') {
                        this.app.renderer.background.color = this.themeColors.background >>> 0;
                    } else if ('backgroundColor' in this.app.renderer) {
                        this.app.renderer.backgroundColor = this.themeColors.background >>> 0;
                    }
                }
            } catch (e) { /* ignore */ }
        }

        // 브라우저 창 크기 변경 시 자동 리사이즈
        window.addEventListener('resize', () => {
            this.app.renderer.resize(window.innerWidth, window.innerHeight);
        });

        // 카메라 컨테이너
        this.camera = new PIXI.Container();
        this.app.stage.addChild(this.camera);
        window._camera = this.camera;

        // 위치 정보 기반으로 오브젝트 생성
        this.createLocationObjects(locations);

        // 키 입력 상태
        const keys = {};
        window.addEventListener("keydown", e => { keys[e.code] = true; });
        window.addEventListener("keyup", e => { keys[e.code] = false; });

        const speed = 5;

        this.app.ticker.add(() => {
            if (keys["ArrowUp"]) this.camera.y += speed;
            if (keys["ArrowDown"]) this.camera.y -= speed;
            if (keys["ArrowLeft"]) this.camera.x += speed;
            if (keys["ArrowRight"]) this.camera.x -= speed;
        });

        let zoom = 1;

        window.addEventListener("wheel", (e) => {
            // Ctrl 키 눌린 상태에서만 줌 활성화
            if (e.ctrlKey) {
                e.preventDefault(); // 브라우저 기본 줌 방지

                if (e.deltaY < 0) {
                    zoom *= 1.1;
                } else {
                    zoom /= 1.1;
                }

                zoom = Math.min(Math.max(zoom, 0.5), 3);

                this.camera.scale.set(zoom);

                console.log(`Zoom level: ${zoom.toFixed(2)}`);
            }
        }, { passive: false });

        return true;
    },

    // --- Robot support ---
    loadRobots: function (robots = []) {
        if (!this.robotObjects) {
            this.robotObjects = new Map();
        }
        if (!robots) return;

        robots.forEach(robot => {
            const group = this._createRobotGroup(robot);
            this.camera.addChild(group);
            this.robotObjects.set(robot.id, group);
        });
        if (!this._robotTickerStarted) {
            this._startRobotTicker();
            this._robotTickerStarted = true;
        }

        // Always run ripple updater once robots are present
        if (!this._rippleTickerAttached && this.app) {
            this.app.ticker.add(this._updateRipples, this);
            this._rippleTickerAttached = true;
        }
    },

    // Set a dashed path from robot to target (world coords). Re-draws on updates.
    setRobotPath: function (robotId, targetX, targetY) {
        if (!this.camera) return;
        this.robotPathTargets.set(robotId, { tx: targetX, ty: targetY });
        this._drawRobotPath(robotId);
    },

    // Clear dashed path for robot
    clearRobotPath: function (robotId) {
        const g = this.robotPathGraphics.get(robotId);
        if (g) {
            try { this.camera.removeChild(g); } catch { }
            g.destroy({ children: false, texture: false, baseTexture: false });
        }
        this.robotPathGraphics.delete(robotId);
        this.robotPathTargets.delete(robotId);
    },

    updateRobot: function (robot) {
        if (!this.robotObjects) return;
        const group = this.robotObjects.get(robot.id);
        if (!group) return;
        const prevX = group.x;
        const prevY = group.y;
        const nextX = robot.x * 2;
        const nextY = robot.y * 2;
        group.x = nextX;
        group.y = nextY;

        // Mark as moving if position changed
        const dx = nextX - prevX;
        const dy = nextY - prevY;
        if (Math.abs(dx) > 0.1 || Math.abs(dy) > 0.1) {
            const now = (typeof performance !== 'undefined') ? performance.now() : Date.now();
            this.robotMoveUntil.set(robot.id, now + 600); // show ripple for 600ms after movement
        }

        // If a path target exists, redraw the dashed path from new position
        if (this.robotPathTargets && this.robotPathTargets.has(robot.id)) {
            this._drawRobotPath(robot.id);
        }
    },

    removeRobot: function (robotId) {
        if (!this.robotObjects) return;
        const group = this.robotObjects.get(robotId);
        if (!group) return;
        this.camera.removeChild(group);
        this.robotObjects.delete(robotId);
        this.clearRobotPath(robotId);
        this._clearRipple(robotId);
    },

    _createRobotGroup: function (robot) {
        const group = new PIXI.Container();
        const color = robot.robotType === 'Logistics'
            ? (this.themeColors.primary ?? 0x00aaff)
            : (this.themeColors.secondary ?? 0xff8800);
        const radius = 10;
        const gfx = new PIXI.Graphics();
        gfx.beginFill(color);
        gfx.drawCircle(0, 0, radius);
        gfx.endFill();
        group.addChild(gfx);
        const style = new PIXI.TextStyle({ fontFamily: 'Arial', fontSize: 9, fill: (this.themeColors.textPrimary ?? 0x000000) });
        const label = new PIXI.Text(robot.id, style);
        label.anchor.set(0.5, 0);
        label.y = radius + 2;
        group.addChild(label);
        group.x = robot.x * 2;
        group.y = robot.y * 2;
        group._robotType = robot.robotType;
        const baseSpeed = robot.robotType === 'Logistics' ? 1.5 : 1.0;
        group.vx = (Math.random() * 2 - 1) * baseSpeed;
        group.vy = (Math.random() * 2 - 1) * baseSpeed;
        gfx.interactive = true;
        gfx.buttonMode = true;
        gfx.on('pointerdown', () => {
            console.log(`로봇 클릭: ${robot.name} (${robot.id}) type=${robot.robotType}`);
        });
        return group;
    },

    refreshThemeStyles: function () {
        // Recolor robots
        if (this.robotObjects) {
            this.robotObjects.forEach((group, id) => {
                if (!group || !group.children || group.children.length === 0) return;
                const gfx = group.children[0];
                if (gfx && gfx.clear && gfx.beginFill) {
                    const isLogistics = (group._robotType === 'Logistics');
                    const color = isLogistics
                        ? (this.themeColors.primary ?? 0x00aaff)
                        : (this.themeColors.secondary ?? 0xff8800);
                    gfx.clear();
                    gfx.beginFill(color);
                    gfx.drawCircle(0, 0, 10);
                    gfx.endFill();
                }
                const label = group.children[1];
                if (label && label.style) {
                    label.style.fill = (this.themeColors.textPrimary ?? 0x000000);
                    label.dirty = true;
                }
            });
        }

        // Redraw existing dashed paths with new color
        if (this.robotPathTargets) {
            this.robotPathTargets.forEach((_, robotId) => this._drawRobotPath(robotId));
        }

        // Recolor locations
        if (this.locationObjects) {
            this.locationObjects.forEach((group, id) => {
                if (!group || !group.children || group.children.length === 0) return;
                const rect = group.children[0];
                const label = group.children[1];
                const locType = group._locType;
                const locStatus = group._locStatus;
                const size = group._locSize || { width: 25, height: 25 };
                const colorMap = {
                    'Cassette': (this.themeColors.warning ?? 0xffc107),
                    'Tray': (this.themeColors.success ?? 0x2e7d32),
                    'Memory': (this.themeColors.info ?? 0x0288d1),
                    'Marker': (this.themeColors.secondary ?? 0x9c27b0)
                };
                const color = colorMap[locType] || 0x888888;
                const borderColor = (locStatus === 'Occupied')
                    ? (this.themeColors.textPrimary ?? 0x000000)
                    : (this.themeColors.textSecondary ?? 0x666666);
                if (rect && rect.clear && rect.beginFill) {
                    rect.clear();
                    rect.beginFill(color);
                    rect.drawRect(0, 0, size.width, size.height);
                    rect.endFill();
                    rect.lineStyle(2, borderColor);
                    rect.drawRect(0, 0, size.width, size.height);
                }
                if (label && label.style) {
                    label.style.fill = (this.themeColors.textPrimary ?? 0x000000);
                    label.dirty = true;
                }
            });
        }
    },

    _startRobotTicker: function () {
        if (!this.app) return;
        this.app.ticker.add(this._updateRobots, this);
    },

    _updateRobots: function () {
        if (!this.robotObjects || !this.app) return;
        const width = this.app.renderer.width;
        const height = this.app.renderer.height;
        this.robotObjects.forEach(group => {
            if (typeof group.vx !== 'number') group.vx = 1;
            if (typeof group.vy !== 'number') group.vy = 1;
            group.x += group.vx;
            group.y += group.vy;
            if (group.x < 0 || group.x > width) group.vx *= -1;
            if (group.y < 0 || group.y > height) group.vy *= -1;
        });
    },

    _ensureRipple: function (robotId) {
        let state = this.robotRippleState.get(robotId);
        if (state && state.gfx && !state.gfx.destroyed) {
            return state;
        }
        const gfx = new PIXI.Graphics();
        this.camera.addChild(gfx);
        state = { gfx: gfx, radius: 14 };
        this.robotRippleState.set(robotId, state);
        return state;
    },

    _clearRipple: function (robotId) {
        const state = this.robotRippleState.get(robotId);
        if (state && state.gfx) {
            try { this.camera.removeChild(state.gfx); } catch { }
            state.gfx.destroy({ children: false, texture: false, baseTexture: false });
        }
        this.robotRippleState.delete(robotId);
        this.robotMoveUntil.delete(robotId);
    },

    _updateRipples: function () {
        if (!this.app || !this.robotObjects) return;
        const now = (typeof performance !== 'undefined') ? performance.now() : Date.now();
        // Iterate over active move indications
        this.robotMoveUntil.forEach((until, robotId) => {
            if (until <= now) {
                this._clearRipple(robotId);
                return;
            }
            const group = this.robotObjects.get(robotId);
            if (!group) {
                this._clearRipple(robotId);
                return;
            }
            const state = this._ensureRipple(robotId);
            const gfx = state.gfx;
            const maxR = 50;
            const minR = 14;
            const speed = 1.2; // px per frame (scaled)
            state.radius += speed;
            if (state.radius > maxR) {
                state.radius = minR;
            }
            const t = (state.radius - minR) / (maxR - minR);
            const alpha = 0.35 * (1.0 - t);
            gfx.clear();
            gfx.lineStyle(2, (this.themeColors.primary ?? 0x3399ff), alpha);
            gfx.drawCircle(group.x, group.y, state.radius);
        });
    },

    _drawRobotPath: function (robotId) {
        if (!this.robotObjects || !this.camera) return;
        const group = this.robotObjects.get(robotId);
        const target = this.robotPathTargets.get(robotId);
        if (!group || !target) return;

        // Remove previous graphic if any
        const prev = this.robotPathGraphics.get(robotId);
        if (prev) {
            try { this.camera.removeChild(prev); } catch { }
            prev.destroy({ children: false, texture: false, baseTexture: false });
        }

        // Create new dashed path graphic
        const g = new PIXI.Graphics();
        g.alpha = 0.8;
        const color = this.themeColors.info ?? 0x4444aa;
        const lineWidth = 2;
        const dash = 10; // px
        const gap = 6; // px

        const x1 = group.x;
        const y1 = group.y;
        const x2 = target.tx * 2;
        const y2 = target.ty * 2;

        const dx = x2 - x1;
        const dy = y2 - y1;
        const dist = Math.hypot(dx, dy);
        if (dist < 1) {
            // Close enough; clear
            this.robotPathGraphics.delete(robotId);
            this.robotPathTargets.delete(robotId);
            return;
        }

        const ux = dx / dist;
        const uy = dy / dist;

        let traveled = 0;
        while (traveled < dist) {
            const seg = Math.min(dash, dist - traveled);
            const sx = x1 + ux * traveled;
            const sy = y1 + uy * traveled;
            const ex = x1 + ux * (traveled + seg);
            const ey = y1 + uy * (traveled + seg);
            g.lineStyle(lineWidth, color, 1);
            g.moveTo(sx, sy);
            g.lineTo(ex, ey);
            traveled += dash + gap;
        }

        this.camera.addChild(g);
        this.robotPathGraphics.set(robotId, g);
    },

    // enable/disable client-side robot auto movement
    setRobotAutoMove: function (enabled) {
        if (!this.app) return;
        if (enabled) {
            if (!this._robotTickerStarted) {
                this._startRobotTicker();
                this._robotTickerStarted = true;
            }
        } else {
            if (this._robotTickerStarted) {
                this.app.ticker.remove(this._updateRobots, this);
                this._robotTickerStarted = false;
            }
        }
    },


    // 새로운 위치 추가
    addLocation: function (location) {
        // 이미 존재하는 위치인지 확인
        if (this.locationObjects.has(location.id)) {
            console.log(`위치 ${location.id}가 이미 존재합니다. 업데이트를 수행합니다.`);
            this.updateLocation(location);
            return;
        }

        const locationGroup = this.createLocationGroup(location);
        this.camera.addChild(locationGroup);
        this.locationObjects.set(location.id, locationGroup);

        // 추가 애니메이션 효과
        locationGroup.alpha = 0;
        locationGroup.scale.set(0.5);

        // Fade in과 scale 애니메이션
        this.animateLocationIn(locationGroup);

        console.log(`새 위치가 추가되었습니다: ${location.name} (${location.id})`);
    },

    // 위치 제거
    removeLocation: function (locationId) {
        const locationGroup = this.locationObjects.get(locationId);
        if (locationGroup) {
            // Fade out 애니메이션 후 제거
            this.animateLocationOut(locationGroup, () => {
                this.camera.removeChild(locationGroup);
                this.locationObjects.delete(locationId);
                console.log(`위치가 제거되었습니다: ${locationId}`);
            });
        } else {
            console.log(`제거할 위치를 찾을 수 없습니다: ${locationId}`);
        }
    },

    // 위치 업데이트
    updateLocation: function (location) {
        const existingGroup = this.locationObjects.get(location.id);
        if (existingGroup) {
            // 기존 위치 제거
            this.camera.removeChild(existingGroup);
            this.locationObjects.delete(location.id);
        }

        // 새로운 위치 생성 및 추가
        const newLocationGroup = this.createLocationGroup(location);
        this.camera.addChild(newLocationGroup);
        this.locationObjects.set(location.id, newLocationGroup);

        // 업데이트 시각 효과
        this.flashLocation(newLocationGroup);

        console.log(`위치가 업데이트되었습니다: ${location.name} (${location.id})`);
    },

    // 모든 위치 제거
    clearAllLocations: function () {
        this.locationObjects.forEach((locationGroup, id) => {
            this.camera.removeChild(locationGroup);
        });
        this.locationObjects.clear();
        console.log("모든 위치가 제거되었습니다.");
    },

    // 위치 객체 생성 (여러 위치)
    createLocationObjects: function (locations = []) {
        // null, undefined 체크
        if (!locations) {
            console.log("위치 데이터가 없습니다.");
            return;
        }

        locations.forEach(location => {
            const locationGroup = this.createLocationGroup(location);
            this.camera.addChild(locationGroup);
            this.locationObjects.set(location.id, locationGroup);
        });

        console.log(`총 ${locations.length}개의 위치가 표시되었습니다.`);
    },

    // 개별 위치 그룹 생성
    createLocationGroup: function (location) {
        // 위치 타입별 색상 정의
        const colorMap = {
            'Cassette': (this.themeColors.warning ?? 0xffc107),
            'Tray': (this.themeColors.success ?? 0x2e7d32),
            'Memory': (this.themeColors.info ?? 0x0288d1),
            'Marker': (this.themeColors.secondary ?? 0x9c27b0)
        };

        // 위치 타입별 크기 정의
        const sizeMap = {
            'Cassette': { width: 30, height: 30 },
            'Tray': { width: 20, height: 20 },
            'Memory': { width: 5, height: 5 },
            'Marker': { width: 40, height: 40 }
        };

        const locationGroup = new PIXI.Container();
        locationGroup._locType = location.locationType;
        locationGroup._locStatus = location.status;

        // 위치 박스 생성
        const rect = new PIXI.Graphics();
        const color = colorMap[location.locationType] || 0x888888;
        let size = sizeMap[location.locationType] || { width: 25, height: 25 };
        if (location && typeof location.width === 'number' && typeof location.height === 'number' && location.width > 0 && location.height > 0) {
            size = { width: location.width, height: location.height };
        }
        locationGroup._locSize = size;

        rect.beginFill(color);
        rect.drawRect(0, 0, size.width, size.height);
        rect.endFill();

        // 테두리 추가 (상태에 따라 다른 색상)
        const borderColor = location.status === 'Occupied'
            ? (this.themeColors.textPrimary ?? 0x000000)
            : (this.themeColors.textSecondary ?? 0x666666);
        rect.lineStyle(2, borderColor);
        rect.drawRect(0, 0, size.width, size.height);

        // 위치 설정 (스케일 조정 - 실제 좌표계에 맞게)
        locationGroup.x = location.x * 2; // 스케일 조정
        locationGroup.y = location.y * 2; // 스케일 조정

        // 상호작용 활성화
        rect.interactive = true;
        rect.buttonMode = true;

        // 클릭 이벤트
        rect.on('pointerdown', () => {
            console.log(`위치 클릭됨: ${location.name} (${location.id})`);
            console.log(`타입: ${location.locationType}, 상태: ${location.status}`);
            console.log(`좌표: (${location.x}, ${location.y}, ${location.z})`);
            if (location.currentItemId) {
                console.log(`현재 아이템: ${location.currentItemId}`);
            }
        });

        // 호버 이벤트
        rect.on('pointerover', () => {
            rect.tint = 0xdddddd; // 밝게 표시
        });

        rect.on('pointerout', () => {
            rect.tint = 0xffffff; // 원래 색상으로 복원
        });

        locationGroup.addChild(rect);

        // 라벨 텍스트 추가
        const style = new PIXI.TextStyle({
            fontFamily: 'Arial',
            fontSize: 8,
            fill: (this.themeColors.textPrimary ?? 0x000000),
            align: 'center'
        });

        const text = new PIXI.Text(location.id, style);
        text.x = size.width / 2;
        text.y = size.height + 2;
        text.anchor.set(0.5, 0);

        locationGroup.addChild(text);

        // 현재 아이템이 있는 경우 표시
        if (location.currentItemId && location.currentItemId !== '') {
            const itemStyle = new PIXI.TextStyle({
                fontFamily: 'Arial',
                fontSize: 8,
                fill: (this.themeColors.primary ?? 0xff0000),
                align: 'center'
            });

            const itemText = new PIXI.Text(location.currentItemId, itemStyle);
            itemText.x = size.width / 2;
            itemText.y = -12;
            itemText.anchor.set(0.5, 0);

            locationGroup.addChild(itemText);
        }

        return locationGroup;
    },

    // 위치 추가 애니메이션
    animateLocationIn: function (locationGroup) {
        // GSAP 또는 간단한 애니메이션이 없다면 기본 애니메이션 사용
        const animateStep = () => {
            locationGroup.alpha += 0.05;
            locationGroup.scale.x += 0.025;
            locationGroup.scale.y += 0.025;

            if (locationGroup.alpha < 1 || locationGroup.scale.x < 1) {
                requestAnimationFrame(animateStep);
            } else {
                locationGroup.alpha = 1;
                locationGroup.scale.set(1);
            }
        };
        animateStep();
    },

    // 위치 제거 애니메이션
    animateLocationOut: function (locationGroup, onComplete) {
        const animateStep = () => {
            locationGroup.alpha -= 0.1;
            locationGroup.scale.x -= 0.05;
            locationGroup.scale.y -= 0.05;

            if (locationGroup.alpha > 0) {
                requestAnimationFrame(animateStep);
            } else {
                onComplete();
            }
        };
        animateStep();
    },

    // 위치 업데이트 시 깜빡임 효과
    flashLocation: function (locationGroup) {
        const originalTint = locationGroup.children[0].tint;
        let flashCount = 0;
        const maxFlash = 6;

        const flash = () => {
            locationGroup.children[0].tint = flashCount % 2 === 0 ? 0xffff00 : originalTint;
            flashCount++;

            if (flashCount < maxFlash) {
                setTimeout(flash, 100);
            } else {
                locationGroup.children[0].tint = originalTint;
            }
        };
        flash();
    }
};
