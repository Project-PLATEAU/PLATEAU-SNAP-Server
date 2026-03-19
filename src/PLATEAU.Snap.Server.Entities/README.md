# scaffold の手順

DbContext のコードとデータベースのエンティティ型を生成する手順を記載する。

## 環境構築

### .NET 8.0 のインストール (または更新)

[.NET 8.0 のダウンロード](https://dotnet.microsoft.com/ja-jp/download/dotnet/8.0)

### Entity Framework Core ツール のインストール
管理者権限が必要。
```
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```

## Scaffold

事前にDBを構築しておく。  
下記コマンドでモデルクラスを生成する。Modelsフォルダ配下はすべて上書きされるため、注意が必要。

```
dotnet ef dbcontext scaffold "Host=localhost;Port=25432;Database=citydb_v4;Username=postgres;Password=password" Npgsql.EntityFrameworkCore.PostgreSQL --context CitydbV4DbContext --context-namespace PLATEAU.Snap.Server.Entities --output-dir Models --force --table surface_geometry --table images --table image_surface_relations --table city_boundary --table building --table cityobject --table surface_images --table roof_surfaces --table building_faces --table textureparam --table surface_data --table tex_image --table objectclass --table appearance --table appear_to_surface_data --table building_appearance --table jobs
```
