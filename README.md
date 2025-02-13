# PLATEAU-SNAP-Server

デジタルツインの実現に向けたクラウドソーシング型 3D 都市モデル作成システム(PLATEAU SNAP)のバックエンドサーバー

## Runtime

### Version

[![](https://img.shields.io/static/v1?style=flat=square&logo=GitHub&logoColor=FFFFFF&label=PLATEAU&nbsp;SNAP&message=0.0.1&color=0e6da0)](https://github.com/Gentlymad-Studios/PackageManagerTools)

### Support

[![](https://img.shields.io/static/v1?style=flat=square&logo=dotnet&logoColor=FFFFFF&label=dotnet&message=8.0&color=0e6da0)](https://dotnet.microsoft.com/ja-jp/download/dotnet/8.0)

### Dependencies

[![](https://img.shields.io/static/v1?style=flat=square&logo=postgresql&logoColor=FFFFFF&label=PostgreSQL&message=16.x&color=0e6da0)](https://www.postgresql.org/docs/16/index.html)
[![](https://img.shields.io/static/v1?style=flat=square&logoColor=FFFFFF&label=PostGIS&message=3.x&color=0e6da0)](https://postgis.net/development/developer_docs/)

## 事前準備

### DB 構築

[CityGML-validation-function](https://github.com/Project-PLATEAU/CityGML-validation-function) の手順に従い、3D City Database を構築します。

[docker-compose.local.yml](./src/docker-compose.local.yml) の DB を使用する場合、`plateau.snap.server` を一時的にコメントアウトして起動して DB を構築します。

```yml
services:
  #plateau.snap.server:
  #  image: ${DOCKER_REGISTRY-}plateausnapserver
  #  build:
  #    context: .
  #    dockerfile: PLATEAU.Snap.Server/Dockerfile
  #  ports:
  #  - "8080:8080"
  #  - "8081:8081"
  postgres:
    image: postgis/postgis:16-3.4
    environment:
    ...
```

### テーブル追加作成

事前に 3D City Database に CityGML がインポートされている必要があります。  
※この手順実行後に追加でインポートした場合、surface_centroid テーブルを再作成する必要があります。

init.sql を実行します。以下はコマンドラインで実行する例です。

```
psql -f init.sql -h localhost -p 25432 -U postgres -d citydb_v4
```

### SRID 特定用データのインポート

city_boundary テーブルに city_boundary.csv をインポートします。以下はコマンドラインで実行する例です。

```
psql -c "\copy city_boundary from city_boundary.csv delimiter ',' csv;" -h localhost -p 25432 -U postgres -d citydb_v4
```

### アプリケーション設定

- PLATEAU.Snap.Server/appsettings.json を開き、必要に応じて設定を変更します。
- デバッグ実行する場合は、PLATEAU.Snap.Server/appsettings.Development.json に設定します。
- 実行環境にデプロイする場合は、`S3:Bucket` のように `:` 区切りの環境変数で指定します。

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Database": {
    "Host": "localhost",
    "Port": 25432,
    "Username": "postgres",
    "Password": "password",
    "Database": "citydb_v4"
  },
  "AWS": {
    "Profile": "snap-dev-user",
    "Region": "ap-northeast-1"
  },
  "S3": {
    "Bucket": "plateausnap-dev"
  },
  "App": {
    "ApiKey": "123"
  }
}
```

### AWS 認証情報

- [Configuration and credential file settings in the AWS CLI](https://docs.aws.amazon.com/ja_jp/cli/v1/userguide/cli-configure-files.html) を参考に、AWS 認証情報を作成します。
- 実行環境にデプロイする場合は、以下の環境変数を使用します。
  - AWS_ACCESS_KEY_ID
  - AWS_SECRET_ACCESS_KEY

### ジオイド・モデルの配置

[基盤地図情報ダウンロードサービス](https://fgd.gsi.go.jp/download/geoid.php) から gsigeo2011_ver2_2_asc.zip をダウンロードします。  
解凍した中にある gsigeo2011_ver2_2.asc を実行時の dll と同じ場所に配置します。デバッグ実行する場合は src/PLATEAU.Snap.Server/bin/Debug/net8.0/gsigeo2011_ver2_2.asc になります。  
※実行環境では S3 からダウンロードする仕組みになっています。

## 実行

### Visual Studio

1. PLATEAU.Snap.Server.sln を開きます。
2. PLATEAU.Snap.Server をスタートアッププロジェクトに設定します。
3. https でデバッグを開始します。

### Visual Studio Code

[チュートリアル: Visual Studio Code を使用して .NET コンソール アプリケーションをデバッグする](https://learn.microsoft.com/ja-jp/dotnet/core/tutorials/debugging-with-visual-studio-code?pivots=dotnet-8-0) を参照してください。

## 参照

- [デプロイ手順](./docs/deploy.md)
- [境界データ作成手順](./docs/create_city_boundary_csv.md)

## プルリクエストの作成手順

- Github のリポジトリの画面上部から `Pull requests` タブを選択します
- push したブランチの `Compare & Pull request` または `New pull request` からブランチを選択してから `Create pull request` を選択してください

### 1). 機能実装のプルリクエストの場合

- 表示されたテンプレートをそのまま使ってください
- (または URL の末尾に `?template=feature_template.md` を追記してください)

### 2). 不具合修正のプルリクエストの場合

- URL の末尾に `?template=fix_template.md` を追記してください

### 3). その他のプルリクエストの場合

- テンプレートは作成していないです
- 必要に応じて追加してください
