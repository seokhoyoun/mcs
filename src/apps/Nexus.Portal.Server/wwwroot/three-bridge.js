import * as THREE from "https://esm.sh/three@0.158.0";
import { OrbitControls } from "https://esm.sh/three@0.158.0/examples/jsm/controls/OrbitControls.js";

let animationHandle = 0;
let renderer = null;
let scene = null;
let camera = null;
let controls = null;
let mapGroup = null;
let robotGroup = null;

export async function initScene(canvasElementId) {
    const canvas = document.getElementById(canvasElementId);
    if (!canvas) {
        throw new Error("Canvas element not found");
    }

    scene = new THREE.Scene();
    camera = new THREE.PerspectiveCamera(60, canvas.clientWidth / canvas.clientHeight, 0.1, 2000);
    camera.position.set(0, 50, 120);

    let useWebGpu = !!navigator.gpu;
    let WebGPURendererClass = null;

    if (useWebGpu) {
        try {
            const mod = await import("https://esm.sh/three@0.158.0/examples/jsm/renderers/WebGPURenderer.js");
            WebGPURendererClass = mod && mod.WebGPURenderer ? mod.WebGPURenderer : null;
        } catch {
            useWebGpu = false;
        }
    }

    if (useWebGpu && WebGPURendererClass) {
        try {
            renderer = new WebGPURendererClass({ canvas, antialias: true, forceWebGPU: true });
            await renderer.init();
        } catch {
            renderer = new THREE.WebGLRenderer({ canvas, antialias: true });
        }
    } else {
        renderer = new THREE.WebGLRenderer({ canvas, antialias: true });
    }

    renderer.setSize(canvas.clientWidth, canvas.clientHeight, false);
    renderer.setPixelRatio(window.devicePixelRatio || 1);
    renderer.setClearColor(0x0f172a);

    controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;
    controls.dampingFactor = 0.05;
    controls.target.set(0, 0, 0);

    const light = new THREE.DirectionalLight(0xffffff, 1.0);
    light.position.set(50, 100, 50);
    scene.add(light);

    const ambient = new THREE.AmbientLight(0xffffff, 0.4);
    scene.add(ambient);

    const grid = new THREE.GridHelper(400, 20, 0x3b82f6, 0x1f2937);
    scene.add(grid);

    const axis = new THREE.AxesHelper(40);
    scene.add(axis);

    const demoGeometry = new THREE.BoxGeometry(20, 12, 20);
    const demoMaterial = new THREE.MeshStandardMaterial({ color: 0x22c55e, metalness: 0.15, roughness: 0.65 });
    const demoMesh = new THREE.Mesh(demoGeometry, demoMaterial);
    demoMesh.position.set(0, 6, 0);
    scene.add(demoMesh);

    mapGroup = new THREE.Group();
    scene.add(mapGroup);

    robotGroup = new THREE.Group();
    scene.add(robotGroup);

    const onResize = () => {
        const width = canvas.clientWidth || 1;
        const height = canvas.clientHeight || 1;
        camera.aspect = width / height;
        camera.updateProjectionMatrix();
        renderer.setSize(width, height, false);
    };

    window.addEventListener("resize", onResize);
    onResize();

    const renderLoop = () => {
        if (!renderer || !scene || !camera) {
            return;
        }
        if (controls) {
            controls.update();
        }
        renderer.render(scene, camera);
        animationHandle = window.requestAnimationFrame(renderLoop);
    };

    renderLoop();
}

export function setBackground(hexColor) {
    if (!renderer || !scene) {
        return;
    }
    try {
        const color = new THREE.Color(hexColor);
        renderer.setClearColor(color);
        scene.background = color;
    } catch {
        // ignore invalid color
    }
}

export function disposeScene() {
    if (animationHandle) {
        window.cancelAnimationFrame(animationHandle);
        animationHandle = 0;
    }

    if (renderer) {
        renderer.dispose();
        renderer = null;
    }

    if (controls) {
        controls.dispose();
        controls = null;
    }

    scene = null;
    camera = null;
}

