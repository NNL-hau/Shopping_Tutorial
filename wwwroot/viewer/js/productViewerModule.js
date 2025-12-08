import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.164.0/build/three.module.js';
import { OrbitControls } from 'https://cdn.jsdelivr.net/npm/three@0.164.0/examples/jsm/controls/OrbitControls.js';
import { GLTFLoader } from 'https://cdn.jsdelivr.net/npm/three@0.164.0/examples/jsm/loaders/GLTFLoader.js';

const API_ENDPOINT = '/api/products';

class ViewerPane {
    constructor(container, name, { onComponentClick, onCameraChange, onLoaded }) {
        this.container = container;
        this.name = name;
        this.onComponentClick = onComponentClick;
        this.onCameraChange = onCameraChange;
        this.onLoaded = onLoaded;
        this.canvasHost = container.querySelector('.renderer-wrapper');
        this.componentListEl = container.querySelector('[data-role="component-list"]');
        this.statusEl = container.querySelector('[data-role="viewer-status"]');
        this.metaEl = container.querySelector('[data-role="viewer-meta"]');
        this.currentProduct = null;
        this.scene = null;
        this.camera = null;
        this.renderer = null;
        this.controls = null;
        this.clock = new THREE.Clock();
        this.mixer = null;
        this.animations = [];
        this.highlightEnabled = true;
        this.highlightedMesh = null;
        this.originalMaterials = new Map();
        this.meshLookup = new Map();
        this.pointer = new THREE.Vector2();
        this.raycaster = new THREE.Raycaster();
        this._animationFrame = null;
        this._cameraDebounce = null;
        this._resizeObserver = null;
        this._componentIndex = new Map();
        this._isInitialized = false;
        this._loader = new GLTFLoader();
    }

    init() {
        if (this._isInitialized) {
            return;
        }

        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0x101216);

        const width = this.canvasHost.clientWidth || 640;
        const height = this.canvasHost.clientHeight || 480;

