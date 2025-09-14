// Three.js scene layer that can run standalone or alongside pixiGame
(function(){
  if (!window) return;
  const hadPixi = !!window.pixiGame;
  const pg = window.pixiGame || {};
  window.pixiGame = pg; // ensure a single namespace for compatibility

  // Preserve originals if present
  const _orig = {
    init: (typeof pg.init === 'function') ? pg.init : null,
    loadRobots: (typeof pg.loadRobots === 'function') ? pg.loadRobots : null,
    updateRobot: (typeof pg.updateRobot === 'function') ? pg.updateRobot : null,
    removeRobot: (typeof pg.removeRobot === 'function') ? pg.removeRobot : null,
    addLocation: (typeof pg.addLocation === 'function') ? pg.addLocation : null,
    updateLocation: (typeof pg.updateLocation === 'function') ? pg.updateLocation : null,
    removeLocation: (typeof pg.removeLocation === 'function') ? pg.removeLocation : null,
    refreshThemeStyles: (typeof pg.refreshThemeStyles === 'function') ? pg.refreshThemeStyles : function(){},
    setTheme: (typeof pg.setTheme === 'function') ? pg.setTheme : null
  };

  // 3D state
  pg.threeEnabled = false;
  pg.three = null; // { renderer, scene, camera, controls, canvas, locationMeshes, robotMeshes, animReqId, onResize, onKeyDown, onKeyUp }

  // Helpers
  function intToThreeColor(n){
    if (typeof n === 'number') return (n >>> 0) & 0xffffff;
    try { return new THREE.Color(n).getHex(); } catch(e){ return 0xffffff; }
  }
  function inferMarkerRole(location){
    if (!location || location.locationType !== 'Marker') return null;
    const idLower = (location.id || '').toString().toLowerCase();
    const nameLower = (location.name || '').toString().toLowerCase();
    if (nameLower.indexOf('move area') !== -1 || idLower.indexOf('move.') === 0) return 'MoveArea';
    if (nameLower.indexOf('area') !== -1 || (idLower.indexOf('a') === 0 && idLower.indexOf('.') === -1)) return 'Area';
    if (nameLower.indexOf('set') !== -1 || idLower.indexOf('.set') !== -1) return 'Set';
    if (nameLower.indexOf('stocker') !== -1 || idLower.indexOf('st') === 0) return 'Stocker';
    return null;
  }

  function createLocationMesh(pgRef, location){
    if (!location || !pgRef.three) return null;
    const role = inferMarkerRole(location);
    const colorMap = {
      'Cassette': (pgRef.themeColors.warning != null ? pgRef.themeColors.warning : 0xffc107),
      'Tray': (pgRef.themeColors.success != null ? pgRef.themeColors.success : 0x2e7d32),
      'Memory': (pgRef.themeColors.info != null ? pgRef.themeColors.info : 0x0288d1),
      'Marker': (pgRef.themeColors.secondary != null ? pgRef.themeColors.secondary : 0x9c27b0)
    };
    const baseType = location.locationType;
    let color = colorMap[baseType] || 0x888888;
    if (role) {
      const regionColorMap = {
        'Area': (pgRef.themeColors.success != null ? pgRef.themeColors.success : 0x2e7d32),
        'Stocker': (pgRef.themeColors.warning != null ? pgRef.themeColors.warning : 0xffc107),
        'Set': (pgRef.themeColors.secondary != null ? pgRef.themeColors.secondary : 0x9c27b0),
        'MoveArea': (pgRef.themeColors.info != null ? pgRef.themeColors.info : 0x0288d1)
      };
      color = regionColorMap[role] || color;
    }
    const borderColor = (location.status === 'Occupied') ? (pgRef.themeColors.textPrimary != null ? pgRef.themeColors.textPrimary : 0x000000) : (pgRef.themeColors.textSecondary != null ? pgRef.themeColors.textSecondary : 0x666666);

    const width = (typeof location.width === 'number' && location.width > 0) ? location.width * 2 : 20;
    const depth = (typeof location.height === 'number' && location.height > 0) ? location.height * 2 : 20;
    const isRegion = !!role;
    const thickness = isRegion ? 2 : 20;
    let geom = new THREE.BoxGeometry(width, thickness, depth);
    const mat = new THREE.MeshLambertMaterial({ color: intToThreeColor(color), transparent: isRegion, opacity: isRegion ? 0.6 : 1.0 });
    const mesh = new THREE.Mesh(geom, mat);
    try {
      const edges = new THREE.EdgesGeometry(geom);
      const lineMat = new THREE.LineBasicMaterial({ color: intToThreeColor(borderColor) });
      const line = new THREE.LineSegments(edges, lineMat);
      mesh.add(line);
    } catch(e) {}
    const px = (location.x || 0) * 2 + width / 2;
    const py = (location.y || 0) * 2 + depth / 2;
    mesh.position.set(px, thickness / 2, -py);
    mesh.userData = { id: location.id, type: 'location', role: role };
    return mesh;
  }

  function createRobotMesh(pgRef, robot){
    if (!pgRef.three) return null;
    const isLogistics = robot.robotType === 'Logistics';
    const color = isLogistics ? (pgRef.themeColors.primary != null ? pgRef.themeColors.primary : 0x00aaff) : (pgRef.themeColors.secondary != null ? pgRef.themeColors.secondary : 0xff8800);
    const radius = 10;
    const height = 18;
    let geom;
    try { geom = new THREE.CylinderGeometry(radius, radius, height, 16); }
    catch(e){ geom = new THREE.BoxGeometry(radius * 2, height, radius * 2); }
    const mat = new THREE.MeshPhongMaterial({ color: intToThreeColor(color), shininess: 80 });
    const mesh = new THREE.Mesh(geom, mat);
    const px = (robot.x || 0) * 2;
    const py = (robot.y || 0) * 2;
    mesh.position.set(px, height / 2, -py);
    mesh.castShadow = true;
    mesh.receiveShadow = true;
    mesh.userData = { id: robot.id, type: 'robot' };
    return mesh;
  }

  function initThree(pgRef, initialLocations){
    if (!pgRef.canvas) return;
    const isCanvasEl = (pgRef.canvas && pgRef.canvas.tagName === 'CANVAS');
    const parent = (!isCanvasEl && pgRef.canvas && pgRef.canvas.nodeType === 1) ? pgRef.canvas : (pgRef.canvas.parentElement || document.body);
    const width = parent.clientWidth || window.innerWidth;
    const height = parent.clientHeight || window.innerHeight;

    const threeCanvas = document.createElement('canvas');
    threeCanvas.id = 'threeCanvas';
    threeCanvas.style.position = 'absolute';
    threeCanvas.style.top = '0';
    threeCanvas.style.left = '0';
    threeCanvas.style.width = '100%';
    threeCanvas.style.height = '100%';
    threeCanvas.style.display = 'block';
    threeCanvas.style.zIndex = '0';
    if (getComputedStyle(parent).position === 'static') parent.style.position = 'relative';
    parent.appendChild(threeCanvas);

    const renderer = new THREE.WebGLRenderer({ canvas: threeCanvas, antialias: true, alpha: true });
    renderer.setPixelRatio(window.devicePixelRatio || 1);
    renderer.setSize(width, height);
    renderer.setClearColor(new THREE.Color((pgRef.themeColors && pgRef.themeColors.background != null) ? pgRef.themeColors.background : 0xffffff), 1);

    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(45, width / height, 1, 10000);
    camera.position.set(600, 600, 600);
    camera.lookAt(0, 0, 0);

    let controls = null;
    try {
      if (THREE.OrbitControls) {
        controls = new THREE.OrbitControls(camera, renderer.domElement);
        controls.enableDamping = true;
        controls.dampingFactor = 0.08;
        controls.minDistance = 100;
        controls.maxDistance = 4000;
        controls.maxPolarAngle = Math.PI * 0.49;
        controls.target.set(0, 0, 0);
        controls.update();
      }
    } catch(e) {}

    const ambient = new THREE.AmbientLight(0xffffff, 0.6);
    scene.add(ambient);
    const dir = new THREE.DirectionalLight(0xffffff, 0.8);
    dir.position.set(500, 800, 400);
    scene.add(dir);

    const grid = new THREE.GridHelper(4000, 80, 0x666666, 0xcccccc);
    grid.position.y = -0.01;
    scene.add(grid);

    const locationMeshes = new Map();
    const robotMeshes = new Map();
    if (Array.isArray(initialLocations)){
      for (let i=0;i<initialLocations.length;i++){
        const loc = initialLocations[i];
        const mesh = createLocationMesh(pgRef, loc);
        if (mesh){ scene.add(mesh); locationMeshes.set(loc.id, mesh); }
      }
    }

    // Keyboard navigation
    const keys = {};
    const onKeyDown = (e) => { keys[e.code] = true; };
    const onKeyUp = (e) => { keys[e.code] = false; };
    window.addEventListener('keydown', onKeyDown);
    window.addEventListener('keyup', onKeyUp);

    // Initialize three state object before starting animation loop
    pgRef.three = { renderer, scene, camera, controls, canvas: threeCanvas, locationMeshes, robotMeshes, animReqId: 0, onResize: null, onKeyDown, onKeyUp };

    const onResize = () => {
      const w = parent.clientWidth || window.innerWidth;
      const h = parent.clientHeight || window.innerHeight;
      renderer.setSize(w, h);
      camera.aspect = w / h;
      camera.updateProjectionMatrix();
    };
    pgRef.three.onResize = onResize;
    window.addEventListener('resize', onResize);

    let lastTime = (typeof performance !== 'undefined') ? performance.now() : Date.now();
    const animate = function(){
      // Guard in case dispose clears pgRef.three during a frame
      if (!pgRef.three) {
        return;
      }
      pgRef.three.animReqId = requestAnimationFrame(animate);

      // Camera keyboard movement
      const now = (typeof performance !== 'undefined') ? performance.now() : Date.now();
      const dt = Math.max(0.001, Math.min(0.033, (now - lastTime) / 1000));
      lastTime = now;
      let moveSpeed = 300 * dt; // units per second
      if (keys['ShiftLeft'] || keys['ShiftRight']) moveSpeed *= 2;
      const forward = new THREE.Vector3();
      camera.getWorldDirection(forward);
      forward.y = 0; forward.normalize();
      const right = new THREE.Vector3();
      right.crossVectors(forward, new THREE.Vector3(0,1,0)).normalize();
      const up = new THREE.Vector3(0,1,0);
      const delta = new THREE.Vector3();
      if (keys['KeyW'] || keys['ArrowUp']) delta.addScaledVector(forward, moveSpeed);
      if (keys['KeyS'] || keys['ArrowDown']) delta.addScaledVector(forward, -moveSpeed);
      if (keys['KeyD'] || keys['ArrowRight']) delta.addScaledVector(right, moveSpeed);
      if (keys['KeyA'] || keys['ArrowLeft']) delta.addScaledVector(right, -moveSpeed);
      if (keys['KeyE']) delta.addScaledVector(up, moveSpeed);
      if (keys['KeyQ']) delta.addScaledVector(up, -moveSpeed);
      if (delta.lengthSq() > 0) {
        camera.position.add(delta);
        if (controls && controls.target) controls.target.add(delta);
      }

      if (controls && controls.update) controls.update();
      renderer.render(scene, camera);
    };
    animate();
  }

  function disposeThree(pgRef){
    if (!pgRef.three) return;
    try { window.removeEventListener('resize', pgRef.three.onResize); } catch(e){}
    try { if (pgRef.three.onKeyDown) window.removeEventListener('keydown', pgRef.three.onKeyDown); } catch(e){}
    try { if (pgRef.three.onKeyUp) window.removeEventListener('keyup', pgRef.three.onKeyUp); } catch(e){}
    try { if (pgRef.three.animReqId) cancelAnimationFrame(pgRef.three.animReqId); } catch(e){}
    try {
      pgRef.three.scene.traverse(obj => {
        if (obj.isMesh){
          if (obj.geometry) obj.geometry.dispose();
          if (obj.material){
            if (Array.isArray(obj.material)) obj.material.forEach(m => m.dispose && m.dispose());
            else if (obj.material.dispose) obj.material.dispose();
          }
        }
      });
    } catch(e){}
    try { if (pgRef.three.renderer) pgRef.three.renderer.dispose(); } catch(e){}
    try { if (pgRef.three.canvas && pgRef.three.canvas.parentElement) pgRef.three.canvas.parentElement.removeChild(pgRef.three.canvas); } catch(e){}
    pgRef.three = null;
  }

  // Public API: enable or disable 3D mode
  pg.enable3D = function(enable, opts){
    const want = !!enable;
    if (want === pg.threeEnabled) return;
    if (want){
      if (typeof THREE === 'undefined') { console.warn('THREE not loaded'); return; }
      const initialLocations = (Array.isArray(opts)) ? opts : ((opts && Array.isArray(opts.locations)) ? opts.locations : []);
      initThree(pg, initialLocations);
      if (pg.canvas && pg.canvas.tagName === 'CANVAS') pg.canvas.style.display = 'none';
      // Mirror existing robots into 3D if Pixi already loaded them
      try {
        if (pg.robotObjects && pg.three) {
          pg.robotObjects.forEach((group, id) => {
            const robot = { id: id, robotType: (group && group._robotType) ? group._robotType : 'Logistics', x: Math.round(((group && group.x) ? group.x : 0) / 2), y: Math.round(((group && group.y) ? group.y : 0) / 2) };
            const mesh = createRobotMesh(pg, robot);
            if (mesh){ pg.three.scene.add(mesh); pg.three.robotMeshes.set(id, mesh); }
          });
        }
      } catch(e){}
      pg.threeEnabled = true;
    } else {
      if (pg.canvas && pg.canvas.tagName === 'CANVAS') pg.canvas.style.display = 'block';
      disposeThree(pg);
      pg.threeEnabled = false;
    }
  };

  // Initialize 3D directly inside a container element (no Pixi/2D required)
  pg.init3D = function(containerId, locations){
    try {
      const el = document.getElementById(containerId);
      if (!el) { console.warn('init3D: container not found', containerId); return false; }
      pg.canvas = el; // treat container as our anchor
      pg.enable3D(true, { locations });
      return true;
    } catch(e){ console.warn('init3D failed', e); return false; }
  };

  // 3D-only helpers (do not call 2D/original methods)
  pg.addLocation3D = function(location){
    if (!pg.three) return;
    try {
      const mesh = createLocationMesh(pg, location);
      if (mesh){ pg.three.scene.add(mesh); pg.three.locationMeshes.set(location.id, mesh); }
    } catch(e){ console.warn('addLocation3D failed', e); }
  };
  pg.updateLocation3D = function(location){
    if (!pg.three) return;
    try {
      const existing = pg.three.locationMeshes.get(location.id);
      if (existing){ try { pg.three.scene.remove(existing); } catch(e){} }
      const mesh = createLocationMesh(pg, location);
      if (mesh){ pg.three.scene.add(mesh); pg.three.locationMeshes.set(location.id, mesh); }
    } catch(e){ console.warn('updateLocation3D failed', e); }
  };
  pg.clearLocations3D = function(){
    if (!pg.three) return;
    try {
      pg.three.locationMeshes.forEach((mesh, id) => {
        try { pg.three.scene.remove(mesh); } catch(e){}
        try { if (mesh.geometry) mesh.geometry.dispose(); if (mesh.material && mesh.material.dispose) mesh.material.dispose(); } catch(e){}
      });
      pg.three.locationMeshes.clear();
    } catch(e){}
  };

  // Robots: 3D-only helpers
  pg.loadRobots3D = function(robots){
    if (!pg.three || !Array.isArray(robots)) return;
    try {
      robots.forEach(r => {
        const mesh = createRobotMesh(pg, r);
        if (mesh){ pg.three.scene.add(mesh); pg.three.robotMeshes.set(r.id, mesh); }
      });
    } catch(e){ console.warn('loadRobots3D failed', e); }
  };
  pg.updateRobot3D = function(robot){
    if (!pg.three || !robot) return;
    let mesh = pg.three.robotMeshes.get(robot.id);
    if (!mesh){
      try {
        mesh = createRobotMesh(pg, robot);
        if (mesh){ pg.three.scene.add(mesh); pg.three.robotMeshes.set(robot.id, mesh); }
      } catch(e){ return; }
    } else {
      try {
        const px = (robot.x || 0) * 2;
        const py = (robot.y || 0) * 2;
        mesh.position.set(px, mesh.position.y, -py);
      } catch(e){}
    }
  };
  pg.removeRobot3D = function(robotId){
    if (!pg.three) return;
    const mesh = pg.three.robotMeshes.get(robotId);
    if (!mesh) return;
    try { pg.three.scene.remove(mesh); } catch(e){}
    try { if (mesh.geometry) mesh.geometry.dispose(); if (mesh.material && mesh.material.dispose) mesh.material.dispose(); } catch(e){}
    pg.three.robotMeshes.delete(robotId);
  };

  // Wrap init to accept options and store canvas (works even if no Pixi)
  pg.init = function(canvasId, locations, options){
    let ok = true;
    if (_orig.init){
      ok = _orig.init.call(pg, canvasId, locations);
      try {
        if (pg.app && pg.app.view) {
          pg.canvas = pg.app.view;
        } else {
          pg.canvas = document.getElementById(canvasId);
        }
      } catch(e){}
    } else {
      try { pg.canvas = document.getElementById(canvasId); } catch(e){}
    }
    if (options && options.mode === '3d'){
      try { pg.enable3D(true, { locations }); } catch(e){ console.warn('enable3D failed', e); }
    }
    return ok;
  };

  // Wrap robot/location methods to mirror into 3D when enabled
  pg.loadRobots = function(robots){
    if (_orig.loadRobots) _orig.loadRobots.call(pg, robots);
    if (pg.threeEnabled && pg.three && Array.isArray(robots)){
      robots.forEach(r => {
        const mesh = createRobotMesh(pg, r);
        if (mesh){ pg.three.scene.add(mesh); pg.three.robotMeshes.set(r.id, mesh); }
      });
    }
  };

  pg.updateRobot = function(robot){
    if (_orig.updateRobot) _orig.updateRobot.call(pg, robot);
    if (pg.threeEnabled && pg.three){
      const mesh = pg.three.robotMeshes.get(robot.id);
      if (mesh){
        const px = (robot.x || 0) * 2;
        const py = (robot.y || 0) * 2;
        mesh.position.set(px, mesh.position.y, -py);
      }
    }
  };

  pg.removeRobot = function(robotId){
    if (_orig.removeRobot) _orig.removeRobot.call(pg, robotId);
    if (pg.threeEnabled && pg.three){
      const mesh = pg.three.robotMeshes.get(robotId);
      if (mesh){
        try { pg.three.scene.remove(mesh); } catch(e){}
        try { if (mesh.geometry) mesh.geometry.dispose(); if (mesh.material && mesh.material.dispose) mesh.material.dispose(); } catch(e){}
        pg.three.robotMeshes.delete(robotId);
      }
    }
  };

  pg.addLocation = function(location){
    if (_orig.addLocation) _orig.addLocation.call(pg, location);
    if (pg.threeEnabled && pg.three){
      const mesh = createLocationMesh(pg, location);
      if (mesh){ pg.three.scene.add(mesh); pg.three.locationMeshes.set(location.id, mesh); }
    }
  };

  pg.updateLocation = function(location){
    if (_orig.updateLocation) _orig.updateLocation.call(pg, location);
    if (pg.threeEnabled && pg.three){
      const existing = pg.three.locationMeshes.get(location.id);
      if (existing){ try { pg.three.scene.remove(existing); } catch(e){} }
      const mesh = createLocationMesh(pg, location);
      if (mesh){ pg.three.scene.add(mesh); pg.three.locationMeshes.set(location.id, mesh); }
    }
  };

  pg.removeLocation = function(locationId){
    if (_orig.removeLocation) _orig.removeLocation.call(pg, locationId);
    if (pg.threeEnabled && pg.three){
      const mesh = pg.three.locationMeshes.get(locationId);
      if (mesh){
        try { pg.three.scene.remove(mesh); } catch(e){}
        try { if (mesh.geometry) mesh.geometry.dispose(); if (mesh.material && mesh.material.dispose) mesh.material.dispose(); } catch(e){}
        pg.three.locationMeshes.delete(locationId);
      }
    }
  };

  pg.refreshThemeStyles = function(){
    try { if (_orig.refreshThemeStyles) _orig.refreshThemeStyles.call(pg); } catch(e){}
    if (pg.threeEnabled && pg.three){
      try {
        if (pg.themeColors && pg.themeColors.background != null){
          pg.three.renderer.setClearColor(new THREE.Color(intToThreeColor(pg.themeColors.background)), 1);
        }
      } catch(e){}
    }
  };

  pg.setTheme = function(themeDto){
    if (_orig.setTheme) {
      try { _orig.setTheme.call(pg, themeDto); } catch(e){}
    } else {
      // Fallback: store parsed theme colors
      const t = themeDto || {};
      pg.themeColors = {
        primary: intToThreeColor(t.primary),
        secondary: intToThreeColor(t.secondary),
        info: intToThreeColor(t.info),
        success: intToThreeColor(t.success),
        warning: intToThreeColor(t.warning),
        textPrimary: intToThreeColor(t.textPrimary),
        textSecondary: intToThreeColor(t.textSecondary),
        surface: intToThreeColor(t.surface),
        background: intToThreeColor(t.background)
      };
    }
    if (pg.threeEnabled && pg.three){
      try {
        if (pg.themeColors && pg.themeColors.background != null){
          pg.three.renderer.setClearColor(new THREE.Color(intToThreeColor(pg.themeColors.background)), 1);
        }
      } catch(e){}
    }
  };
})();
