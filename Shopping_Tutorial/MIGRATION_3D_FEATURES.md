# Hướng dẫn Migration cho tính năng 3D

## Các Model mới đã tạo:

1. **Product3DModel** - Lưu thông tin file 3D model của sản phẩm
2. **ProductAnnotationModel** - Lưu các annotation (chú thích) trên model 3D
3. **ProductConfigurationModel** - Lưu cấu hình tùy chỉnh (màu, vật liệu, linh kiện)

## Các thay đổi trong Model hiện có:

1. **ProductModel** - Thêm relationships:
   - `Product3D` (1-1)
   - `Annotations` (1-nhiều)
   - `Configurations` (1-nhiều)

2. **UserCartItem** - Thêm:
   - `ConfigurationId` (FK đến ProductConfigurationModel)
   - `ConfigurationData` (JSON string để lưu cấu hình)

## Cách tạo Migration:

Mở **Package Manager Console** hoặc **Terminal** và chạy lệnh:

```bash
dotnet ef migrations add Add3DModelsAndAnnotations
```

Sau đó apply migration:

```bash
dotnet ef database update
```

## Cấu trúc Database mới:

### Bảng: Product3DModels
- Id (PK)
- ProductID (FK → Products)
- Model3DPath (đường dẫn file .glb/.gltf)
- TexturePath
- DefaultScale
- CameraPositionX, Y, Z
- SupportAR, SupportVR
- CreatedDate, UpdatedDate

### Bảng: ProductAnnotations
- Id (PK)
- ProductID (FK → Products)
- Title, Content
- PositionX, Y, Z (vị trí 3D)
- MarkerColor
- DisplayOrder
- IsDefault
- CreatedByUserId
- CreatedDate, UpdatedDate

### Bảng: ProductConfigurations
- Id (PK)
- ProductID (FK → Products)
- UserId
- SelectedColor
- SelectedMaterial
- SelectedComponents (JSON)
- CustomPrice
- ConfigurationName
- IsDefault, IsInCart
- CartItemId
- CreatedDate, UpdatedDate

### Cập nhật: UserCartItems
- ConfigurationId (FK → ProductConfigurations, nullable)
- ConfigurationData (JSON string, nullable)

## Lưu ý:

- Tất cả các trường mới đều có thể NULL (trừ các trường required)
- Migration sẽ tự động tạo các bảng và foreign keys
- Nếu có lỗi, kiểm tra lại connection string trong appsettings.json

