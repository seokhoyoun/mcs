// Minimal Three.js layer integrated with pixiGame namespace
(function () {
    if (!window) return;
    const pg = window.pixiGame || (window.pixiGame = {});

    function intColor(val) {
        if (typeof val === 'number') return (val >>> 0) & 0xffffff;
        try { return new THREE.Color(val).getHex(); } catch (e) { return 0xffffff; }
    }
    function num(a, b, def) { if (typeof a === 'number' && isFinite(a)) return a; if (typeof b === 'number' && isFinite(b)) return b; return def; }
    function normLoc(l) {
        if (!l) return { id: '', name: '', locationType: 'Marker', status: 'Available', x: 0, y: 0, z: 0, width: 0, height: 0, depth: 0, markerRole: '' };
        const id = l.id != null ? l.id : l.Id; const name = l.name != null ? l.name : l.Name;
        const locationType = l.locationType != null ? l.locationType : l.LocationType;
        const status = l.status != null ? l.status : l.Status;
        const x = num(l.x, l.X, 0), y = num(l.y, l.Y, 0), z = num(l.z, l.Z, 0);
        const width = num(l.width, l.Width, 0), height = num(l.height, l.Height, 0), depth = num(l.depth, l.Depth, 0);
        const markerRole = (l.markerRole != null ? l.markerRole : l.MarkerRole) || '';
        return { id, name, locationType, status, x, y, z, width, height, depth, markerRole };
    }
    function normRobot(r) { if (!r) return { id: '', robotType: 'Logistics', x: 0, y: 0, z: 0 }; return { id: (r.id != null ? r.id : r.Id), robotType: (r.robotType != null ? r.robotType : r.RobotType), x: num(r.x, r.X, 0), y: num(r.y, r.Y, 0), z: num(r.z, r.Z, 0) }; }

    function roleFromId(l) {
        const loc = normLoc(l); if (!loc || loc.locationType !== 'Marker') return null; const s = (loc.id || '').toString().toUpperCase();
        if (/\.SET[0-9A-Z]+$/.test(s)) return 'set';
        if (s.indexOf('MOVE.') === 0) return 'movearea';
        if (s.indexOf('ST') === 0) return 'stocker';
        if (s.indexOf('A') === 0 && s.indexOf('.') === -1) return 'area';
        return null;
    }

    function pickRole(l) { const n = normLoc(l); if (n.markerRole) return n.markerRole.toString().toLowerCase(); return roleFromId(n); }

    function createLocationMesh(pgRef, l) {
        const loc = normLoc(l);
        const baseType = loc.locationType;
        const typeColors = {
            'Cassette': (pgRef.themeColors && pgRef.themeColors.info != null ? pgRef.themeColors.info : 0xffc107),
            'Tray': (pgRef.themeColors && pgRef.themeColors.success != null ? pgRef.themeColors.success : 0x2e7d32),
            'Memory': (pgRef.themeColors && pgRef.themeColors.info != null ? pgRef.themeColors.info : 0x0288d1),
            'Marker': (pgRef.themeColors && pgRef.themeColors.secondary != null ? pgRef.themeColors.secondary : 0x9c27b0)
        };
        let color = typeColors[baseType] || 0x888888;
        const role = pickRole(loc);
        if (role) {
            const map = {
                'area': (pgRef.themeColors && pgRef.themeColors.background != null ? pgRef.themeColors.background : 0x2e7d32),
                'stocker': (pgRef.themeColors && pgRef.themeColors.background != null ? pgRef.themeColors.background : 0xffc107),
                'set': (pgRef.themeColors && pgRef.themeColors.primary != null ? pgRef.themeColors.primary : 0x3f51b5),
                'movearea': (pgRef.themeColors && pgRef.themeColors.info != null ? pgRef.themeColors.info : 0x0288d1)
            };
            color = map[role] || color;
        }
        const borderColor = (loc.status === 'Occupied') ? (pgRef.themeColors && pgRef.themeColors.textPrimary != null ? pgRef.themeColors.textPrimary : 0x000000) : (pgRef.themeColors && pgRef.themeColors.textSecondary != null ? pgRef.themeColors.textSecondary : 0x666666);
        const widthX = typeof loc.width === 'number' ? loc.width : 0;
        const heightY = typeof loc.height === 'number' ? loc.height : 0;
        const lengthZ = typeof loc.depth === 'number' ? loc.depth : 0;
        const geom = new THREE.BoxGeometry(widthX, heightY, lengthZ);
        const makeTransparent = (!!role) || (baseType === 'Cassette');
        const opacity = role ? 0.2 : ((baseType === 'Cassette') ? 0.4 : 1.0);
        const mat = new THREE.MeshLambertMaterial({ color: intColor(color), transparent: makeTransparent, opacity: opacity });
        const mesh = new THREE.Mesh(geom, mat);
        try { const edges = new THREE.EdgesGeometry(geom); const lineMat = new THREE.LineBasicMaterial({ color: intColor(borderColor) }); mesh.add(new THREE.LineSegments(edges, lineMat)); } catch (e) { }
    const px = (loc.x || 0) + widthX / 2;
    const pz = (loc.y || 0) + lengthZ / 2;
    const py = ((typeof loc.z === 'number' ? loc.z : 0)) + (heightY / 2);
    mesh.position.set(px, py, -pz);
        mesh.userData = { id: loc.id, type: 'location', role: role };
        return mesh;
    }

    function createRobotMesh(pgRef, r) { const n = normRobot(r); const c = n.robotType === 'Logistics' ? (pgRef.themeColors && pgRef.themeColors.primary != null ? pgRef.themeColors.primary : 0x00aaff) : (pgRef.themeColors && pgRef.themeColors.secondary != null ? pgRef.themeColors.secondary : 0xff8800); const geom = new THREE.CylinderGeometry(10, 10, 18, 16); const mat = new THREE.MeshPhongMaterial({ color: intColor(c), shininess: 80 }); const mesh = new THREE.Mesh(geom, mat); mesh.position.set((n.x || 0), 9, -(n.y || 0)); mesh.castShadow = true; mesh.receiveShadow = true; mesh.userData = { id: n.id, type: 'robot' }; return mesh; }

    function initThree(pgRef, initialLocations) {
        const parent = pgRef.canvas && pgRef.canvas.nodeType === 1 ? pgRef.canvas : document.body;
        const w = parent.clientWidth || window.innerWidth; const h = parent.clientHeight || window.innerHeight;
        const canvas = document.createElement('canvas'); canvas.id = 'threeCanvas'; Object.assign(canvas.style, { position: 'absolute', top: '0', left: '0', width: '100%', height: '100%', display: 'block', zIndex: '0' }); if (getComputedStyle(parent).position === 'static') parent.style.position = 'relative'; parent.appendChild(canvas);
        const renderer = new THREE.WebGLRenderer({ canvas: canvas, antialias: true, alpha: true }); renderer.setPixelRatio(window.devicePixelRatio || 1); renderer.setSize(w, h); if (pgRef.themeColors && pgRef.themeColors.background != null) { renderer.setClearColor(new THREE.Color(intColor(pgRef.themeColors.background)), 1); }
        const scene = new THREE.Scene(); const camera = new THREE.PerspectiveCamera(45, w / h, 1, 10000); camera.position.set(600, 600, 600); camera.lookAt(0, 0, 0);
        let controls = null; if (THREE.OrbitControls) { controls = new THREE.OrbitControls(camera, renderer.domElement); controls.enableDamping = true; controls.dampingFactor = 0.08; controls.minDistance = 10; controls.maxDistance = 10000; controls.maxPolarAngle = Math.PI * 0.49; if (THREE.MOUSE) { controls.mouseButtons = { LEFT: THREE.MOUSE.PAN, MIDDLE: THREE.MOUSE.DOLLY, RIGHT: THREE.MOUSE.ROTATE }; } controls.target.set(0, 0, 0); controls.update(); }
        scene.add(new THREE.AmbientLight(0xffffff, 0.6)); const dir = new THREE.DirectionalLight(0xffffff, 0.8); dir.position.set(500, 800, 400); scene.add(dir);
        const grid = new THREE.GridHelper(4000, 80, 0x666666, 0xcccccc); grid.position.y = -0.01; scene.add(grid);
        const locationMeshes = new Map(); const robotMeshes = new Map();
        if (Array.isArray(initialLocations)) {
            try { for (let i = 0; i < initialLocations.length; i++) { const m = createLocationMesh(pgRef, initialLocations[i]); if (m) { scene.add(m); const nl = normLoc(initialLocations[i]); locationMeshes.set(nl.id, m); } } } catch (e) { }
        }
        // Fit camera
        (function () { if (!Array.isArray(initialLocations) || initialLocations.length === 0) return; let minX = Infinity, minZ = Infinity, maxX = -Infinity, maxZ = -Infinity; for (let i = 0; i < initialLocations.length; i++) { const l = normLoc(initialLocations[i]); const x0 = l.x || 0, z0 = l.y || 0, x1 = x0 + (l.width || 0), z1 = z0 + (l.depth || 0); if (x0 < minX) minX = x0; if (z0 < minZ) minZ = z0; if (x1 > maxX) maxX = x1; if (z1 > maxZ) maxZ = z1; } if (!isFinite(minX) || !isFinite(minZ) || !isFinite(maxX) || !isFinite(maxZ)) return; const cx = (minX + maxX) / 2, cz = (minZ + maxZ) / 2; const span = Math.max(100, Math.max(maxX - minX, maxZ - minZ)); const dist = span * 1.4; if (controls) { controls.target.set(cx, 0, -cz); camera.position.set(cx + dist, dist, -cz + dist); camera.lookAt(controls.target); controls.update(); } else { camera.position.set(cx + dist, dist, -cz + dist); camera.lookAt(new THREE.Vector3(cx, 0, -cz)); } })();
        // Mouse XYZ overlay
        const raycaster = new THREE.Raycaster(); const pointer = new THREE.Vector2(); const ground = new THREE.Plane(new THREE.Vector3(0, 1, 0), 0); const onMove = (e) => { const rect = renderer.domElement.getBoundingClientRect(); pointer.set(((e.clientX - rect.left) / rect.width) * 2 - 1, -((e.clientY - rect.top) / rect.height) * 2 + 1); raycaster.setFromCamera(pointer, camera); const hit = new THREE.Vector3(); if (raycaster.ray.intersectPlane(ground, hit)) { const gx = Math.round(hit.x), gy = Math.round(-hit.z), gz = Math.round(hit.y); const el = document.getElementById('mouseXYZ'); if (el) { el.textContent = `X: ${gx}  Y: ${gy}  Z: ${gz}`; } } }; renderer.domElement.addEventListener('mousemove', onMove);
        // Resize
        const onResize = () => { const nw = parent.clientWidth || window.innerWidth; const nh = parent.clientHeight || window.innerHeight; renderer.setSize(nw, nh); camera.aspect = nw / nh; camera.updateProjectionMatrix(); };
        window.addEventListener('resize', onResize);
        // Animate
        function animate() { pg.three && (pg.three.animReq = requestAnimationFrame(animate)); if (controls) controls.update(); renderer.render(scene, camera); }
        pg.three = { renderer, scene, camera, controls, canvas, locationMeshes, robotMeshes, onResize, onMove, animReq: 0 };
        animate();
    }

    pg.init3D = function (containerId, locations) { try { const el = document.getElementById(containerId); if (!el) return false; pg.canvas = el; initThree(pg, Array.isArray(locations) ? locations : []); return true; } catch (e) { console.warn('init3D failed', e); return false; } };
    pg.addLocation3D = function (loc) { if (!pg.three) return; try { const m = createLocationMesh(pg, loc); if (m) { pg.three.scene.add(m); const nl = normLoc(loc); pg.three.locationMeshes.set(nl.id, m); } } catch (e) { console.warn('addLocation3D failed', e); } };
    pg.loadRobots3D = function (robots) { if (!pg.three || !Array.isArray(robots)) return; try { robots.forEach(r => { const m = createRobotMesh(pg, r); if (m) { pg.three.scene.add(m); const nr = normRobot(r); pg.three.robotMeshes.set(nr.id, m); } }); } catch (e) { console.warn('loadRobots3D failed', e); } };
    pg.updateRobot3D = function (robot) { if (!pg.three || !robot) return; const nr = normRobot(robot); let m = pg.three.robotMeshes.get(nr.id); if (!m) { try { m = createRobotMesh(pg, robot); if (m) { pg.three.scene.add(m); pg.three.robotMeshes.set(nr.id, m); } } catch (e) { return; } } else { try { m.position.set((nr.x || 0), m.position.y, -(nr.y || 0)); } catch (e) { } } };
})();
