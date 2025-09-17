// Minimal Three.js only scene manager (no PixiJS)
(function () {
    if (!window) {
        return;
    }

    // Expose as window.nexus3d
    const nexus3dInstance = window.nexus3d || (window.nexus3d = {});

    function convertToIntegerColor(colorValue) {
        if (typeof colorValue === 'number') {
            return (colorValue >>> 0) & 0xffffff;
        }

        try {
            return new THREE.Color(colorValue).getHex();
        } catch (error) {
            return 0xffffff;
        }
    }

    function getValidNumber(primaryValue, secondaryValue, defaultValue) {
        if (typeof primaryValue === 'number' && isFinite(primaryValue)) {
            return primaryValue;
        }

        if (typeof secondaryValue === 'number' && isFinite(secondaryValue)) {
            return secondaryValue;
        }

        return defaultValue;
    }

    function normalizeLocationData(locationData) {
        if (!locationData) {
            return {
                id: '',
                name: '',
                locationType: 'Marker',
                status: 'Available',
                parentId: '',
                isVisible: true,
                isRelativePosition: false,
                rotate: 0,
                x: 0,
                y: 0,
                z: 0,
                width: 0,
                height: 0,
                depth: 0,
                markerRole: ''
            };
        }

        const locationId = locationData.id != null ? locationData.id : locationData.Id;
        const locationName = locationData.name != null ? locationData.name : locationData.Name;
        const locationType = locationData.locationType != null ? locationData.locationType : locationData.LocationType;
        const locationStatus = locationData.status != null ? locationData.status : locationData.Status;
        const parentId = locationData.parentId != null ? locationData.parentId : locationData.ParentId;
        const isVisible = (locationData.isVisible != null ? locationData.isVisible : locationData.IsVisible);
        const isRelativePosition = (locationData.isRelativePosition != null ? locationData.isRelativePosition : locationData.IsRelativePosition) || false;
        const xCoordinate = getValidNumber(locationData.x, locationData.X, 0);
        const yCoordinate = getValidNumber(locationData.y, locationData.Y, 0);
        const zCoordinate = getValidNumber(locationData.z, locationData.Z, 0);
        const locationWidth = getValidNumber(locationData.width, locationData.Width, 0);
        const locationHeight = getValidNumber(locationData.height, locationData.Height, 0);
        const locationDepth = getValidNumber(locationData.depth, locationData.Depth, 0);
        const rotateXDeg = getValidNumber(locationData.rotateX, locationData.RotateX, 0);
        const rotateYDeg = getValidNumber(locationData.rotateY, locationData.RotateY, 0);
        const rotateZDeg = getValidNumber(locationData.rotateZ, locationData.RotateZ, 0);
        // backward compat: single rotate as yaw (Y)
        let legacyYaw = getValidNumber(locationData.rotate, locationData.Rotate, 0);
        const finalRotX = rotateXDeg || 0;
        const finalRotY = (rotateYDeg || legacyYaw || 0);
        const finalRotZ = rotateZDeg || 0;
        const markerRole = (locationData.markerRole != null ? locationData.markerRole : locationData.MarkerRole) || '';
        const currentItemId = (locationData.currentItemId != null ? locationData.currentItemId : locationData.CurrentItemId) || '';

        return {
            id: locationId,
            name: locationName,
            locationType,
            status: locationStatus,
            parentId: parentId || '',
            isVisible: (isVisible === false ? false : true),
            isRelativePosition: !!isRelativePosition,
            x: xCoordinate,
            y: yCoordinate,
            z: zCoordinate,
            width: locationWidth,
            height: locationHeight,
            depth: locationDepth,
            markerRole,
            currentItemId,
            rotateX: finalRotX,
            rotateY: finalRotY,
            rotateZ: finalRotZ
        };
    }

    // --- Dimensions ---
    function normalizeDimension(value) {
        const w = getValidNumber(value && (value.width != null ? value.width : value.Width), null, 0);
        const h = getValidNumber(value && (value.height != null ? value.height : value.Height), null, 0);
        const d = getValidNumber(value && (value.depth != null ? value.depth : value.Depth), null, 0);
        return { width: w, height: h, depth: d };
    }

    function getTransportDims(nexus3dRef, locationType) {
        const dims = (nexus3dRef && nexus3dRef.dimensions && nexus3dRef.dimensions.transports) ? nexus3dRef.dimensions.transports : null;
        if (!dims) return null;
        const key = (locationType || '').toString().toLowerCase();
        if (key === 'cassette') return normalizeDimension(dims.cassette);
        if (key === 'tray') return normalizeDimension(dims.tray);
        if (key === 'memory') return normalizeDimension(dims.memory);
        return null;
    }

    function normalizeRobotData(robotData) {
        if (!robotData) {
            return {
                id: '',
                robotType: 'Logistics',
                x: 0,
                y: 0,
                z: 0
            };
        }

        const robotId = robotData.id != null ? robotData.id : robotData.Id;
        const robotType = robotData.robotType != null ? robotData.robotType : robotData.RobotType;
        const xPosition = getValidNumber(robotData.x, robotData.X, 0);
        const yPosition = getValidNumber(robotData.y, robotData.Y, 0);
        const zPosition = getValidNumber(robotData.z, robotData.Z, 0);

        return {
            id: robotId,
            robotType,
            x: xPosition,
            y: yPosition,
            z: zPosition
        };
    }

    function determineRoleFromLocationId(locationData) {
        const normalizedLocation = normalizeLocationData(locationData);

        if (!normalizedLocation || normalizedLocation.locationType !== 'Marker') {
            return null;
        }

        return null;
    }

    function selectLocationRole(locationData) {
        const normalizedLocation = normalizeLocationData(locationData);

        if (normalizedLocation.markerRole) {
            return normalizedLocation.markerRole.toString().toLowerCase();
        }

        return determineRoleFromLocationId(normalizedLocation);
    }

    function createLocationMeshObject(nexus3dRef, locationData) {
        const normalizedLocation = normalizeLocationData(locationData);
        const baseLocationType = normalizedLocation.locationType;

        const locationTypeColorMap = {
            'Cassette': nexus3dRef.themeColors && nexus3dRef.themeColors.background != null ? nexus3dRef.themeColors.background : 0xffc107,
            'Tray': nexus3dRef.themeColors && nexus3dRef.themeColors.background != null ? nexus3dRef.themeColors.background : 0x2e7d32,
            'Memory': nexus3dRef.themeColors && nexus3dRef.themeColors.info != null ? nexus3dRef.themeColors.info : 0x0288d1,
            'Marker': nexus3dRef.themeColors && nexus3dRef.themeColors.success != null ? nexus3dRef.themeColors.success : 0x9c27b0
        };

        let selectedColor = locationTypeColorMap[baseLocationType] || 0x888888;
        const locationRole = selectLocationRole(normalizedLocation);

        if (locationRole) {
            const roleColorMap = {
                'area': nexus3dRef.themeColors && nexus3dRef.themeColors.success != null ? nexus3dRef.themeColors.success : 0x2e7d32,
                'stocker': nexus3dRef.themeColors && nexus3dRef.themeColors.background != null ? nexus3dRef.themeColors.background : 0xffc107,
                'set': nexus3dRef.themeColors && nexus3dRef.themeColors.primary != null ? nexus3dRef.themeColors.primary : 0x3f51b5,
                'movearea': nexus3dRef.themeColors && nexus3dRef.themeColors.success != null ? nexus3dRef.themeColors.success : 0x0288d1
            };
            selectedColor = roleColorMap[locationRole] || selectedColor;
        }

        let edgeColor;
        if (normalizedLocation.status === 'Occupied') {
            edgeColor = nexus3dRef.themeColors && nexus3dRef.themeColors.textPrimary != null ? nexus3dRef.themeColors.textPrimary : 0x000000;
        } else {
            edgeColor = nexus3dRef.themeColors && nexus3dRef.themeColors.textSecondary != null ? nexus3dRef.themeColors.textSecondary : 0x666666;
        }

        const meshWidth = typeof normalizedLocation.width === 'number' ? normalizedLocation.width : 0;
        const meshHeight = typeof normalizedLocation.height === 'number' ? normalizedLocation.height : 0;
        const meshDepth = typeof normalizedLocation.depth === 'number' ? normalizedLocation.depth : 0;

        const boxGeometry = new THREE.BoxGeometry(meshWidth, meshHeight, meshDepth);
        const shouldMakeTransparent = (!!locationRole) || (baseLocationType === 'Cassette');

        let materialOpacity;
        switch (true) {
            case !!locationRole:
                materialOpacity = 0.18; // region roles: slightly transparent
                break;
            case baseLocationType === 'Cassette':
                materialOpacity = 0.30; // cassette: more transparent to see inner item
                break;
            case baseLocationType === 'Tray':
                materialOpacity = 0.5; // tray: semi-transparent
                break;
            default:
                materialOpacity = 1.0;
                break;
        }

        const isTransparent = (materialOpacity < 1.0) || shouldMakeTransparent;
        const meshMaterial = new THREE.MeshLambertMaterial({
            color: convertToIntegerColor(selectedColor),
            transparent: isTransparent,
            opacity: materialOpacity,
            depthWrite: !isTransparent
        });

        const locationMesh = new THREE.Mesh(boxGeometry, meshMaterial);

        try {
            const edgeGeometry = new THREE.EdgesGeometry(boxGeometry);
            const edgeMaterial = new THREE.LineBasicMaterial({ color: convertToIntegerColor(edgeColor) });
            locationMesh.add(new THREE.LineSegments(edgeGeometry, edgeMaterial));
        } catch (error) {
            // 에러 무시
        }

        const positionX = (normalizedLocation.x || 0) + meshWidth / 2;
        const positionZ = (normalizedLocation.y || 0) + meshDepth / 2;

        let positionY;
        if (typeof normalizedLocation.z === 'number') {
            positionY = normalizedLocation.z + (meshHeight / 2);
        } else {
            positionY = 0 + (meshHeight / 2);
        }

        locationMesh.position.set(positionX, positionY, -positionZ);
        try {
            const rx = (normalizedLocation.rotateX || 0) * Math.PI / 180.0;
            const ry = (normalizedLocation.rotateY || 0) * Math.PI / 180.0;
            const rz = (normalizedLocation.rotateZ || 0) * Math.PI / 180.0;
            locationMesh.rotation.set(rx, ry, rz);
        } catch { }
        locationMesh.userData = {
            id: normalizedLocation.id,
            type: 'location',
            role: locationRole,
            locationType: baseLocationType,
            status: normalizedLocation.status,
            parentId: normalizedLocation.parentId,
            isVisible: normalizedLocation.isVisible,
            isRelativePosition: normalizedLocation.isRelativePosition,
            currentItemId: normalizedLocation.currentItemId || ''
        };
        locationMesh.visible = (normalizedLocation.isVisible !== false);

        // If this location has an item, render an inner 3D box (transport) using dimension standards
        try {
            if ((baseLocationType === 'Cassette' || baseLocationType === 'Tray' || baseLocationType === 'Memory') && normalizedLocation.currentItemId) {
                const td = getTransportDims(nexus3dRef, baseLocationType);
                let innerWidth = meshWidth * 0.9;
                let innerHeight = meshHeight * 1;
                let innerDepth = meshDepth * 0.9;
                if (td) {
                    innerWidth = Math.min(td.width || innerWidth, meshWidth);
                    innerHeight = Math.min(td.height || innerHeight, meshHeight);
                    innerDepth = Math.min(td.depth || innerDepth, meshDepth);
                }
                innerWidth = Math.max(2, innerWidth);
                innerHeight = Math.max(2, innerHeight);
                innerDepth = Math.max(2, innerDepth);

                const itemGeom = new THREE.BoxGeometry(innerWidth, innerHeight, innerDepth);
                const itemColor = (nexus3dRef.themeColors && nexus3dRef.themeColors.error != null)
                    ? nexus3dRef.themeColors.error
                    : 0xff4444;
                const itemMat = new THREE.MeshPhongMaterial({ color: convertToIntegerColor(itemColor), transparent: true, shininess: 60, opacity : 0.5, depthWrite: true, depthTest: true });
                const itemMesh = new THREE.Mesh(itemGeom, itemMat);
                itemMesh.position.set(0, 0, 0);
                itemMesh.userData = { tag: 'item', itemId: normalizedLocation.currentItemId };
                try { itemMesh.renderOrder = (locationMesh.renderOrder || 0) + 1; } catch { }
                try {
                    const itemEdgeGeom = new THREE.EdgesGeometry(itemGeom);
                    const edgeCol = (nexus3dRef.themeColors && nexus3dRef.themeColors.textPrimary != null)
                        ? nexus3dRef.themeColors.textPrimary
                        : 0x000000;
                    const itemEdgeMat = new THREE.LineBasicMaterial({ color: convertToIntegerColor(edgeCol) });
                    itemMesh.add(new THREE.LineSegments(itemEdgeGeom, itemEdgeMat));
                } catch { }
                locationMesh.add(itemMesh);
                locationMesh.visible = true;
            }
        } catch { }

        return locationMesh;
    }

    function upsertCassetteItemMesh(nexus3dRef, parentMesh, normalizedLocation, boxSize, baseLocationType) {
        try {
            const hasItem = !!normalizedLocation.currentItemId;
            let existingItem = null;
            if (parentMesh && parentMesh.children && parentMesh.children.length > 0) {
                for (let idx = 0; idx < parentMesh.children.length; idx++) {
                    const c = parentMesh.children[idx];
                    if (c && c.userData && c.userData.tag === 'item') {
                        existingItem = c;
                        break;
                    }
                }
            }

            if (!hasItem && existingItem) {
                try {
                    parentMesh.remove(existingItem);
                    if (existingItem.geometry && existingItem.geometry.dispose) existingItem.geometry.dispose();
                    if (existingItem.material && existingItem.material.dispose) existingItem.material.dispose();
                } catch { }
                return;
            }

            if (hasItem && !existingItem) {
                const meshWidth = typeof boxSize.width === 'number' ? boxSize.width : 0;
                const meshHeight = typeof boxSize.height === 'number' ? boxSize.height : 0;
                const meshDepth = typeof boxSize.depth === 'number' ? boxSize.depth : 0;

                const td = getTransportDims(nexus3dRef, baseLocationType);
                let innerWidth = meshWidth * 0.8;
                let innerHeight = meshHeight * 0.6;
                let innerDepth = meshDepth * 0.8;
                if (td) {
                    innerWidth = Math.min(td.width || innerWidth, meshWidth);
                    innerHeight = Math.min(td.height || innerHeight, meshHeight);
                    innerDepth = Math.min(td.depth || innerDepth, meshDepth);
                }
                innerWidth = Math.max(2, innerWidth);
                innerHeight = Math.max(2, innerHeight);
                innerDepth = Math.max(2, innerDepth);

                const itemGeom = new THREE.BoxGeometry(innerWidth, innerHeight, innerDepth);
                const itemColor = (nexus3dRef.themeColors && nexus3dRef.themeColors.primary != null)
                    ? nexus3dRef.themeColors.primary
                    : 0x3f51b5;
                const itemMat = new THREE.MeshPhongMaterial({ color: convertToIntegerColor(itemColor), shininess: 60, transparent: true, opacity: 0.6 });
                const itemMesh = new THREE.Mesh(itemGeom, itemMat);
                itemMesh.position.set(0, 0, 0);
                itemMesh.userData = { tag: 'item', itemId: normalizedLocation.currentItemId };

                try {
                    const itemEdgeGeom = new THREE.EdgesGeometry(itemGeom);
                    const edgeCol = (nexus3dRef.themeColors && nexus3dRef.themeColors.textPrimary != null)
                        ? nexus3dRef.themeColors.textPrimary
                        : 0x000000;
                    const itemEdgeMat = new THREE.LineBasicMaterial({ color: convertToIntegerColor(edgeCol) });
                    itemMesh.add(new THREE.LineSegments(itemEdgeGeom, itemEdgeMat));
                } catch { }

                parentMesh.add(itemMesh);
                return;
            }

            if (hasItem && existingItem) {
                // Update item color and id; adjust geometry if needed
                try {
                    existingItem.userData.itemId = normalizedLocation.currentItemId;
                    const newColor = (nexus3dRef.themeColors && nexus3dRef.themeColors.primary != null)
                        ? nexus3dRef.themeColors.primary
                        : 0x3f51b5;
                    if (existingItem.material && existingItem.material.color) {
                        existingItem.material.color.setHex(convertToIntegerColor(newColor));
                        existingItem.material.needsUpdate = true;
                    }

                    const td = getTransportDims(nexus3dRef, baseLocationType);
                    let desiredWidth = boxSize.width * 0.8;
                    let desiredHeight = boxSize.height * 0.6;
                    let desiredDepth = boxSize.depth * 0.8;
                    if (td) {
                        desiredWidth = Math.min(td.width || desiredWidth, boxSize.width);
                        desiredHeight = Math.min(td.height || desiredHeight, boxSize.height);
                        desiredDepth = Math.min(td.depth || desiredDepth, boxSize.depth);
                    }
                    desiredWidth = Math.max(2, desiredWidth);
                    desiredHeight = Math.max(2, desiredHeight);
                    desiredDepth = Math.max(2, desiredDepth);
                    const params = existingItem.geometry && existingItem.geometry.parameters ? existingItem.geometry.parameters : {};
                    if (params.width !== desiredWidth || params.height !== desiredHeight || params.depth !== desiredDepth) {
                        const newGeom = new THREE.BoxGeometry(desiredWidth, desiredHeight, desiredDepth);
                        if (existingItem.geometry && existingItem.geometry.dispose) existingItem.geometry.dispose();
                        existingItem.geometry = newGeom;
                        // Update edges child if present
                        try {
                            for (let ci = existingItem.children.length - 1; ci >= 0; ci--) {
                                const child = existingItem.children[ci];
                                if (child && child.isLineSegments) {
                                    existingItem.remove(child);
                                    if (child.geometry && child.geometry.dispose) child.geometry.dispose();
                                    if (child.material && child.material.dispose) child.material.dispose();
                                }
                            }
                            const edgeCol = (nexus3dRef.themeColors && nexus3dRef.themeColors.textPrimary != null)
                                ? nexus3dRef.themeColors.textPrimary
                                : 0x000000;
                            const edgeGeom = new THREE.EdgesGeometry(newGeom);
                            const edgeMat = new THREE.LineBasicMaterial({ color: convertToIntegerColor(edgeCol) });
                            existingItem.add(new THREE.LineSegments(edgeGeom, edgeMat));
                        } catch { }
                    }
                } catch { }
            }
        } catch { }
    }

    function createRobotMeshObject(nexus3dRef, robotData) {
        const normalizedRobot = normalizeRobotData(robotData);

        let robotColor;
        if (normalizedRobot.robotType === 'Logistics') {
            robotColor = nexus3dRef.themeColors && nexus3dRef.themeColors.primary != null ? nexus3dRef.themeColors.primary : 0x00aaff;
        } else {
            robotColor = nexus3dRef.themeColors && nexus3dRef.themeColors.secondary != null ? nexus3dRef.themeColors.secondary : 0xff8800;
        }

        const cylinderGeometry = new THREE.CylinderGeometry(10, 10, 18, 16);
        const robotMaterial = new THREE.MeshPhongMaterial({
            color: convertToIntegerColor(robotColor),
            shininess: 80
        });
        const robotMesh = new THREE.Mesh(cylinderGeometry, robotMaterial);

        const initialX = (normalizedRobot.x || 0);
        const initialY = (typeof normalizedRobot.z === 'number' && isFinite(normalizedRobot.z)) ? normalizedRobot.z : 9;
        const initialZ = -(normalizedRobot.y || 0);
        robotMesh.position.set(initialX, initialY, initialZ);
        robotMesh.castShadow = true;
        robotMesh.receiveShadow = true;
        robotMesh.userData = {
            id: normalizedRobot.id,
            type: 'robot',
            robotType: normalizedRobot.robotType
        };

        return robotMesh;
    }

    function initializeThreeJsScene(nexus3dRef, initialLocationList) {
        let containerElement;
        if (nexus3dRef.canvas && nexus3dRef.canvas.nodeType === 1) {
            containerElement = nexus3dRef.canvas;
        } else {
            containerElement = document.body;
        }

        const containerWidth = containerElement.clientWidth || window.innerWidth;
        const containerHeight = containerElement.clientHeight || window.innerHeight;

        const canvasElement = document.createElement('canvas');
        canvasElement.id = 'threeCanvas';

        Object.assign(canvasElement.style, {
            position: 'absolute',
            top: '0',
            left: '0',
            width: '100%',
            height: '100%',
            display: 'block',
            zIndex: '0'
        });

        if (getComputedStyle(containerElement).position === 'static') {
            containerElement.style.position = 'relative';
        }

        containerElement.appendChild(canvasElement);

        const webglRenderer = new THREE.WebGLRenderer({
            canvas: canvasElement,
            antialias: true,
            alpha: true
        });
        webglRenderer.setPixelRatio(window.devicePixelRatio || 1);
        webglRenderer.setSize(containerWidth, containerHeight);

        if (nexus3dRef.themeColors && nexus3dRef.themeColors.background != null) {
            webglRenderer.setClearColor(new THREE.Color(convertToIntegerColor(nexus3dRef.themeColors.background)), 1);
        }

        const sceneObject = new THREE.Scene();
        const perspectiveCamera = new THREE.PerspectiveCamera(45, containerWidth / containerHeight, 1, 10000);
        perspectiveCamera.position.set(600, 600, 600);
        perspectiveCamera.lookAt(0, 0, 0);

        let orbitControls = null;

        if (THREE.OrbitControls) {
            orbitControls = new THREE.OrbitControls(perspectiveCamera, webglRenderer.domElement);
            orbitControls.enableDamping = true;
            orbitControls.dampingFactor = 0.08;
            orbitControls.minDistance = 10;
            orbitControls.maxDistance = 10000;
            orbitControls.maxPolarAngle = Math.PI * 0.49;

            if (THREE.MOUSE) {
                orbitControls.mouseButtons = {
                    LEFT: THREE.MOUSE.PAN,
                    MIDDLE: THREE.MOUSE.DOLLY,
                    RIGHT: THREE.MOUSE.ROTATE
                };
            }

            orbitControls.target.set(0, 0, 0);
            orbitControls.update();
        }

        sceneObject.add(new THREE.AmbientLight(0xffffff, 0.6));

        const directionalLight = new THREE.DirectionalLight(0xffffff, 0.8);
        directionalLight.position.set(500, 800, 400);
        sceneObject.add(directionalLight);

        const gridHelper = new THREE.GridHelper(4000, 80, 0x666666, 0xcccccc);
        gridHelper.position.y = -0.01;
        sceneObject.add(gridHelper);

        const locationMeshCollection = new Map();
        const robotMeshCollection = new Map();

        if (Array.isArray(initialLocationList)) {
            try {
                const normalizedList = initialLocationList.map(normalizeLocationData);
                const cassetteItems = normalizedList.filter(x => x.locationType === 'Cassette');
                const trayItems = normalizedList.filter(x => x.locationType === 'Tray');
                const otherItems = normalizedList.filter(x => x.locationType !== 'Cassette' && x.locationType !== 'Tray');

                // 1) Add cassettes first
                cassetteItems.forEach(loc => {
                    const locationMesh = createLocationMeshObject(nexus3dRef, loc);
                    if (locationMesh) {
                        sceneObject.add(locationMesh);
                        locationMeshCollection.set(loc.id, locationMesh);
                    }
                });

                // 2) Add non-cassette, non-tray items
                otherItems.forEach(loc => {
                    const locationMesh = createLocationMeshObject(nexus3dRef, loc);
                    if (locationMesh) {
                        sceneObject.add(locationMesh);
                        locationMeshCollection.set(loc.id, locationMesh);
                    }
                });

                // Helper to find containing cassette for a tray by 2D bounds
                function findContainingCassette(tray) {
                    const tMinX = tray.x;
                    const tMaxX = tray.x + tray.width;
                    const tMinY = tray.y;
                    const tMaxY = tray.y + tray.depth;
                    let found = null;
                    cassetteItems.some(cs => {
                        const cMinX = cs.x;
                        const cMaxX = cs.x + cs.width;
                        const cMinY = cs.y;
                        const cMaxY = cs.y + cs.depth;
                        const inside = tMinX >= cMinX && tMaxX <= cMaxX && tMinY >= cMinY && tMaxY <= cMaxY;
                        if (inside) {
                            found = cs;
                            return true;
                        }
                        return false;
                    });
                    return found;
                }

                // 3) Add trays: only if parent cassette (by parentId or containment) has currentItemId
                const cassettesWithoutItem = new Set(cassetteItems.filter(c => !c.currentItemId).map(c => c.id));
                trayItems.forEach(tray => {
                    const trayMesh = createLocationMeshObject(nexus3dRef, tray);
                    if (!trayMesh) { return; }
                    // Decide container cassette id
                    let containerId = null;
                    if (tray.parentId) {
                        containerId = tray.parentId;
                    } else {
                        const containerCassette = findContainingCassette(tray);
                        containerId = containerCassette ? containerCassette.id : null;
                    }
                    if (containerId && cassettesWithoutItem.has(containerId)) {
                        // Skip adding tray if container cassette has no current item
                        try { trayMesh.visible = false; } catch { }
                        return;
                    }
                    sceneObject.add(trayMesh);
                    locationMeshCollection.set(tray.id, trayMesh);
                });

                // Second pass: parent any item that has parentId set (to location or robot) and/or trays inside cassettes
                function getParentObjectById(pid) {
                    if (!pid) return null;
                    const loc = locationMeshCollection.get(pid);
                    if (loc) return loc;
                    const rob = robotMeshCollection.get(pid);
                    if (rob) return rob;
                    return null;
                }
                normalizedList.forEach(loc => {
                    if (!loc.parentId) return;
                    const child = locationMeshCollection.get(loc.id);
                    const parentObj = getParentObjectById(loc.parentId);
                    if (!child || !parentObj) return;
                    try {
                        parentObj.add(child);
                        if (loc.isRelativePosition) {
                            const p = (parentObj.geometry && parentObj.geometry.parameters) ? parentObj.geometry.parameters : {};
                            const pHalfW = (typeof p.width === 'number') ? (p.width / 2) : 0;
                            const pHalfH = (typeof p.height === 'number') ? (p.height / 2) : 0;
                            const pHalfD = (typeof p.depth === 'number') ? (p.depth / 2) : 0;
                            const halfW = (loc.width || 0) / 2;
                            const halfH = (loc.height || 0) / 2;
                            const halfD = (loc.depth || 0) / 2;
                            const offX = (loc.x || 0);
                            const offY = (loc.y || 0);
                            const offZ = (typeof loc.z === 'number') ? loc.z : 0;
                            const localX = -pHalfW + (offX + halfW);
                            const localY = -pHalfH + (offZ + halfH);
                            const localZ = +pHalfD - (offY + halfD);
                            child.position.set(localX, localY, localZ);
                        } else {
                            const targetWorld = new THREE.Vector3(
                                (loc.x || 0) + (loc.width || 0) / 2,
                                (typeof loc.z === 'number' ? loc.z : 0) + (loc.height || 0) / 2,
                                -((loc.y || 0) + (loc.depth || 0) / 2)
                            );
                            const local = parentObj.worldToLocal(targetWorld.clone());
                            child.position.copy(local);
                        }
                        // Keep item placeholder visible even when trays exist
                    } catch { }
                });
            } catch (error) {
                // 에러 무시
            }
        }

        // 카메라 자동 조정
        (function fitCameraToLocations() {
            if (!Array.isArray(initialLocationList) || initialLocationList.length === 0) {
                return;
            }

            let minXBound = Infinity;
            let minZBound = Infinity;
            let maxXBound = -Infinity;
            let maxZBound = -Infinity;

            for (let locationIndex = 0; locationIndex < initialLocationList.length; locationIndex++) {
                const locationData = normalizeLocationData(initialLocationList[locationIndex]);
                const startX = locationData.x || 0;
                const startZ = locationData.y || 0;
                const endX = startX + (locationData.width || 0);
                const endZ = startZ + (locationData.depth || 0);

                if (startX < minXBound) {
                    minXBound = startX;
                }

                if (startZ < minZBound) {
                    minZBound = startZ;
                }

                if (endX > maxXBound) {
                    maxXBound = endX;
                }

                if (endZ > maxZBound) {
                    maxZBound = endZ;
                }
            }

            if (!isFinite(minXBound) || !isFinite(minZBound) || !isFinite(maxXBound) || !isFinite(maxZBound)) {
                return;
            }

            const centerX = (minXBound + maxXBound) / 2;
            const centerZ = (minZBound + maxZBound) / 2;
            const boundingSpan = Math.max(100, Math.max(maxXBound - minXBound, maxZBound - minZBound));
            const cameraDistance = boundingSpan * 1.4;

            if (orbitControls) {
                orbitControls.target.set(centerX, 0, -centerZ);
                perspectiveCamera.position.set(centerX + cameraDistance, cameraDistance, -centerZ + cameraDistance);
                perspectiveCamera.lookAt(orbitControls.target);
                orbitControls.update();
            } else {
                perspectiveCamera.position.set(centerX + cameraDistance, cameraDistance, -centerZ + cameraDistance);
                perspectiveCamera.lookAt(new THREE.Vector3(centerX, 0, -centerZ));
            }
        })();

        // 마우스 좌표 표시
        const raycastHelper = new THREE.Raycaster();
        const mousePointer = new THREE.Vector2();
        const groundPlane = new THREE.Plane(new THREE.Vector3(0, 1, 0), 0);

        const handleMouseMovement = (mouseEvent) => {
            const canvasRect = webglRenderer.domElement.getBoundingClientRect();
            mousePointer.set(
                ((mouseEvent.clientX - canvasRect.left) / canvasRect.width) * 2 - 1,
                -((mouseEvent.clientY - canvasRect.top) / canvasRect.height) * 2 + 1
            );

            raycastHelper.setFromCamera(mousePointer, perspectiveCamera);
            let intersectionPoint = null;

            try {
                const intersectedObjects = raycastHelper.intersectObjects(sceneObject.children, true);

                if (intersectedObjects && intersectedObjects.length > 0) {
                    intersectionPoint = intersectedObjects[0].point.clone();
                }
            } catch {
                // 에러 무시
            }

            if (!intersectionPoint) {
                const groundIntersection = new THREE.Vector3();

                if (raycastHelper.ray.intersectPlane(groundPlane, groundIntersection)) {
                    intersectionPoint = groundIntersection;
                }
            }

            if (intersectionPoint) {
                // Convert Three.js world (Y-up) to domain (X/Y floor, Z height)
                const worldX = intersectionPoint.x;
                const worldY = intersectionPoint.y;
                const worldZ = intersectionPoint.z;

                const domainX = Math.round(worldX);
                const domainY = Math.round(-worldZ); // domain Y maps to -world Z
                const domainZ = Math.round(worldY);  // domain Z (height) maps to world Y

                const coordinateDisplayElement = document.getElementById('mouseXYZ');
                if (coordinateDisplayElement) {
                    coordinateDisplayElement.textContent = `X: ${domainX}  Y: ${domainY}  Z: ${domainZ}`;
                }
            }
        };

        webglRenderer.domElement.addEventListener('mousemove', handleMouseMovement);

        // 창 크기 조정
        const handleWindowResize = () => {
            const newWidth = containerElement.clientWidth || window.innerWidth;
            const newHeight = containerElement.clientHeight || window.innerHeight;

            webglRenderer.setSize(newWidth, newHeight);
            perspectiveCamera.aspect = newWidth / newHeight;
            perspectiveCamera.updateProjectionMatrix();
        };

        window.addEventListener('resize', handleWindowResize);

        // 애니메이션 루프
        function animationLoop() {
            if (nexus3dInstance.three) {
                nexus3dInstance.three.animationRequestId = requestAnimationFrame(animationLoop);
            }

            if (orbitControls) {
                orbitControls.update();
            }

            webglRenderer.render(sceneObject, perspectiveCamera);
        }

        nexus3dInstance.three = {
            renderer: webglRenderer,
            scene: sceneObject,
            camera: perspectiveCamera,
            controls: orbitControls,
            canvas: canvasElement,
            locationMeshes: locationMeshCollection,
            robotMeshes: robotMeshCollection,
            onResize: handleWindowResize,
            onMove: handleMouseMovement,
            animationRequestId: 0
        };

        // Expose a theme applier for external theme changes
        nexus3dInstance._applyThemeToThree = function () {
            try {
                const colors = nexus3dInstance.themeColors || {};
                if (colors.background != null && webglRenderer && webglRenderer.setClearColor) {
                    webglRenderer.setClearColor(new THREE.Color(convertToIntegerColor(colors.background)), 1);
                }

                // Update location mesh materials
                if (locationMeshCollection && locationMeshCollection.forEach) {
                    locationMeshCollection.forEach((mesh, id) => {
                        if (!mesh || !mesh.material) { return; }
                        const userData = mesh.userData || {};
                        const baseType = userData.locationType;
                        const role = userData.role ? String(userData.role).toLowerCase() : '';
                        const status = userData.status || '';

                        const typeColorMap = {
                            'Cassette': colors.info != null ? colors.info : 0xffc107,
                            'Tray': colors.success != null ? colors.success : 0x2e7d32,
                            'Memory': colors.info != null ? colors.info : 0x0288d1,
                            'Marker': colors.secondary != null ? colors.secondary : 0x9c27b0
                        };

                        let selectedColor = typeColorMap[baseType] || 0x888888;
                        if (role) {
                            const roleColorMap = {
                                'area': colors.background != null ? colors.background : 0x2e7d32,
                                'stocker': colors.background != null ? colors.background : 0xffc107,
                                'set': colors.primary != null ? colors.primary : 0x3f51b5,
                                'movearea': colors.info != null ? colors.info : 0x0288d1
                            };
                            if (roleColorMap[role] != null) {
                                selectedColor = roleColorMap[role];
                            }
                        }

                        const edgeColor = (status === 'Occupied')
                            ? (colors.textPrimary != null ? colors.textPrimary : 0x000000)
                            : (colors.textSecondary != null ? colors.textSecondary : 0x666666);

                        // Update fill material
                        try {
                            const fillColor = convertToIntegerColor(selectedColor);
                            if (mesh.material && mesh.material.color) {
                                mesh.material.color.setHex(fillColor);
                                // Opacity based on role/type
                                let opacity = 1.0;
                        if (role) { opacity = 0.18; }
                        else if (baseType === 'Cassette') { opacity = 0.30; }
                        const isT = opacity < 1.0;
                        mesh.material.transparent = isT;
                        mesh.material.opacity = opacity;
                        if (typeof mesh.material.depthWrite === 'boolean') {
                            mesh.material.depthWrite = !isT;
                        }
                        mesh.material.needsUpdate = true;
                    }
                } catch { }

                        // Update edge material
                        try {
                            const edgeHex = convertToIntegerColor(edgeColor);
                            if (mesh.children && mesh.children.length > 0) {
                                for (let ci = 0; ci < mesh.children.length; ci++) {
                                    const child = mesh.children[ci];
                                    if (child && child.isLineSegments && child.material && child.material.color) {
                                        child.material.color.setHex(edgeHex);
                                        child.material.needsUpdate = true;
                                    }
                                }
                            }
                        } catch { }
                    });
                }

                // Update robot mesh materials
                if (robotMeshCollection && robotMeshCollection.forEach) {
                    robotMeshCollection.forEach((mesh, id) => {
                        try {
                            const ud = mesh.userData || {};
                            const isLogistics = (ud.robotType === 'Logistics');
                            const color = isLogistics
                                ? (colors.primary != null ? colors.primary : 0x00aaff)
                                : (colors.secondary != null ? colors.secondary : 0xff8800);
                            const hex = convertToIntegerColor(color);
                            if (mesh.material && mesh.material.color) {
                                mesh.material.color.setHex(hex);
                                mesh.material.needsUpdate = true;
                            }
                        } catch { }
                    });
                }

                // Update item boxes inside cassette locations
                if (locationMeshCollection && locationMeshCollection.forEach) {
                    locationMeshCollection.forEach((mesh) => {
                        try {
                            if (!mesh || !mesh.children || mesh.children.length === 0) { return; }
                            for (let i = 0; i < mesh.children.length; i++) {
                                const child = mesh.children[i];
                                if (child && child.userData && child.userData.tag === 'item' && child.material && child.material.color) {
                                    const newColor = colors.primary != null ? colors.primary : 0x3f51b5;
                                    child.material.color.setHex(convertToIntegerColor(newColor));
                                    child.material.needsUpdate = true;
                                }
                            }
                        } catch { }
                    });
                }
            } catch { }
        };

        animationLoop();
    }

    // Accept MudBlazor palette dto and apply immediately
    nexus3dInstance.setTheme = function (themeDto) {
        try {
            nexus3dInstance.themeColors = themeDto || {};
            if (typeof nexus3dInstance._applyThemeToThree === 'function') {
                nexus3dInstance._applyThemeToThree();
            }
        } catch { }
    };

    // Inject dimensional standards from portal (e.g., transports)
    nexus3dInstance.setDimensions = function (dimPayload) {
        try {
            const payload = dimPayload || {};
            const transports = payload.transports || {};
            nexus3dInstance.dimensions = {
                transports: {
                    cassette: normalizeDimension(transports.cassette || { width: 28, height: 58, depth: 58 }),
                    tray: normalizeDimension(transports.tray || { width: 28, height: 3, depth: 28 }),
                    memory: normalizeDimension(transports.memory || { width: 4, height: 4, depth: 4 })
                }
            };
        } catch { }
    };

    nexus3dInstance.init3D = function (containerElementId, locationList) {
        try {
            const targetElement = document.getElementById(containerElementId);

            if (!targetElement) {
                return false;
            }

            nexus3dInstance.canvas = targetElement;

            const validLocationArray = Array.isArray(locationList) ? locationList : [];
            initializeThreeJsScene(nexus3dInstance, validLocationArray);

            return true;
        } catch (error) {
            console.warn('init3D failed', error);
            return false;
        }
    };

    nexus3dInstance.addLocation3D = function (locationData) {
        if (!nexus3dInstance.three) {
            return;
        }

        try {
            const locationMesh = createLocationMeshObject(nexus3dInstance, locationData);

            if (locationMesh) {
                nexus3dInstance.three.scene.add(locationMesh);
                const normalizedLocationData = normalizeLocationData(locationData);
                nexus3dInstance.three.locationMeshes.set(normalizedLocationData.id, locationMesh);
            }
        } catch (error) {
            console.warn('addLocation3D failed', error);
        }
    };

    nexus3dInstance.loadRobots3D = function (robotList) {
        if (!nexus3dInstance.three || !Array.isArray(robotList)) {
            return;
        }

        try {
            robotList.forEach(robotData => {
                const robotMesh = createRobotMeshObject(nexus3dInstance, robotData);

                if (robotMesh) {
                    nexus3dInstance.three.scene.add(robotMesh);
                    const normalizedRobotData = normalizeRobotData(robotData);
                    nexus3dInstance.three.robotMeshes.set(normalizedRobotData.id, robotMesh);
                }
            });
        } catch (error) {
            console.warn('loadRobots3D failed', error);
        }
    };

    nexus3dInstance.updateRobot3D = function (robotData) {
        if (!nexus3dInstance.three || !robotData) {
            return;
        }

        const normalizedRobotData = normalizeRobotData(robotData);
        let existingRobotMesh = nexus3dInstance.three.robotMeshes.get(normalizedRobotData.id);

        if (!existingRobotMesh) {
            try {
                existingRobotMesh = createRobotMeshObject(nexus3dInstance, robotData);

                if (existingRobotMesh) {
                    nexus3dInstance.three.scene.add(existingRobotMesh);
                    nexus3dInstance.three.robotMeshes.set(normalizedRobotData.id, existingRobotMesh);
                }
            } catch (error) {
                return;
            }
        } else {
            try {
                const nextX = (normalizedRobotData.x || 0);
                const nextY = (typeof normalizedRobotData.z === 'number' && isFinite(normalizedRobotData.z)) ? normalizedRobotData.z : existingRobotMesh.position.y;
                const nextZ = -(normalizedRobotData.y || 0);
                existingRobotMesh.position.set(nextX, nextY, nextZ);
            } catch (error) {
                // 에러 무시
            }
        }
    };

    // Update or create a location at runtime (size/status/item/position/type)
    nexus3dInstance.updateLocation3D = function (locationData) {
        if (!nexus3dInstance.three || !locationData) {
            return;
        }

        const normalizedLocation = normalizeLocationData(locationData);
        const baseLocationType = normalizedLocation.locationType;
        const role = selectLocationRole(normalizedLocation);
        const parentId = normalizedLocation.parentId || '';

        let existingMesh = nexus3dInstance.three.locationMeshes.get(normalizedLocation.id);

        if (!existingMesh) {
            try {
                const newMesh = createLocationMeshObject(nexus3dInstance, normalizedLocation);
                nexus3dInstance.three.scene.add(newMesh);
                nexus3dInstance.three.locationMeshes.set(normalizedLocation.id, newMesh);
                return;
            } catch (error) {
                console.warn('updateLocation3D create failed', error);
                return;
            }
        }

        try {
            // Update userData
            existingMesh.userData = existingMesh.userData || {};
            existingMesh.userData.id = normalizedLocation.id;
            existingMesh.userData.type = 'location';
            existingMesh.userData.role = role;
            existingMesh.userData.locationType = baseLocationType;
            existingMesh.userData.status = normalizedLocation.status;

            // Update geometry if size changed
            const desiredWidth = typeof normalizedLocation.width === 'number' ? normalizedLocation.width : 0;
            const desiredHeight = typeof normalizedLocation.height === 'number' ? normalizedLocation.height : 0;
            const desiredDepth = typeof normalizedLocation.depth === 'number' ? normalizedLocation.depth : 0;
            let needGeomUpdate = false;
            try {
                const params = (existingMesh.geometry && existingMesh.geometry.parameters) ? existingMesh.geometry.parameters : {};
                if (params.width !== desiredWidth || params.height !== desiredHeight || params.depth !== desiredDepth) {
                    needGeomUpdate = true;
                }
            } catch { needGeomUpdate = true; }

            if (needGeomUpdate) {
                try {
                    // Remove existing edge children
                    for (let ci = existingMesh.children.length - 1; ci >= 0; ci--) {
                        const child = existingMesh.children[ci];
                        if (child && child.isLineSegments) {
                            existingMesh.remove(child);
                            if (child.geometry && child.geometry.dispose) child.geometry.dispose();
                            if (child.material && child.material.dispose) child.material.dispose();
                        }
                    }
                } catch { }

                const newGeom = new THREE.BoxGeometry(desiredWidth, desiredHeight, desiredDepth);
                try { if (existingMesh.geometry && existingMesh.geometry.dispose) existingMesh.geometry.dispose(); } catch { }
                existingMesh.geometry = newGeom;

                // Rebuild edges
                try {
                    const edgeColor = (nexus3dInstance.themeColors && nexus3dInstance.themeColors.textSecondary != null)
                        ? nexus3dInstance.themeColors.textSecondary
                        : 0x666666;
                    const edgeGeom = new THREE.EdgesGeometry(newGeom);
                    const edgeMat = new THREE.LineBasicMaterial({ color: convertToIntegerColor(edgeColor) });
                    existingMesh.add(new THREE.LineSegments(edgeGeom, edgeMat));
                } catch { }
            }

            // Attach to parent if specified
            if (parentId) {
                const parentObj = (nexus3dInstance.three.locationMeshes.get(parentId) || nexus3dInstance.three.robotMeshes.get(parentId) || null);
                if (parentObj && existingMesh.parent !== parentObj) {
                    try { parentObj.add(existingMesh); } catch { }
                }
                if (normalizedLocation.isRelativePosition) {
                    const p = (parentObj && parentObj.geometry && parentObj.geometry.parameters) ? parentObj.geometry.parameters : {};
                    const pHalfW = (typeof p.width === 'number') ? (p.width / 2) : 0;
                    const pHalfH = (typeof p.height === 'number') ? (p.height / 2) : 0;
                    const pHalfD = (typeof p.depth === 'number') ? (p.depth / 2) : 0;
                    const halfW = desiredWidth / 2;
                    const halfH = desiredHeight / 2;
                    const halfD = desiredDepth / 2;
                    const offX = (normalizedLocation.x || 0);
                    const offY = (normalizedLocation.y || 0);
                    const offZ = (typeof normalizedLocation.z === 'number') ? normalizedLocation.z : 0;
                    const localX = -pHalfW + (offX + halfW);
                    const localY = -pHalfH + (offZ + halfH);
                    const localZ = +pHalfD - (offY + halfD);
                    existingMesh.position.set(localX, localY, localZ);
                } else {
                    const posX = (normalizedLocation.x || 0) + desiredWidth / 2;
                    const posZ = (normalizedLocation.y || 0) + desiredDepth / 2;
                    const posY = (typeof normalizedLocation.z === 'number') ? (normalizedLocation.z + (desiredHeight / 2)) : (desiredHeight / 2);
                    const targetWorld = new THREE.Vector3(posX, posY, -posZ);
                    const local = parentObj ? parentObj.worldToLocal(targetWorld.clone()) : targetWorld;
                    existingMesh.position.copy(local);
                }
                existingMesh.userData = existingMesh.userData || {};
                existingMesh.userData.parentId = parentId;
                existingMesh.userData.isRelativePosition = normalizedLocation.isRelativePosition;
                existingMesh.userData.rotateX = normalizedLocation.rotateX || 0;
                existingMesh.userData.rotateY = normalizedLocation.rotateY || 0;
                existingMesh.userData.rotateZ = normalizedLocation.rotateZ || 0;
            } else {
                const posX = (normalizedLocation.x || 0) + desiredWidth / 2;
                const posZ = (normalizedLocation.y || 0) + desiredDepth / 2;
                const posY = (typeof normalizedLocation.z === 'number') ? (normalizedLocation.z + (desiredHeight / 2)) : (desiredHeight / 2);
                existingMesh.position.set(posX, posY, -posZ);
            }
            existingMesh.visible = (normalizedLocation.isVisible !== false);
            try {
                const rx = (normalizedLocation.rotateX || 0) * Math.PI / 180.0;
                const ry = (normalizedLocation.rotateY || 0) * Math.PI / 180.0;
                const rz = (normalizedLocation.rotateZ || 0) * Math.PI / 180.0;
                existingMesh.rotation.set(rx, ry, rz);
            } catch { }

            // Hide tray under cassette if parent cassette has no currentItemId
            try {
                if (baseLocationType === 'Tray' && existingMesh.parent && existingMesh.parent.userData && existingMesh.parent.userData.locationType === 'Cassette') {
                    const cassetteHasItem = !!(existingMesh.parent.userData.currentItemId);
                    existingMesh.visible = cassetteHasItem && (normalizedLocation.isVisible !== false);
                }
            } catch { }

            // If this location is a cassette, update its currentItemId in userData
            try {
                if (baseLocationType === 'Cassette') {
                    existingMesh.userData.currentItemId = normalizedLocation.currentItemId || '';
                }
            } catch { }

            // Update material color and opacity
            try {
                const colors = nexus3dInstance.themeColors || {};
                const typeColorMap = {
                    'Cassette': colors.info != null ? colors.info : 0xffc107,
                    'Tray': colors.success != null ? colors.success : 0x2e7d32,
                    'Memory': colors.info != null ? colors.info : 0x0288d1,
                    'Marker': colors.secondary != null ? colors.secondary : 0x9c27b0
                };
                let selectedColor = typeColorMap[baseLocationType] || 0x888888;
                if (role) {
                    const roleColorMap = {
                        'area': colors.background != null ? colors.background : 0x2e7d32,
                        'stocker': colors.background != null ? colors.background : 0xffc107,
                        'set': colors.primary != null ? colors.primary : 0x3f51b5,
                        'movearea': colors.info != null ? colors.info : 0x0288d1
                    };
                    if (roleColorMap[role] != null) {
                        selectedColor = roleColorMap[role];
                    }
                }
                const edgeColor = (existingMesh.userData.status === 'Occupied')
                    ? (colors.textPrimary != null ? colors.textPrimary : 0x000000)
                    : (colors.textSecondary != null ? colors.textSecondary : 0x666666);

                if (existingMesh.material && existingMesh.material.color) {
                    existingMesh.material.color.setHex(convertToIntegerColor(selectedColor));
                    let opacity = 1.0;
                    if (role) { opacity = 0.18; }
                    else if (baseLocationType === 'Cassette') { opacity = 0.30; }
                    const isT = opacity < 1.0;
                    existingMesh.material.transparent = isT;
                    existingMesh.material.opacity = opacity;
                    if (typeof existingMesh.material.depthWrite === 'boolean') {
                        existingMesh.material.depthWrite = !isT;
                    }
                    existingMesh.material.needsUpdate = true;
                }

                // Update edge color
                if (existingMesh.children && existingMesh.children.length > 0) {
                    for (let ci = 0; ci < existingMesh.children.length; ci++) {
                        const child = existingMesh.children[ci];
                        if (child && child.isLineSegments && child.material && child.material.color) {
                            child.material.color.setHex(convertToIntegerColor(edgeColor));
                            child.material.needsUpdate = true;
                        }
                    }
                }
            } catch { }

            // Re-parent trays into cassettes if needed
            if (baseLocationType === 'Tray') {
                try {
                    // find a container cassette by bounds
                    const tMinX = normalizedLocation.x;
                    const tMaxX = normalizedLocation.x + desiredWidth;
                    const tMinY = normalizedLocation.y;
                    const tMaxY = normalizedLocation.y + desiredDepth;
                    let container = null;
                    nexus3dInstance.three.locationMeshes.forEach((m, key) => {
                        const ud = m.userData || {};
                        if (ud.locationType !== 'Cassette') return;
                        const params = m.geometry && m.geometry.parameters ? m.geometry.parameters : {};
                        const cx = (m.position.x - (params.width || 0) / 2);
                        const cy = (-(m.position.z) - (params.depth || 0) / 2);
                        const cMinX = cx;
                        const cMaxX = cx + (params.width || 0);
                        const cMinY = cy;
                        const cMaxY = cy + (params.depth || 0);
                        const inside = tMinX >= cMinX && tMaxX <= cMaxX && tMinY >= cMinY && tMaxY <= cMaxY;
                        if (inside) { container = m; }
                    });
                    if (container) {
                        // attach as child if not already
                        if (existingMesh.parent !== container) {
                            try {
                                container.add(existingMesh);
                            } catch { }
                        }
                        // remove cassette placeholder item if any
                        try {
                            for (let ci = container.children.length - 1; ci >= 0; ci--) {
                                const child = container.children[ci];
                                if (child && child.userData && child.userData.tag === 'item') {
                                    container.remove(child);
                                    if (child.geometry && child.geometry.dispose) child.geometry.dispose();
                                    if (child.material && child.material.dispose) child.material.dispose();
                                }
                            }
                        } catch { }
                    } else {
                        // ensure it's attached to scene if no container
                        if (existingMesh.parent !== nexus3dInstance.three.scene) {
                            try { nexus3dInstance.three.scene.add(existingMesh); } catch { }
                        }
                    }
                } catch { }
            }

            // Upsert inner item mesh for cassette/tray regardless of tray children
            if (baseLocationType === 'Cassette' || baseLocationType === 'Tray') {
                upsertCassetteItemMesh(nexus3dInstance, existingMesh, normalizedLocation, {
                    width: desiredWidth,
                    height: desiredHeight,
                    depth: desiredDepth
                }, baseLocationType);
            } else {
                // Remove any stray item meshes if type changed away from cassette
                try {
                    for (let ci = existingMesh.children.length - 1; ci >= 0; ci--) {
                        const child = existingMesh.children[ci];
                        if (child && child.userData && child.userData.tag === 'item') {
                            existingMesh.remove(child);
                            if (child.geometry && child.geometry.dispose) child.geometry.dispose();
                            if (child.material && child.material.dispose) child.material.dispose();
                        }
                    }
                } catch { }
            }

        } catch (error) {
            console.warn('updateLocation3D failed', error);
        }
    };
})();
