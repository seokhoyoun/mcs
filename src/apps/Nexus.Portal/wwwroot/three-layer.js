// Minimal Three.js only scene manager (no PixiJS)
(function () {
    if (!window) {
        return;
    }

    // Expose as window.nexus3d
    const nexus3dInstance = window.nexus3d || (window.nexus3d = {});
    nexus3dInstance._interaction = nexus3dInstance._interaction || {
        dotnetRef: null,
        hoverMethod: null,
        clickMethod: null,
        lastHoverKey: null
    };
    nexus3dInstance.registerInteraction = function (dotnetRef, hoverMethodName, clickMethodName) {
        try {
            nexus3dInstance._interaction.dotnetRef = dotnetRef || null;
            nexus3dInstance._interaction.hoverMethod = (hoverMethodName && typeof hoverMethodName === 'string') ? hoverMethodName : null;
            nexus3dInstance._interaction.clickMethod = (clickMethodName && typeof clickMethodName === 'string') ? clickMethodName : null;
        } catch { }
    };

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

    function normalizeEdgeData(edgeData) {
        if (!edgeData) {
            return {
                id: '',
                fromX: 0,
                fromY: 0,
                fromZ: 0,
                toX: 0,
                toY: 0,
                toZ: 0,
                color: null
            };
        }

        const edgeId = edgeData.id != null ? edgeData.id : edgeData.Id;
        const fromXValue = getValidNumber(edgeData.fromX, edgeData.FromX, 0);
        const fromYValue = getValidNumber(edgeData.fromY, edgeData.FromY, 0);
        const fromZValue = getValidNumber(edgeData.fromZ, edgeData.FromZ, 0);
        const toXValue = getValidNumber(edgeData.toX, edgeData.ToX, 0);
        const toYValue = getValidNumber(edgeData.toY, edgeData.ToY, 0);
        const toZValue = getValidNumber(edgeData.toZ, edgeData.ToZ, 0);
        const edgeColor = edgeData.color != null ? edgeData.color : edgeData.Color;

        return {
            id: edgeId,
            fromX: fromXValue,
            fromY: fromYValue,
            fromZ: fromZValue,
            toX: toXValue,
            toY: toYValue,
            toZ: toZValue,
            color: edgeColor
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
        let selectedColor = (nexus3dRef.themeColors && nexus3dRef.themeColors.primary != null) ? nexus3dRef.themeColors.primary : 0x888888;
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
        const materialOpacity = locationRole ? 0.18 : 1.0;
        const isTransparent = (materialOpacity < 1.0);
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
            locationType: normalizedLocation.locationType,
            status: normalizedLocation.status,
            parentId: normalizedLocation.parentId,
            isVisible: normalizedLocation.isVisible,
            isRelativePosition: normalizedLocation.isRelativePosition,
            currentItemId: normalizedLocation.currentItemId || ''
        };
        locationMesh.visible = (normalizedLocation.isVisible !== false);

        // If this location has an item, render an inner 3D box
        try {
            if (normalizedLocation.currentItemId) {
                const innerWidth = Math.max(2, meshWidth);
                const innerHeight = Math.max(2, meshHeight);
                const innerDepth = Math.max(2, meshDepth);

                const itemGeom = new THREE.BoxGeometry(innerWidth, innerHeight, innerDepth);
                const itemColorValue = (nexus3dRef.themeColors && nexus3dRef.themeColors.primary != null) ? nexus3dRef.themeColors.primary : 0x3f51b5;
                const itemMat = new THREE.MeshPhongMaterial({ color: convertToIntegerColor(itemColorValue), transparent: true, shininess: 60, opacity : 0.5, depthWrite: true, depthTest: true });
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

    function upsertCassetteItemMesh(nexus3dRef, parentMesh, normalizedLocation, boxSize) {
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

                const innerWidth = Math.max(2, meshWidth);
                const innerHeight = Math.max(2, meshHeight);
                const innerDepth = Math.max(2, meshDepth);

                const itemGeom = new THREE.BoxGeometry(innerWidth, innerHeight, innerDepth);
                const itemColorValue = (nexus3dRef.themeColors && nexus3dRef.themeColors.primary != null) ? nexus3dRef.themeColors.primary : 0x3f51b5;
                const itemMat = new THREE.MeshPhongMaterial({ color: convertToIntegerColor(itemColorValue), shininess: 60, transparent: true, opacity: 0.6 });
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
                    const newColorValue = (nexus3dRef.themeColors && nexus3dRef.themeColors.primary != null) ? nexus3dRef.themeColors.primary : 0x3f51b5;
                    if (existingItem.material && existingItem.material.color) {
                        existingItem.material.color.setHex(convertToIntegerColor(newColorValue));
                        existingItem.material.needsUpdate = true;
                    }

                    const desiredWidth = Math.max(2, boxSize.width);
                    const desiredHeight = Math.max(2, boxSize.height);
                    const desiredDepth = Math.max(2, boxSize.depth);
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

    function createEdgeMeshObject(nexus3dRef, edgeData) {
        const normalizedEdge = normalizeEdgeData(edgeData);

        const startVector = new THREE.Vector3(
            normalizedEdge.fromX || 0,
            normalizedEdge.fromZ || 0,
            -(normalizedEdge.fromY || 0)
        );
        const endVector = new THREE.Vector3(
            normalizedEdge.toX || 0,
            normalizedEdge.toZ || 0,
            -(normalizedEdge.toY || 0)
        );

        const edgeGeometry = new THREE.BufferGeometry().setFromPoints([startVector, endVector]);
        const edgeColor = (normalizedEdge.color != null)
            ? normalizedEdge.color
            : (nexus3dRef.themeColors && nexus3dRef.themeColors.textSecondary != null ? nexus3dRef.themeColors.textSecondary : 0x666666);
        const edgeMaterial = new THREE.LineBasicMaterial({
            color: convertToIntegerColor(edgeColor),
            linewidth: 2
        });

        const edgeMesh = new THREE.Line(edgeGeometry, edgeMaterial);
        edgeMesh.userData = {
            id: normalizedEdge.id,
            type: 'edge',
            start: startVector.clone(),
            end: endVector.clone()
        };
        return edgeMesh;
    }

    function fitCameraToScene(nexus3dRef) {
        if (!nexus3dRef || !nexus3dRef.three) {
            return;
        }

        const sceneHandles = nexus3dRef.three;
        const camera = sceneHandles.camera;
        const controls = sceneHandles.controls;

        let minXBound = Infinity;
        let minZBound = Infinity;
        let maxXBound = -Infinity;
        let maxZBound = -Infinity;

        function includeBounds(worldPosition, width, depth) {
            const halfWidth = (typeof width === 'number') ? width / 2 : 0;
            const halfDepth = (typeof depth === 'number') ? depth / 2 : 0;
            const startX = worldPosition.x - halfWidth;
            const endX = worldPosition.x + halfWidth;
            const startZ = worldPosition.z - halfDepth;
            const endZ = worldPosition.z + halfDepth;
            if (startX < minXBound) { minXBound = startX; }
            if (startZ < minZBound) { minZBound = startZ; }
            if (endX > maxXBound) { maxXBound = endX; }
            if (endZ > maxZBound) { maxZBound = endZ; }
        }

        if (sceneHandles.locationMeshes && sceneHandles.locationMeshes.forEach) {
            sceneHandles.locationMeshes.forEach((mesh) => {
                try {
                    const worldPos = new THREE.Vector3();
                    mesh.getWorldPosition(worldPos);
                    const params = (mesh.geometry && mesh.geometry.parameters) ? mesh.geometry.parameters : {};
                    includeBounds(worldPos, params.width || 0, params.depth || 0);
                } catch { }
            });
        }

        if (sceneHandles.edgeMeshes && sceneHandles.edgeMeshes.forEach) {
            sceneHandles.edgeMeshes.forEach((mesh) => {
                try {
                    const start = mesh.userData && mesh.userData.start ? mesh.userData.start : null;
                    const end = mesh.userData && mesh.userData.end ? mesh.userData.end : null;
                    if (start) { includeBounds(start, 0, 0); }
                    if (end) { includeBounds(end, 0, 0); }
                } catch { }
            });
        }

        if (sceneHandles.robotMeshes && sceneHandles.robotMeshes.forEach) {
            sceneHandles.robotMeshes.forEach((mesh) => {
                try {
                    const worldPos = new THREE.Vector3();
                    mesh.getWorldPosition(worldPos);
                    includeBounds(worldPos, 20, 20);
                } catch { }
            });
        }

        if (!isFinite(minXBound) || !isFinite(minZBound) || !isFinite(maxXBound) || !isFinite(maxZBound)) {
            return;
        }

        const centerX = (minXBound + maxXBound) / 2;
        const centerZ = (minZBound + maxZBound) / 2;
        const boundingSpan = Math.max(100, Math.max(maxXBound - minXBound, maxZBound - minZBound));
        const cameraDistance = boundingSpan * 1.4;

        if (controls) {
            controls.target.set(centerX, 0, centerZ);
            camera.position.set(centerX + cameraDistance, cameraDistance, centerZ + cameraDistance);
            camera.lookAt(controls.target);
            controls.update();
        } else {
            camera.position.set(centerX + cameraDistance, cameraDistance, centerZ + cameraDistance);
            camera.lookAt(new THREE.Vector3(centerX, 0, centerZ));
        }
    }

    function initializeThreeJsScene(nexus3dRef) {
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

        const edgeMeshCollection = new Map();

        function ensureDefaultCamera() {
            try {
                fitCameraToScene({ three: { camera: perspectiveCamera, controls: orbitControls, locationMeshes: locationMeshCollection, robotMeshes: robotMeshCollection, edgeMeshes: edgeMeshCollection } });
            } catch { }
        }

        ensureDefaultCamera();

        // 마우스 좌표 표시
        const raycastHelper = new THREE.Raycaster();
        const mousePointer = new THREE.Vector2();
        const groundPlane = new THREE.Plane(new THREE.Vector3(0, 1, 0), 0);

        function findHitTarget(object3D) {
            let current = object3D;
            while (current) {
                if (current.userData && (current.userData.type === 'location' || current.userData.robotType || current.userData.tag === 'item')) {
                    return current;
                }
                current = current.parent;
            }
            return null;
        }

        function buildEventPayload(target) {
            const ud = (target && target.userData) ? target.userData : {};
            if (ud && ud.tag === 'item') {
                const parentData = (target.parent && target.parent.userData) ? target.parent.userData : {};
                const parentId = parentData && parentData.id ? parentData.id : '';
                return { kind: 'item', itemId: ud.itemId || '', parentId: parentId };
            }
            if (ud && ud.type === 'location') {
                return { kind: 'location', id: ud.id || '', role: ud.role || '', status: ud.status || '', itemId: ud.currentItemId || '' };
            }
            if (ud && ud.robotType) {
                return { kind: 'robot', id: ud.id || '', robotType: ud.robotType };
            }
            if (ud && ud.type === 'edge') {
                return { kind: 'edge', id: ud.id || '' };
            }
            return { kind: 'unknown' };
        }

        function hoverKeyFromTarget(target) {
            if (!target || !target.userData) return null;
            if (target.userData.tag === 'item') return `item:${target.userData.itemId}`;
            if (target.userData.type === 'location') return `location:${target.userData.id}`;
            if (target.userData.robotType) return `robot:${target.userData.id}`;
            if (target.userData.type === 'edge') return `edge:${target.userData.id}`;
            return null;
        }

        function setHoverVisual(target, enabled) {
            try {
                if (!target) return;
                const helper = target.userData && target.userData._hoverHelper ? target.userData._hoverHelper : null;
                if (enabled) {
                    if (!helper) {
                        const hh = new THREE.BoxHelper(target, 0xffcc00);
                        hh.userData = { tag: 'hoverHelper' };
                        sceneObject.add(hh);
                        if (!target.userData) target.userData = {};
                        target.userData._hoverHelper = hh;
                    } else {
                        helper.update();
                    }
                    document.body.style.cursor = 'pointer';
                } else {
                    if (helper) {
                        try { sceneObject.remove(helper); } catch { }
                        try { if (helper.geometry && helper.geometry.dispose) helper.geometry.dispose(); } catch { }
                        try { if (helper.material && helper.material.dispose) helper.material.dispose(); } catch { }
                        try { delete target.userData._hoverHelper; } catch { }
                    }
                    document.body.style.cursor = '';
                }
            } catch { }
        }

        const handleMouseMovement = (mouseEvent) => {
            const canvasRect = webglRenderer.domElement.getBoundingClientRect();
            mousePointer.set(
                ((mouseEvent.clientX - canvasRect.left) / canvasRect.width) * 2 - 1,
                -((mouseEvent.clientY - canvasRect.top) / canvasRect.height) * 2 + 1
            );

            raycastHelper.setFromCamera(mousePointer, perspectiveCamera);
            let intersectionPoint = null;
            let hitTarget = null;

            try {
                const intersectedObjects = raycastHelper.intersectObjects(sceneObject.children, true);

                if (intersectedObjects && intersectedObjects.length > 0) {
                    intersectionPoint = intersectedObjects[0].point.clone();
                    for (let i = 0; i < intersectedObjects.length; i++) {
                        const t = findHitTarget(intersectedObjects[i].object);
                        if (t) { hitTarget = t; break; }
                    }
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

            // Hover handling
            try {
                const key = hoverKeyFromTarget(hitTarget);
                const prevKey = nexus3dInstance._interaction.lastHoverKey;
                if (key !== prevKey) {
                    // clear previous visual
                    if (prevKey) {
                        // find previously hovered target by matching collections
                        let prevTarget = null;
                        if (prevKey.startsWith('location:')) {
                            const id = prevKey.substring('location:'.length);
                            prevTarget = locationMeshCollection.get(id) || null;
                        } else if (prevKey.startsWith('robot:')) {
                            const id = prevKey.substring('robot:'.length);
                            prevTarget = robotMeshCollection.get(id) || null;
                        }
                        setHoverVisual(prevTarget, false);
                    }
                    setHoverVisual(hitTarget, !!key);
                    nexus3dInstance._interaction.lastHoverKey = key;

                    // notify dotnet on change only
                    if (nexus3dInstance._interaction.dotnetRef && nexus3dInstance._interaction.hoverMethod) {
                        const payload = hitTarget ? buildEventPayload(hitTarget) : null;
                        try { nexus3dInstance._interaction.dotnetRef.invokeMethodAsync(nexus3dInstance._interaction.hoverMethod, payload); } catch { }
                    }
                } else {
                    // maintain helper
                    if (hitTarget && hitTarget.userData && hitTarget.userData._hoverHelper) {
                        try { hitTarget.userData._hoverHelper.update(); } catch { }
                    }
                }
            } catch { }
        };

        webglRenderer.domElement.addEventListener('mousemove', handleMouseMovement);

        const handleClick = (mouseEvent) => {
            try {
                const canvasRect = webglRenderer.domElement.getBoundingClientRect();
                mousePointer.set(
                    ((mouseEvent.clientX - canvasRect.left) / canvasRect.width) * 2 - 1,
                    -((mouseEvent.clientY - canvasRect.top) / canvasRect.height) * 2 + 1
                );
                raycastHelper.setFromCamera(mousePointer, perspectiveCamera);
                const intersects = raycastHelper.intersectObjects(sceneObject.children, true);
                if (intersects && intersects.length > 0) {
                    let target = null;
                    for (let i = 0; i < intersects.length; i++) {
                        const t = findHitTarget(intersects[i].object);
                        if (t) { target = t; break; }
                    }
                    if (target && nexus3dInstance._interaction.dotnetRef && nexus3dInstance._interaction.clickMethod) {
                        const payload = buildEventPayload(target);
                        nexus3dInstance._interaction.dotnetRef.invokeMethodAsync(nexus3dInstance._interaction.clickMethod, payload);
                    }
                }
            } catch { }
        };
        webglRenderer.domElement.addEventListener('click', handleClick);

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
            edgeMeshes: edgeMeshCollection,
            onResize: handleWindowResize,
            onMove: handleMouseMovement,
            onClick: handleClick,
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
                    locationMeshCollection.forEach((mesh) => {
                        if (!mesh || !mesh.material) { return; }
                        const userData = mesh.userData || {};
                        const role = userData.role ? String(userData.role).toLowerCase() : '';
                        const status = userData.status || '';

                        let selectedColor = colors.primary != null ? colors.primary : 0x888888;
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

                        try {
                            const fillColor = convertToIntegerColor(selectedColor);
                            if (mesh.material && mesh.material.color) {
                                mesh.material.color.setHex(fillColor);
                                let opacity = 1.0;
                                if (role) { opacity = 0.18; }
                                const isTransparent = opacity < 1.0;
                                mesh.material.transparent = isTransparent;
                                mesh.material.opacity = opacity;
                                if (typeof mesh.material.depthWrite === 'boolean') {
                                    mesh.material.depthWrite = !isTransparent;
                                }
                                mesh.material.needsUpdate = true;
                            }
                        } catch { }

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

                if (edgeMeshCollection && edgeMeshCollection.forEach) {
                    edgeMeshCollection.forEach((mesh) => {
                        try {
                            const edgeHex = convertToIntegerColor(colors.textSecondary != null ? colors.textSecondary : 0x666666);
                            if (mesh.material && mesh.material.color) {
                                mesh.material.color.setHex(edgeHex);
                                mesh.material.needsUpdate = true;
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

    nexus3dInstance.init3D = function (containerElementId) {
        try {
            const targetElement = document.getElementById(containerElementId);

            if (!targetElement) {
                return false;
            }

            nexus3dInstance.canvas = targetElement;
            initializeThreeJsScene(nexus3dInstance);

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
                fitCameraToScene(nexus3dInstance);
            }
        } catch (error) {
            console.warn('addLocation3D failed', error);
        }
    };

    nexus3dInstance.addSpace3D = function (spaceData) {
        if (!spaceData) {
            return;
        }

        const normalizedSpace = normalizeLocationData(spaceData);
        if (!normalizedSpace.locationType) {
            normalizedSpace.locationType = 'Space';
        }
        if (!normalizedSpace.markerRole) {
            normalizedSpace.markerRole = 'Area';
        }
        nexus3dInstance.addLocation3D(normalizedSpace);
    };

    nexus3dInstance.addEdge3D = function (edgeData) {
        if (!nexus3dInstance.three) {
            return;
        }

        try {
            const edgeMesh = createEdgeMeshObject(nexus3dInstance, edgeData);
            if (!edgeMesh) {
                return;
            }

            nexus3dInstance.three.scene.add(edgeMesh);
            const normalizedEdge = normalizeEdgeData(edgeData);
            nexus3dInstance.three.edgeMeshes.set(normalizedEdge.id, edgeMesh);
            fitCameraToScene(nexus3dInstance);
        } catch (error) {
            console.warn('addEdge3D failed', error);
        }
    };

    nexus3dInstance.addRobot3D = function (robotData) {
        nexus3dInstance.updateRobot3D(robotData);
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

        try {
            fitCameraToScene(nexus3dInstance);
        } catch { }
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

        try {
            fitCameraToScene(nexus3dInstance);
        } catch { }
    };

    // Update or create a location at runtime (size/status/item/position/type)
   
})();
