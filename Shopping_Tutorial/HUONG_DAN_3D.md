# 🎮 Hướng dẫn sử dụng chức năng 3D

## 📋 Mục lục
1. [Cài đặt ban đầu](#cài-đặt-ban-đầu)
2. [Truy cập chức năng 3D](#truy-cập-chức-năng-3d)
3. [Sử dụng các tính năng](#sử-dụng-các-tính-năng)
4. [Upload file 3D Model](#upload-file-3d-model)
5. [Troubleshooting](#troubleshooting)

---

## 🚀 Cài đặt ban đầu

### Bước 1: Tạo Migration và Update Database

Mở **Package Manager Console** trong Visual Studio:
- Tools → NuGet Package Manager → Package Manager Console

Hoặc mở **Terminal** và chạy:

```bash
# Tạo migration
dotnet ef migrations add Add3DModelsAndAnnotations

# Apply migration vào database
dotnet ef database update
```

### Bước 2: Kiểm tra Database

Sau khi migration thành công, bạn sẽ có 3 bảng mới:
- `Product3DModels` - Lưu thông tin file 3D
- `ProductAnnotations` - Lưu các chú thích
- `ProductConfigurations` - Lưu cấu hình tùy chỉnh

---

## 🎯 Truy cập chức năng 3D

### Cách 1: Từ trang chi tiết sản phẩm

1. Vào trang **chi tiết sản phẩm** (Product/Details)
2. Tìm nút **"Xem 3D / AR/VR"** (màu xanh, có icon cube)
3. Click vào nút để mở trang 3D Viewer

### Cách 2: Từ trang so sánh sản phẩm

1. Vào trang **So sánh sản phẩm** (Home/CompareMany)
2. Sau phần chat so sánh, có phần **"🔧 Tùy chỉnh sản phẩm"**
3. Mỗi sản phẩm có nút **"Xem 3D"** để mở 3D viewer

### Cách 3: Truy cập trực tiếp

URL: `/Product/View3D?id={ProductId}`

Ví dụ: `https://localhost:5001/Product/View3D?id=1`

---

## 🎨 Sử dụng các tính năng

### 1. 🛒 3D Shopping Cart (Giỏ hàng 3D)

**Tính năng:**
- Xem sản phẩm trong không gian 3D
- Xoay, zoom, pan model bằng chuột
- Thêm vào giỏ hàng trực tiếp từ 3D viewer

**Cách sử dụng:**
1. Mở trang 3D Viewer
2. Sử dụng chuột để:
   - **Click + Drag**: Xoay model
   - **Scroll**: Zoom in/out
   - **Right Click + Drag**: Pan camera
3. Click nút **"🛒 Thêm vào giỏ hàng"** ở panel điều khiển

### 2. 💬 Annotation (Chú thích)

**Tính năng:**
- Xem các chú thích có sẵn trên model
- Click vào marker đỏ để xem popup thông tin
- Thêm chú thích mới

**Cách sử dụng:**

**Xem chú thích:**
- Click vào các **marker đỏ** (hình tròn nhấp nháy) trên model
- Popup sẽ hiển thị tiêu đề và nội dung chú thích

**Thêm chú thích mới:**
1. Click nút **"➕ Thêm chú thích"**
2. Nhập tiêu đề
3. Nhập nội dung
4. Chú thích sẽ được thêm vào vị trí mặc định (giữa model)

**Chú thích mặc định:**
- Màn hình (Screen)
- Bàn phím (Keyboard)
- Cổng kết nối (Ports)

### 3. 🔍 AR/VR Mode

**Tính năng:**
- Xem sản phẩm trong AR (Augmented Reality)
- Xem sản phẩm trong VR (Virtual Reality)
- Sử dụng WebXR API

**Cách sử dụng:**

**AR Mode:**
1. Click nút **"🥽 AR Mode"**
2. Cho phép truy cập camera (nếu được hỏi)
3. Đặt model vào không gian thực
4. Di chuyển điện thoại/thiết bị để xem từ các góc độ

**Lưu ý AR:**
- Chỉ hoạt động trên **Chrome Android** hoặc **Safari iOS**
- Cần thiết bị có camera
- Cần kết nối HTTPS (hoặc localhost)

**VR Mode:**
1. Click nút **"🥽 VR Mode"**
2. Kết nối headset VR (nếu có)
3. Sử dụng controller để tương tác

**Lưu ý VR:**
- Cần trình duyệt hỗ trợ WebXR
- Cần headset VR (Oculus, HTC Vive, etc.)
- Có thể test trên desktop với VR simulator

### 4. 🔧 Configurator (Tùy chỉnh)

**Tính năng:**
- Thay đổi màu sắc sản phẩm
- Thay đổi vật liệu (kim loại, thủy tinh, gỗ, etc.)
- Bật/tắt các linh kiện

**Cách sử dụng:**

**Từ trang 3D Viewer:**
1. Scroll xuống phần **"🔧 Tùy chỉnh sản phẩm"**
2. Chọn màu từ bảng màu
3. Chọn vật liệu từ dropdown
4. Tick/untick các linh kiện

**Từ trang So sánh:**
1. Vào trang **So sánh sản phẩm**
2. Scroll xuống phần **"🔧 Tùy chỉnh sản phẩm"**
3. Mỗi sản phẩm có configurator riêng:
   - Chọn màu
   - Chọn vật liệu
   - Bật/tắt linh kiện
4. Click **"Thêm vào giỏ"** để lưu cấu hình

**Các tùy chọn:**

**Màu sắc:**
- 🔵 Xanh dương (#3498db)
- 🔴 Đỏ (#e74c3c)
- 🟢 Xanh lá (#2ecc71)
- 🟠 Cam (#f39c12)
- 🟣 Tím (#9b59b6)
- ⚫ Xám đen (#34495e)

**Vật liệu:**
- Tiêu chuẩn
- Kim loại (Metal)
- Thủy tinh (Glass)
- Nhựa (Plastic)
- Gỗ (Wood)
- Carbon Fiber

**Linh kiện:**
- ✅ Màn hình
- ✅ Bàn phím
- ✅ Pin
- ✅ Camera
- ✅ Loa

---

## 📤 Upload file 3D Model

### Bước 1: Chuẩn bị file 3D

**Định dạng hỗ trợ:**
- `.glb` (Khuyến nghị - nhẹ, nhanh)
- `.gltf` (với file .bin và textures)
- `.obj` (cần file .mtl)

**Nơi lưu file:**
- Tạo thư mục: `wwwroot/models/3d/`
- Upload file vào thư mục này

### Bước 2: Thêm vào Database

**Cách 1: Qua Admin Panel (nếu có)**
1. Vào Admin → Products
2. Edit sản phẩm
3. Thêm đường dẫn file 3D

**Cách 2: Qua SQL Script**

```sql
INSERT INTO Product3DModels (ProductID, Model3DPath, SupportAR, SupportVR, DefaultScale, CameraPositionX, CameraPositionY, CameraPositionZ, CreatedDate)
VALUES (1, '/models/3d/product1.glb', 1, 1, 1.0, 0, 2, 5, GETDATE());
```

**Cách 3: Qua Code (tạo Controller action)**

Thêm vào `ProductController.cs`:

```csharp
[HttpPost]
public async Task<IActionResult> Upload3DModel(long productId, IFormFile modelFile)
{
    if (modelFile == null || modelFile.Length == 0)
        return BadRequest("No file uploaded");

    var fileName = $"product_{productId}_{DateTime.Now.Ticks}.glb";
    var filePath = Path.Combine("wwwroot", "models", "3d", fileName);

    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await modelFile.CopyToAsync(stream);
    }

    var product3D = new Product3DModel
    {
        ProductID = productId,
        Model3DPath = $"/models/3d/{fileName}",
        SupportAR = true,
        SupportVR = true,
        DefaultScale = 1.0m,
        CameraPositionX = 0,
        CameraPositionY = 2,
        CameraPositionZ = 5
    };

    _dataContext.Product3DModels.Add(product3D);
    await _dataContext.SaveChangesAsync();

    return Ok(new { message = "3D model uploaded successfully", path = product3D.Model3DPath });
}
```

### Bước 3: Load model trong View3D

File `View3D.cshtml` hiện đang dùng geometry mặc định (BoxGeometry). Để load file 3D thật:

```javascript
// Thay thế phần tạo geometry mặc định
const loader = new THREE.GLTFLoader();
loader.load('@Model.Product3D?.Model3DPath', (gltf) => {
    const model = gltf.scene;
    model.scale.set(
        @(Model.Product3D?.DefaultScale ?? 1.0),
        @(Model.Product3D?.DefaultScale ?? 1.0),
        @(Model.Product3D?.DefaultScale ?? 1.0)
    );
    scene.add(model);
    productMesh = model; // Lưu reference để thao tác sau
}, undefined, (error) => {
    console.error('Error loading 3D model:', error);
    // Fallback về geometry mặc định
});
```

---

## 🛠️ Troubleshooting

### Lỗi: "The name 'keyframes' does not exist"
**Giải pháp:** Đã sửa - dùng `@@keyframes` thay vì `@keyframes` trong Razor

### 3D Model không hiển thị
**Kiểm tra:**
1. File có tồn tại không?
2. Đường dẫn đúng không?
3. Console có lỗi JavaScript không?
4. File format có đúng không? (nên dùng .glb)

### AR/VR không hoạt động
**Nguyên nhân:**
- Trình duyệt không hỗ trợ WebXR
- Chưa bật HTTPS (cần cho AR)
- Thiết bị không có camera/VR headset

**Giải pháp:**
- Dùng Chrome trên Android hoặc Safari trên iOS
- Test trên localhost (được coi là secure)
- Kiểm tra `navigator.xr` có tồn tại không

### Annotation không hiển thị
**Kiểm tra:**
1. Có data trong database không?
2. JavaScript console có lỗi không?
3. Raycasting có hoạt động không?

### Configurator không lưu
**Kiểm tra:**
1. Migration đã chạy chưa?
2. Database có bảng `ProductConfigurations` chưa?
3. AJAX call có thành công không? (check Network tab)

---

## 📝 Ghi chú

- **File 3D nên < 5MB** để load nhanh
- **Sử dụng .glb format** thay vì .gltf (nhẹ hơn)
- **Test trên nhiều trình duyệt** (Chrome, Firefox, Edge)
- **Mobile responsive** - đã được thiết kế responsive

---

## 🎓 Tài liệu tham khảo

- [Three.js Documentation](https://threejs.org/docs/)
- [WebXR API](https://developer.mozilla.org/en-US/docs/Web/API/WebXR_Device_API)
- [GLTF Format](https://www.khronos.org/gltf/)

---

**Chúc bạn sử dụng vui vẻ! 🎉**

