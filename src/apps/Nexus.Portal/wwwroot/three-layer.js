// Minimal Three.js layer integrated with pixiGame namespace
(function () {
    if (!window) {
        return;
    }

    const pixiGameInstance = window.pixiGame || (window.pixiGame = {});

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
        const xCoordinate = getValidNumber(locationData.x, locationData.X, 0);
        const yCoordinate = getValidNumber(locationData.y, locationData.Y, 0);
        const zCoordinate = getValidNumber(locationData.z, locationData.Z, 0);
        const locationWidth = getValidNumber(locationData.width, locationData.Width, 0);
        const locationHeight = getValidNumber(locationData.height, locationData.Height, 0);
        const locationDepth = getValidNumber(locationData.depth, locationData.Depth, 0);
        const markerRole = (locationData.markerRole != null ? locationData.markerRole : locationData.MarkerRole) || '';

        return {
            id: locationId,
            name: locationName,
            locationType,
            status: locationStatus,
            x: xCoordinate,
            y: yCoordinate,
            z: zCoordinate,
            width: locationWidth,
            height: locationHeight,
            depth: locationDepth,
            markerRole
        };
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

    function createLocationMeshObject(pixiGameReference, locationData) {
        const normalizedLocation = normalizeLocationData(locationData);
        const baseLocationType = normalizedLocation.locationType;

        const locationTypeColorMap = {
            'Cassette': pixiGameReference.themeColors && pixiGameReference.themeColors.info != null ? pixiGameReference.themeColors.info : 0xffc107,
            'Tray': pixiGameReference.themeColors && pixiGameReference.themeColors.success != null ? pixiGameReference.themeColors.success : 0x2e7d32,
            'Memory': pixiGameReference.themeColors && pixiGameReference.themeColors.info != null ? pixiGameReference.themeColors.info : 0x0288d1,
            'Marker': pixiGameReference.themeColors && pixiGameReference.themeColors.secondary != null ? pixiGameReference.themeColors.secondary : 0x9c27b0
        };

        let selectedColor = locationTypeColorMap[baseLocationType] || 0x888888;
        const locationRole = selectLocationRole(normalizedLocation);

        if (locationRole) {
            const roleColorMap = {
                'area': pixiGameReference.themeColors && pixiGameReference.themeColors.background != null ? pixiGameReference.themeColors.background : 0x2e7d32,
                'stocker': pixiGameReference.themeColors && pixiGameReference.themeColors.background != null ? pixiGameReference.themeColors.background : 0xffc107,
                'set': pixiGameReference.themeColors && pixiGameReference.themeColors.primary != null ? pixiGameReference.themeColors.primary : 0x3f51b5,
                'movearea': pixiGameReference.themeColors && pixiGameReference.themeColors.info != null ? pixiGameReference.themeColors.info : 0x0288d1
            };
            selectedColor = roleColorMap[locationRole] || selectedColor;
        }

        let edgeColor;
        if (normalizedLocation.status === 'Occupied') {
            edgeColor = pixiGameReference.themeColors && pixiGameReference.themeColors.textPrimary != null ? pixiGameReference.themeColors.textPrimary : 0x000000;
        } else {
            edgeColor = pixiGameReference.themeColors && pixiGameReference.themeColors.textSecondary != null ? pixiGameReference.themeColors.textSecondary : 0x666666;
        }

        const meshWidth = typeof normalizedLocation.width === 'number' ? normalizedLocation.width : 0;
        const meshHeight = typeof normalizedLocation.height === 'number' ? normalizedLocation.height : 0;
        const meshDepth = typeof normalizedLocation.depth === 'number' ? normalizedLocation.depth : 0;

        const boxGeometry = new THREE.BoxGeometry(meshWidth, meshHeight, meshDepth);
        const shouldMakeTransparent = (!!locationRole) || (baseLocationType === 'Cassette');

        let materialOpacity;
        if (locationRole) {
            materialOpacity = 0.2;
        } else if (baseLocationType === 'Cassette') {
            materialOpacity = 0.4;
        } else {
            materialOpacity = 1.0;
        }

        const meshMaterial = new THREE.MeshLambertMaterial({
            color: convertToIntegerColor(selectedColor),
            transparent: shouldMakeTransparent,
            opacity: materialOpacity
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
        locationMesh.userData = {
            id: normalizedLocation.id,
            type: 'location',
            role: locationRole
        };

        return locationMesh;
    }

    function createRobotMeshObject(pixiGameReference, robotData) {
        const normalizedRobot = normalizeRobotData(robotData);

        let robotColor;
        if (normalizedRobot.robotType === 'Logistics') {
            robotColor = pixiGameReference.themeColors && pixiGameReference.themeColors.primary != null ? pixiGameReference.themeColors.primary : 0x00aaff;
        } else {
            robotColor = pixiGameReference.themeColors && pixiGameReference.themeColors.secondary != null ? pixiGameReference.themeColors.secondary : 0xff8800;
        }

        const cylinderGeometry = new THREE.CylinderGeometry(10, 10, 18, 16);
        const robotMaterial = new THREE.MeshPhongMaterial({
            color: convertToIntegerColor(robotColor),
            shininess: 80
        });
        const robotMesh = new THREE.Mesh(cylinderGeometry, robotMaterial);

        robotMesh.position.set((normalizedRobot.x || 0), 9, -(normalizedRobot.y || 0));
        robotMesh.castShadow = true;
        robotMesh.receiveShadow = true;
        robotMesh.userData = {
            id: normalizedRobot.id,
            type: 'robot'
        };

        return robotMesh;
    }

    function initializeThreeJsScene(pixiGameReference, initialLocationList) {
        let containerElement;
        if (pixiGameReference.canvas && pixiGameReference.canvas.nodeType === 1) {
            containerElement = pixiGameReference.canvas;
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

        if (pixiGameReference.themeColors && pixiGameReference.themeColors.background != null) {
            webglRenderer.setClearColor(new THREE.Color(convertToIntegerColor(pixiGameReference.themeColors.background)), 1);
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
                for (let locationIndex = 0; locationIndex < initialLocationList.length; locationIndex++) {
                    const locationMesh = createLocationMeshObject(pixiGameReference, initialLocationList[locationIndex]);

                    if (locationMesh) {
                        sceneObject.add(locationMesh);
                        const normalizedLocationData = normalizeLocationData(initialLocationList[locationIndex]);
                        locationMeshCollection.set(normalizedLocationData.id, locationMesh);
                    }
                }
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
                const roundedX = Math.round(intersectionPoint.x);
                const roundedY = Math.round(intersectionPoint.y);
                const roundedZ = Math.round(intersectionPoint.z);
                const coordinateDisplayElement = document.getElementById('mouseXYZ');

                if (coordinateDisplayElement) {
                    coordinateDisplayElement.textContent = `X: ${roundedX}  Y: ${roundedY}  Z: ${roundedZ}`;
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
            if (pixiGameInstance.three) {
                pixiGameInstance.three.animationRequestId = requestAnimationFrame(animationLoop);
            }

            if (orbitControls) {
                orbitControls.update();
            }

            webglRenderer.render(sceneObject, perspectiveCamera);
        }

        pixiGameInstance.three = {
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

        animationLoop();
    }

    pixiGameInstance.init3D = function (containerElementId, locationList) {
        try {
            const targetElement = document.getElementById(containerElementId);

            if (!targetElement) {
                return false;
            }

            pixiGameInstance.canvas = targetElement;

            const validLocationArray = Array.isArray(locationList) ? locationList : [];
            initializeThreeJsScene(pixiGameInstance, validLocationArray);

            return true;
        } catch (error) {
            console.warn('init3D failed', error);
            return false;
        }
    };

    pixiGameInstance.addLocation3D = function (locationData) {
        if (!pixiGameInstance.three) {
            return;
        }

        try {
            const locationMesh = createLocationMeshObject(pixiGameInstance, locationData);

            if (locationMesh) {
                pixiGameInstance.three.scene.add(locationMesh);
                const normalizedLocationData = normalizeLocationData(locationData);
                pixiGameInstance.three.locationMeshes.set(normalizedLocationData.id, locationMesh);
            }
        } catch (error) {
            console.warn('addLocation3D failed', error);
        }
    };

    pixiGameInstance.loadRobots3D = function (robotList) {
        if (!pixiGameInstance.three || !Array.isArray(robotList)) {
            return;
        }

        try {
            robotList.forEach(robotData => {
                const robotMesh = createRobotMeshObject(pixiGameInstance, robotData);

                if (robotMesh) {
                    pixiGameInstance.three.scene.add(robotMesh);
                    const normalizedRobotData = normalizeRobotData(robotData);
                    pixiGameInstance.three.robotMeshes.set(normalizedRobotData.id, robotMesh);
                }
            });
        } catch (error) {
            console.warn('loadRobots3D failed', error);
        }
    };

    pixiGameInstance.updateRobot3D = function (robotData) {
        if (!pixiGameInstance.three || !robotData) {
            return;
        }

        const normalizedRobotData = normalizeRobotData(robotData);
        let existingRobotMesh = pixiGameInstance.three.robotMeshes.get(normalizedRobotData.id);

        if (!existingRobotMesh) {
            try {
                existingRobotMesh = createRobotMeshObject(pixiGameInstance, robotData);

                if (existingRobotMesh) {
                    pixiGameInstance.three.scene.add(existingRobotMesh);
                    pixiGameInstance.three.robotMeshes.set(normalizedRobotData.id, existingRobotMesh);
                }
            } catch (error) {
                return;
            }
        } else {
            try {
                existingRobotMesh.position.set(
                    (normalizedRobotData.x || 0),
                    existingRobotMesh.position.y,
                    -(normalizedRobotData.y || 0)
                );
            } catch (error) {
                // 에러 무시
            }
        }
    };
})();