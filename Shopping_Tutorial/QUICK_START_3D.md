# ⚡ Quick Start - Chức năng 3D

## 🚀 Bước 1: Chạy Migration

```bash
# Trong Package Manager Console hoặc Terminal
dotnet ef migrations add Add3DModelsAndAnnotations
dotnet ef database update
```

## 🎯 Bước 2: Truy cập chức năng 3D

### Cách nhanh nhất:
1. Vào trang **chi tiết sản phẩm** bất kỳ
2. Click nút **"Xem 3D / AR/VR"** (màu xanh, có icon cube)
3. Trang 3D Viewer sẽ mở ra

**URL trực tiếp:** `/Product/View3D?id=1` (thay 1 bằng ID sản phẩm)

## 🎮 Sử dụng nhanh

### Xem 3D Model:
- **Click + Drag**: Xoay model
- **Scroll**: Zoom in/out
- **Right Click + Drag**: Pan camera

### Thêm vào giỏ hàng:
- Click nút **"🛒 Thêm vào giỏ hàng"** ở panel bên phải

### Xem chú thích:
- Click vào các **marker đỏ** (hình tròn nhấp nháy) trên model

### AR/VR:
- Click **"🥽 AR Mode"** hoặc **"🥽 VR Mode"**
- (Cần trình duyệt hỗ trợ WebXR)

### Tùy chỉnh:
- Scroll xuống phần **"🔧 Tùy chỉnh sản phẩm"**
- Chọn màu, vật liệu, bật/tắt linh kiện

## 📤 Thêm file 3D Model (Tùy chọn)

### Nếu muốn dùng file 3D thật:

1. **Tạo thư mục:**
   ```
   wwwroot/models/3d/
   ```

2. **Upload file .glb vào thư mục**

3. **Thêm vào database:**
   ```sql
   INSERT INTO Product3DModels (ProductID, Model3DPath, SupportAR, SupportVR, DefaultScale, CameraPositionX, CameraPositionY, CameraPositionZ, CreatedDate)
   VALUES (1, '/models/3d/product1.glb', 1, 1, 1.0, 0, 2, 5, GETDATE());
   ```

4. **Cập nhật View3D.cshtml** để load file (xem chi tiết trong HUONG_DAN_3D.md)

## ✅ Kiểm tra

- [ ] Migration đã chạy thành công
- [ ] Database có 3 bảng mới: Product3DModels, ProductAnnotations, ProductConfigurations
- [ ] Có thể truy cập `/Product/View3D?id=1`
- [ ] 3D viewer hiển thị được
- [ ] Có thể thêm vào giỏ hàng

## 🆘 Gặp lỗi?

Xem file **HUONG_DAN_3D.md** phần Troubleshooting

---

**Xong! Bạn đã sẵn sàng sử dụng chức năng 3D! 🎉**