        this.camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 2000);
        this.camera.position.set(0.8, 0.9, 1.4);

        const hemiLight = new THREE.HemisphereLight(0xffffff, 0x222233, 1.1);
        hemiLight.position.set(0, 1, 0);
        this.scene.add(hemiLight);

        const dirLight = new THREE.DirectionalLight(0xffffff, 1.2);
        dirLight.position.set(2.5, 2.5, 2.5);
        dirLight.castShadow = true;
        this.scene.add(dirLight);

        const fillLight = new THREE.DirectionalLight(0x99ccff, 0.65);
        fillLight.position.set(-2.5, 1.6, -2.2);
        this.scene.add(fillLight);

        this.renderer = new THREE.WebGLRenderer({ antialias: true });
        this.renderer.setPixelRatio(window.devicePixelRatio || 1.5);
        this.renderer.setSize(width, height);
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
        this.canvasHost.appendChild(this.renderer.domElement);

        this.controls = new OrbitControls(this.camera, this.renderer.domElement);
        this.controls.enableDamping = true;
        this.controls.dampingFactor = 0.06;
        this.controls.minDistance = 0.3;
        this.controls.maxDistance = 5;
        this.controls.autoRotate = false;
        this.controls.addEventListener('change', () => {
            if (typeof this.onCameraChange === 'function') {
                if (this._cameraDebounce) {
                    clearTimeout(this._cameraDebounce);
                }
                this._cameraDebounce = setTimeout(() => {
                    this.onCameraChange(this.getCameraState());
                }, 12);
            }
        });

        this.renderer.domElement.addEventListener('pointerdown', (event) => this.handlePointer(event));

        this._resizeObserver = new ResizeObserver(() => this.handleResize());
        this._resizeObserver.observe(this.canvasHost);

        this.animate();
        this._isInitialized = true;
    }

    async loadProduct(product) {
        if (!this._isInitialized) {
            this.init();
        }

        this.setStatus(`Đang tải mô hình ${product.name}...`);
        this.currentProduct = product;
        this.componentListEl.innerHTML = '';
        this.meshLookup.clear();
        this._componentIndex.clear();
        this.clearHighlight();

        if (this.gltfScene) {
            this.scene.remove(this.gltfScene);
            this.disposeNode(this.gltfScene);
            this.gltfScene = null;
        }

        this.mixer = null;
        this.animations = [];

        try {
            const gltf = await this._loader.loadAsync(product.modelUrl);
            this.gltfScene = gltf.scene;
            this.animations = gltf.animations || [];

            gltf.scene.traverse((child) => {
                if (child.isMesh) {
                    child.castShadow = true;
                    child.receiveShadow = true;
                    this.meshLookup.set(child.name, child);
                    if (!child.material) {
                        child.material = new THREE.MeshStandardMaterial({
                            color: 0xb0bec5,
                            metalness: 0.6,
                            roughness: 0.35
                        });
                    }
                }
            });

            this.scene.add(gltf.scene);

            if (this.animations.length > 0) {
                this.mixer = new THREE.AnimationMixer(gltf.scene);
                const clip = this.animations[0];
                const action = this.mixer.clipAction(clip);
                action.play();
            }

            this.frameScene(gltf.scene);
            this.populateComponentList(product);
            this.setMeta(product);
            this.setStatus(`Đã tải ${product.name}`);

            if (typeof this.onLoaded === 'function') {
                this.onLoaded(product, { hasAnimation: this.animations.length > 0 });
            }
        }
        catch (error) {
            console.error(`[${this.name}] Không thể tải mô hình GLB`, error);
            this.setStatus('Không thể tải mô hình. Kiểm tra đường dẫn file .glb.');
        }
    }

    frameScene(scene) {
        const box = new THREE.Box3().setFromObject(scene);
        const size = box.getSize(new THREE.Vector3());
        const center = box.getCenter(new THREE.Vector3());

        const maxDim = Math.max(size.x, size.y, size.z);
        const fitHeightDistance = maxDim / (2 * Math.atan((Math.PI * this.camera.fov) / 360));
        const fitWidthDistance = fitHeightDistance / this.camera.aspect;
        const distance = 1.2 * Math.max(fitHeightDistance, fitWidthDistance);

        const direction = new THREE.Vector3()
            .subVectors(this.camera.position, this.controls.target)
            .normalize();

        this.controls.target.copy(center);
        this.camera.position.copy(direction.multiplyScalar(distance).add(center));
        this.camera.near = distance / 100;
        this.camera.far = distance * 100;
        this.camera.updateProjectionMatrix();
        this.controls.update();
    }

    populateComponentList(product) {
        this.componentListEl.innerHTML = '';
        product.components
            .sort((a, b) => a.name.localeCompare(b.name))
            .forEach((component) => {
                this._componentIndex.set(component.id, component);
                const item = document.createElement('article');
                item.className = 'component-item';
                item.dataset.componentId = String(component.id);
                item.dataset.meshName = component.meshName;
                item.innerHTML = `
                    <h4>${component.name}</h4>
                    <p>${component.description}</p>
                    <div class="component-tags">
                        <span>Vật liệu: ${component.material || 'Không rõ'}</span>
                        <span>Khối lượng: ${component.weight ? component.weight.toFixed(2) + ' kg' : '—'}</span>
                    </div>
                `;

                item.addEventListener('click', () => {
                    this.focusComponent(component.id);
                    if (typeof this.onComponentClick === 'function') {
                        this.onComponentClick(component);
                    }
                });

                this.componentListEl.appendChild(item);
            });
    }

    focusComponent(componentId) {
        const component = this._componentIndex.get(componentId);
        if (!component) {
            return;
        }
        this.highlightMesh(component.meshName);
        this.componentListEl.querySelectorAll('.component-item').forEach((item) => {
            item.classList.toggle('active', item.dataset.componentId === String(componentId));
        });
    }

    highlightMesh(meshName) {
        if (!this.highlightEnabled) {
            return;
        }

        if (this.highlightedMesh) {
            this.restoreMaterial(this.highlightedMesh);
            this.highlightedMesh = null;
        }

        const mesh = this.meshLookup.get(meshName);
        if (!mesh || !mesh.material) {
            return;
        }

        if (!this.originalMaterials.has(mesh.uuid)) {
            this.originalMaterials.set(mesh.uuid, mesh.material.clone());
        }

        mesh.material = mesh.material.clone();
        mesh.material.emissive = new THREE.Color(0x00bcd4);
        mesh.material.emissiveIntensity = 0.45;
        mesh.material.color = new THREE.Color(0xffffff);
        mesh.material.needsUpdate = true;
        this.highlightedMesh = mesh;
    }

    highlightMeshSoft(meshName) {
        if (!this.highlightEnabled) {
            return;
        }

        const mesh = this.meshLookup.get(meshName);
        if (!mesh) {
            return;
        }

        mesh.userData._originalOpacity = mesh.material.opacity ?? 1;
        mesh.material = mesh.material.clone();
        mesh.material.opacity = 0.6;
        mesh.material.transparent = true;
        mesh.material.color = new THREE.Color(0x4fc3f7);
        mesh.material.emissive = new THREE.Color(0x4fc3f7);
        mesh.material.emissiveIntensity = 0.28;
        mesh.material.needsUpdate = true;

        setTimeout(() => {
            this.restoreMaterial(mesh);
        }, 220);
    }

    restoreMaterial(mesh) {
        if (!mesh) {
            return;
        }

        const originalMaterial = this.originalMaterials.get(mesh.uuid);
        if (originalMaterial) {
            mesh.material.dispose();
            mesh.material = originalMaterial.clone();
        }
        else if (mesh.userData?._originalOpacity !== undefined) {
            mesh.material.opacity = mesh.userData._originalOpacity;
            mesh.material.transparent = mesh.userData._originalOpacity < 1;
        }
        mesh.material.needsUpdate = true;
    }

    clearHighlight() {
        if (this.highlightedMesh) {
            this.restoreMaterial(this.highlightedMesh);
        }
        this.componentListEl.querySelectorAll('.component-item').forEach((item) => item.classList.remove('active'));
    }

    handlePointer(event) {
        const rect = this.renderer.domElement.getBoundingClientRect();
        this.pointer.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
        this.pointer.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

        this.raycaster.setFromCamera(this.pointer, this.camera);
        const intersects = this.raycaster.intersectObjects(Array.from(this.meshLookup.values()), true);

        if (intersects.length > 0) {
            const hit = intersects[0].object;
            const component = this.getComponentByMesh(hit.name);
            if (component) {
                this.focusComponent(component.id);
                if (typeof this.onComponentClick === 'function') {
                    this.onComponentClick(component);
                }
            }
        }
    }

    getComponentByMesh(meshName) {
        return this.currentProduct?.components.find((component) => component.meshName === meshName);
    }

    setMeta(product) {
        if (!this.metaEl) {
            return;
        }

        this.metaEl.innerHTML = `
            <span><strong>Hãng:</strong> ${product.manufacturer}</span>
            <span><strong>Nhóm:</strong> ${product.category}</span>
            <span><strong>Chất liệu khung:</strong> ${product.material}</span>
            <span><strong>Khối lượng:</strong> ${product.weight.toFixed(2)} kg</span>
        `;
    }

    setStatus(text) {
        if (this.statusEl) {
            this.statusEl.innerHTML = text ? `<span>${text}</span>` : '';
        }
    }

    toggleHighlight(enable) {
        this.highlightEnabled = enable;
        if (!enable) {
            this.clearHighlight();
        }
    }

    playPauseAnimation() {
        if (!this.mixer || this.animations.length === 0) {
            return { hasAnimation: false, playing: false };
        }

        const action = this.mixer.clipAction(this.animations[0]);
        if (action.paused) {
            action.paused = false;
            action.play();
            return { hasAnimation: true, playing: true };
        }

        action.paused = !action.paused;
        if (action.paused) {
            action.stop();
        }
        else {
            action.reset();
            action.play();
        }
        return { hasAnimation: true, playing: !action.paused };
    }

    handleResize() {
        if (!this.renderer || !this.camera) {
            return;
        }
        const width = this.canvasHost.clientWidth;
        const height = this.canvasHost.clientHeight;
        this.renderer.setSize(width, height);
        this.camera.aspect = width / height;
        this.camera.updateProjectionMatrix();
    }

    animate() {
        this._animationFrame = requestAnimationFrame(() => this.animate());
        if (this.controls) {
            this.controls.update();
        }
        if (this.mixer) {
            this.mixer.update(this.clock.getDelta());
        }
        if (this.renderer && this.scene && this.camera) {
            this.renderer.render(this.scene, this.camera);
        }
    }

    dispose() {
        cancelAnimationFrame(this._animationFrame);
        if (this._resizeObserver) {
            this._resizeObserver.disconnect();
        }

        if (this.renderer) {
            this.renderer.dispose();
        }
        if (this.controls) {
            this.controls.dispose();
        }
        if (this.scene) {
            this.disposeNode(this.scene);
        }
        this.meshLookup.clear();
    }

    disposeNode(node) {
        if (!node) {
            return;
        }
        node.traverse((child) => {
            if (child.isMesh) {
                child.geometry?.dispose();
                if (child.material?.isMaterial) {
                    child.material.dispose();
                }
            }
        });
    }

    getCameraState() {
        return {
            position: this.camera.position.clone(),
            target: this.controls.target.clone()
        };
    }

    applyCameraState(state) {
        if (!state) {
            return;
        }
        this.camera.position.copy(state.position);
        this.controls.target.copy(state.target);
        this.camera.updateProjectionMatrix();
        this.controls.update();
    }
}