export function resetMap() {
    if (!scene) {
        return;
    }
    if (mapGroup) {
        scene.remove(mapGroup);
    }
    if (robotGroup) {
        scene.remove(robotGroup);
    }
    mapGroup = new THREE.Group();
    robotGroup = new THREE.Group();
    scene.add(mapGroup);
    scene.add(robotGroup);
}

export function addSpace(payload) {
    if (!mapGroup) {
        return;
    }
    addBox(payload, 0x22c55e, 0.18);
}

export function addLocation(payload) {
    if (!mapGroup) {
        return;
    }

    let colorHex = 0x3b82f6;
    if (payload.markerRole) {
        const role = String(payload.markerRole).toLowerCase();
        if (role === "area") {
            colorHex = 0x22c55e;
        } else if (role === "stocker") {
            colorHex = 0xf59e0b;
        } else if (role === "movearea") {
            colorHex = 0x60a5fa;
        } else if (role === "set") {
            colorHex = 0x8b5cf6;
        }
    }

    if (payload.status && String(payload.status).toLowerCase() === "occupied") {
        colorHex = 0xef4444;
    }

    addBox(payload, colorHex, 0.32);
}

export function addEdge(payload) {
    if (!mapGroup) {
        return;
    }

    const material = new THREE.LineBasicMaterial({ color: payload.color ? payload.color : 0x94a3b8 });
    const points = [];
    points.push(new THREE.Vector3(payload.fromX, payload.fromZ, -payload.fromY));
    points.push(new THREE.Vector3(payload.toX, payload.toZ, -payload.toY));
    const geometry = new THREE.BufferGeometry().setFromPoints(points);
    const line = new THREE.Line(geometry, material);
    mapGroup.add(line);
}

export function addRobot(payload) {
    if (!robotGroup) {
        return;
    }

    const geometry = new THREE.SphereGeometry(8, 20, 20);
    const color = payload.robotType && String(payload.robotType).toLowerCase() !== "logistics" ? 0xf97316 : 0x0ea5e9;
    const material = new THREE.MeshStandardMaterial({ color, metalness: 0.3, roughness: 0.5 });
    const mesh = new THREE.Mesh(geometry, material);
    mesh.position.set(payload.x, payload.z, -payload.y);
    robotGroup.add(mesh);
}

export function addSpaceFromJson(jsonText) {
    try {
        const payload = JSON.parse(jsonText);
        addSpace(payload);
    } catch {
    }
}

export function addLocationFromJson(jsonText) {
    try {
        const payload = JSON.parse(jsonText);
        addLocation(payload);
    } catch {
    }
}

export function addEdgeFromJson(jsonText) {
    try {
        const payload = JSON.parse(jsonText);
        addEdge(payload);
    } catch {
    }
}

export function addRobotFromJson(jsonText) {
    try {
        const payload = JSON.parse(jsonText);
        addRobot(payload);
    } catch {
    }
}

const bridge = {
    initScene,
    setBackground,
    disposeScene,
    resetMap,
    addSpaceFromJson,
    addLocationFromJson,
    addEdgeFromJson,
    addRobotFromJson
};

if (typeof globalThis !== "undefined") {
    globalThis.__threeBridge = bridge;
}

function addBox(payload, colorHex, opacity) {
    const width = payload.width || 1;
    const height = payload.height || 1;
    const depth = payload.depth || 1;

    const geometry = new THREE.BoxGeometry(width, height, depth);
    const material = new THREE.MeshLambertMaterial({
        color: colorHex,
        transparent: opacity < 1.0,
        opacity: opacity
    });

    const mesh = new THREE.Mesh(geometry, material);
    const edge = new THREE.EdgesGeometry(geometry);
    const edgeMaterial = new THREE.LineBasicMaterial({ color: 0x0f172a });
    mesh.add(new THREE.LineSegments(edge, edgeMaterial));

    const px = (payload.x || 0) + width / 2;
    const py = (payload.z || 0) + height / 2;
    const pz = -((payload.y || 0) + depth / 2);
    mesh.position.set(px, py, pz);

    mapGroup.add(mesh);
}
