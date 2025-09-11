window.pixiGame = {
    app: null,
    camera: null,
    locationObjects: new Map(), // 위치 객체들을 저장하는 Map

    init: function (canvasId, locations = []) {
        const canvas = document.getElementById(canvasId);

        // Application 생성 (화면 크기 = 브라우저 크기)
        this.app = new PIXI.Application({
            view: canvas,
            resizeTo: window,   // 브라우저 창 크기에 맞게 자동 리사이즈
            backgroundColor: 0xffffff
        });

        window._pixiApp = this.app;

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

        console.log("typeof:", typeof robots);
        console.log("isArray:", Array.isArray(robots));
        console.log("value:", robots);

        robots.forEach(robot => {
            const group = this._createRobotGroup(robot);
            this.camera.addChild(group);
            this.robotObjects.set(robot.id, group);
        });
        if (!this._robotTickerStarted) {
            this._startRobotTicker();
            this._robotTickerStarted = true;
        }
    },

    addRobot: function (robot) {
        if (!this.robotObjects) this.robotObjects = new Map();
        if (this.robotObjects.has(robot.id)) {
            this.updateRobot(robot);
            return;
        }
        const group = this._createRobotGroup(robot);
        this.camera.addChild(group);
        this.robotObjects.set(robot.id, group);
    },

    updateRobot: function (robot) {
        if (!this.robotObjects) return;
        const group = this.robotObjects.get(robot.id);
        if (!group) return;
        group.x = robot.x * 2;
        group.y = robot.y * 2;
    },

    removeRobot: function (robotId) {
        if (!this.robotObjects) return;
        const group = this.robotObjects.get(robotId);
        if (!group) return;
        this.camera.removeChild(group);
        this.robotObjects.delete(robotId);
    },

    _createRobotGroup: function (robot) {
        const group = new PIXI.Container();
        const color = robot.robotType === 'Logistics' ? 0x00aaff : 0xff8800;
        const radius = 10;
        const gfx = new PIXI.Graphics();
        gfx.beginFill(color);
        gfx.drawCircle(0, 0, radius);
        gfx.endFill();
        group.addChild(gfx);
        const style = new PIXI.TextStyle({ fontFamily: 'Arial', fontSize: 10, fill: 0x000000 });
        const label = new PIXI.Text(robot.id, style);
        label.anchor.set(0.5, 0);
        label.y = radius + 2;
        group.addChild(label);
        group.x = robot.x * 2;
        group.y = robot.y * 2;
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
            'Cassette': 0xff4444,    // 빨간색
            'Tray': 0x44ff44,        // 초록색
            'Memory': 0x4444ff,      // 파란색
            'Marker': 0xffff44        // 노란색
        };

        // 위치 타입별 크기 정의
        const sizeMap = {
            'Cassette': { width: 30, height: 30 },
            'Tray': { width: 20, height: 20 },
            'Memory': { width: 5, height: 5 },
            'Marker': { width: 40, height: 40 }
        };

        const locationGroup = new PIXI.Container();

        // 위치 박스 생성
        const rect = new PIXI.Graphics();
        const color = colorMap[location.locationType] || 0x888888;
        const size = sizeMap[location.locationType] || { width: 25, height: 25 };

        rect.beginFill(color);
        rect.drawRect(0, 0, size.width, size.height);
        rect.endFill();

        // 테두리 추가 (상태에 따라 다른 색상)
        const borderColor = location.status === 'Occupied' ? 0x000000 : 0x666666;
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
            fill: 0x000000,
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
                fill: 0xff0000,
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