class ProductViewerModule {
    constructor(root) {
        this.root = root;
        this.products = [];
        this.viewerA = null;
        this.viewerB = null;
        this.cameraSyncEnabled = true;
        this.highlightEnabled = true;
        this.componentPopup = null;
        this.popupTitle = null;
        this.popupBody = null;
        this.popupFooter = null;
        this._isLoading = false;
        this._cameraSyncLock = false;
    }

    async init() {
        if (!this.root) {
            console.warn('Không tìm thấy vùng chứa module 3D.');
            return;
        }

        this.renderLayout();
        this.cacheElements();
        this.bindToolbarActions();

        try {
            this.setLoading(true);
            await this.fetchProducts();
            this.populateSelectors();
            this.initializeViewers();
            this.attachSelectionHandlers();
            this.setLoading(false);
        }
        catch (error) {
            console.error('Không thể khởi tạo module so sánh 3D', error);
            this.setLoading(false, 'Không thể tải dữ liệu sản phẩm từ API.');
        }
    }

    renderLayout() {
        this.root.innerHTML = `
            <h2>So sánh 3D sản phẩm</h2>
            <p class="viewer-intro">
                Chọn hai sản phẩm để hiển thị song song trong không gian 3D. Tất cả dữ liệu được đồng bộ từ API nội bộ,
                camera luân phiên theo dõi để bạn dễ đối chiếu từng bộ phận chi tiết.
            </p>
            <div class="viewer-toolbar" data-role="viewer-toolbar">
                <button data-action="sync-camera" class="active">Đồng bộ camera</button>
                <button data-action="toggle-highlight" class="active">Highlight bộ phận</button>
                <button data-action="play-animation">Chạy animation</button>
                <button data-action="reset-camera">Đặt lại góc nhìn</button>
            </div>
            <div class="viewer-panels">
                ${this.renderViewerPanel('A')}
                ${this.renderViewerPanel('B')}
            </div>
            <aside class="component-popup" data-role="component-popup">
                <header>
                    <h4 data-role="popup-title">Component info</h4>
                    <button type="button" data-action="close-popup" aria-label="Đóng">&times;</button>
                </header>
                <div class="popup-body" data-role="popup-body"></div>
                <footer data-role="popup-footer"></footer>
            </aside>
        `;
    }

    renderViewerPanel(label) {
        return `
            <section class="viewer-panel" data-viewer="${label}">
                <div class="viewer-header">
                    <h3>Viewer ${label}</h3>
                </div>
                <select class="viewer-select" data-role="product-select" data-viewer="${label}">
                    <option value="">Đang tải danh sách sản phẩm...</option>
                </select>
                <div class="renderer-wrapper"></div>
                <div class="viewer-meta" data-role="viewer-meta"></div>
                <div class="component-list" data-role="component-list"></div>
                <div class="viewer-status" data-role="viewer-status"></div>
            </section>
        `;
    }

    cacheElements() {
        this.toolbar = this.root.querySelector('[data-role="viewer-toolbar"]');
        this.selects = this.root.querySelectorAll('[data-role="product-select"]');
        this.componentPopup = this.root.querySelector('[data-role="component-popup"]');
        this.popupTitle = this.root.querySelector('[data-role="popup-title"]');
        this.popupBody = this.root.querySelector('[data-role="popup-body"]');
        this.popupFooter = this.root.querySelector('[data-role="popup-footer"]');
    }

    bindToolbarActions() {
        this.toolbar.querySelector('[data-action="sync-camera"]').addEventListener('click', (event) => {
            this.cameraSyncEnabled = !this.cameraSyncEnabled;
            event.currentTarget.classList.toggle('active', this.cameraSyncEnabled);
        });

        this.toolbar.querySelector('[data-action="toggle-highlight"]').addEventListener('click', (event) => {
            this.highlightEnabled = !this.highlightEnabled;
            event.currentTarget.classList.toggle('active', this.highlightEnabled);
            if (this.viewerA) this.viewerA.toggleHighlight(this.highlightEnabled);
            if (this.viewerB) this.viewerB.toggleHighlight(this.highlightEnabled);
        });

        this.toolbar.querySelector('[data-action="play-animation"]').addEventListener('click', (event) => {
            const states = [
                this.viewerA?.playPauseAnimation(),
                this.viewerB?.playPauseAnimation()
            ].filter(Boolean);

            const anyAnimation = states.some((state) => state?.hasAnimation);
            if (!anyAnimation) {
                event.currentTarget.classList.remove('active');
                event.currentTarget.textContent = 'Không có animation';
                setTimeout(() => {
                    event.currentTarget.textContent = 'Chạy animation';
                }, 2200);
                return;
            }

            const playing = states.some((state) => state?.playing);
            event.currentTarget.classList.toggle('active', playing);
            event.currentTarget.textContent = playing ? 'Dừng animation' : 'Chạy animation';
        });

        this.toolbar.querySelector('[data-action="reset-camera"]').addEventListener('click', () => {
            [this.viewerA, this.viewerB].forEach((viewer) => {
                if (!viewer || !viewer.currentProduct) return;
                viewer.frameScene(viewer.gltfScene ?? viewer.scene);
                viewer.setStatus('Đã đặt lại góc nhìn');
            });
        });

        this.componentPopup.querySelector('[data-action="close-popup"]').addEventListener('click', () => {
            this.hidePopup();
        });
    }

    async fetchProducts() {
        const response = await fetch(API_ENDPOINT);
        if (!response.ok) {
            throw new Error(`API trả về lỗi ${response.status}`);
        }
        this.products = await response.json();
    }

    populateSelectors() {
        this.selects.forEach((select) => {
            select.innerHTML = this.products
                .map((product) => `<option value="${product.id}">${product.name}</option>`)
                .join('');
        });

        if (this.products.length > 0) {
            const [first, second] = this.products;
            const selectsArray = Array.from(this.selects);

            if (selectsArray[0] && first) {
                selectsArray[0].value = first.id;
            }
            if (selectsArray[1]) {
                selectsArray[1].value = second ? second.id : first?.id ?? '';
            }
        }
    }

    initializeViewers() {
        const panelA = this.root.querySelector('.viewer-panel[data-viewer="A"]');
        const panelB = this.root.querySelector('.viewer-panel[data-viewer="B"]');

        this.viewerA = new ViewerPane(panelA, 'A', {
            onComponentClick: (component) => this.handleComponentSelection('A', component),
            onCameraChange: (state) => this.syncCamera('A', state),
            onLoaded: (product) => panelA.classList.add('active')
        });
        this.viewerB = new ViewerPane(panelB, 'B', {
            onComponentClick: (component) => this.handleComponentSelection('B', component),
            onCameraChange: (state) => this.syncCamera('B', state),
            onLoaded: (product) => panelB.classList.add('active')
        });

        this.viewerA.init();
        this.viewerB.init();

        const defaultA = this.selects[0]?.value;
        const defaultB = this.selects[1]?.value;

        if (defaultA) {
            const product = this.products.find((p) => String(p.id) === String(defaultA));
            if (product) {
                this.viewerA.loadProduct(product);
            }
        }

        if (defaultB) {
            const product = this.products.find((p) => String(p.id) === String(defaultB));
            if (product) {
                this.viewerB.loadProduct(product);
            }
        }
    }

    attachSelectionHandlers() {
        this.selects.forEach((select) => {
            select.addEventListener('change', (event) => {
                const viewerName = event.target.dataset.viewer;
                const productId = event.target.value;
                const product = this.products.find((p) => String(p.id) === String(productId));
                if (!product) {
                    return;
                }

                if (viewerName === 'A') {
                    this.viewerA.loadProduct(product);
                }
                else {
                    this.viewerB.loadProduct(product);
                }
            });
        });
    }

    handleComponentSelection(sourceViewer, component) {
        this.showPopup(component);

        const [source, target] = sourceViewer === 'A'
            ? [this.viewerA, this.viewerB]
            : [this.viewerB, this.viewerA];

        source.highlightMesh(component.meshName);

        if (target?.currentProduct) {
            const counterpart = target.currentProduct.components.find((c) => c.meshName === component.meshName || c.name === component.name);
            if (counterpart) {
                target.highlightMesh(counterpart.meshName);
                target.highlightMeshSoft(counterpart.meshName);
                target.componentListEl.querySelectorAll('.component-item').forEach((item) => {
                    item.classList.toggle('active', item.dataset.componentId === String(counterpart.id));
                });
            }
        }
    }

    syncCamera(source, state) {
        if (!this.cameraSyncEnabled || this._cameraSyncLock) {
            return;
        }

        this._cameraSyncLock = true;
        try {
            if (source === 'A' && this.viewerB) {
                this.viewerB.applyCameraState(state);
            }
            else if (source === 'B' && this.viewerA) {
                this.viewerA.applyCameraState(state);
            }
        }
        finally {
            this._cameraSyncLock = false;
        }
    }

    showPopup(component) {
        if (!component) {
            this.hidePopup();
            return;
        }

        this.popupTitle.textContent = component.name;
        this.popupBody.innerHTML = `
            <div><strong>Mô tả:</strong> ${component.description || '—'}</div>
            <div><strong>Vật liệu:</strong> ${component.material || '—'}</div>
            <div><strong>Khối lượng:</strong> ${component.weight ? component.weight.toFixed(2) + ' kg' : 'Không xác định'}</div>
            <div><strong>Mesh:</strong> ${component.meshName || 'Không rõ'}</div>
            ${component.notes ? `<div><strong>Ghi chú:</strong> ${component.notes}</div>` : ''}
        `;
        this.popupFooter.innerHTML = `
            <a href="${component.assetUrl ?? '#'}" target="_blank" rel="noopener">Tải tài liệu kỹ thuật</a>
            <a href="${component.spareUrl ?? '#'}" target="_blank" rel="noopener">Phụ kiện thay thế</a>
        `;
        this.componentPopup.classList.add('visible');
    }

    hidePopup() {
        this.componentPopup.classList.remove('visible');
    }

    setLoading(isLoading, message) {
        this._isLoading = isLoading;
        if (isLoading) {
            this.root.classList.add('is-loading');
            this.root.setAttribute('data-loading-text', message ?? 'Đang tải dữ liệu 3D...');
        }
        else {
            this.root.classList.remove('is-loading');
            this.root.removeAttribute('data-loading-text');
            if (message) {
                const status = document.createElement('div');
                status.className = 'viewer-status';
                status.innerHTML = `<span>${message}</span>`;
                this.root.appendChild(status);
            }
        }
    }
}

export function bootstrapProductViewer() {
    const root = document.getElementById('product-compare-3d');
    if (!root) {
        return;
    }
    const module = new ProductViewerModule(root);
    module.init();
    return module;
}

document.addEventListener('DOMContentLoaded', () => {
    bootstrapProductViewer();
});

