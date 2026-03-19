# デプロイ手順書

## アーキテクチャ

アーキテクチャは以下の通りです。

![アーキテクチャ図](../resources/architectur.drawio.svg)

## デプロイ (Terraform)

アーキテクチャ図の通り、PLATEAU-SNAP-Server だけではなく PLATEAU-SNAP-CMS も同時にデプロイされます。  
事前に PLATEAU-SNAP-Server と PLATEAU-SNAP-CMS のソースコードを clone しておきます。

### Terraform の設定

terraform/environments/dev/terraform.tf と terraform/environments/dev/provider.tf を編集して設定をします。  
terraform/environments/dev/provider.tf で指定する backend の s3 は事前に作成し、適切なアクセス許可をしておく必要があります。  
※tfstate を共有する必要がない場合は backend をコメントアウトしてください。

### Terraform の初期化

```bash
terraform init
```

※一度デプロイされているなどで以下のようなメッセージが表示た場合、`-reconfigure` オプションを付けて実行します。

```
Error: Backend initialization required: please run "terraform init"
│
│ Reason: Backend configuration block has changed
│
│ The "backend" is the interface that Terraform uses to store state,
│ perform operations, etc. If this message is showing up, it means that the
│ Terraform configuration you're using is using a custom configuration for
│ the Terraform backend.
│
│ Changes to backend configurations require reinitialization. This allows
│ Terraform to set up the new configuration, copy existing state, etc. Please run
│ "terraform init" with either the "-reconfigure" or "-migrate-state" flags to
│ use the current configuration.
```

### ECR のみデプロイ

各コンテナが ECR のイメージを参照していますが、現時点ではイメージを push するコードは terraform に存在せず、手動で build & push する設定になっています。  
このため、先に ECR のみデプロイします。

```
terraform apply -target=aws_ecr_repository.default -target=aws_ecr_repository.geo_lambda_image -target=aws_ecr_repository.export_lambda_image  -target=aws_ecr_repository.cms
```

### build & push

ECR にログインします。  
初回は administrator で実行しますが、デプロイ後はより権限が限定された snap-dev-user でログインするのが適切です。

```
AWS_PROFILE="administrator"
AWS_REGION="ap-northeast-1"
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --profile "$AWS_PROFILE" --query Account --output text)
ECR_REGISTRY="$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com"
aws ecr get-login-password --region "$AWS_REGION" --profile "$AWS_PROFILE" | docker login --username AWS --password-stdin "$ECR_REGISTRY"
```

```
cd ~/PLATEAU-SNAP-Server/src
docker image build -f PLATEAU.Snap.Server/Dockerfile -t plateausnap-dev .
docker image build -f PLATEAU.Snap.Server.Lambda/Dockerfile -t export-lambda .
cd ~/PLATEAU-SNAP-Server/lambdas
docker image build -f Dockerfile -t geo-lambda .
cd ~/PLATEAU-SNAP-CMS
docker image build -f docker/remix/Dockerfile -t plateausnap-dev-cms .

docker tag plateausnap-dev:latest "$ECR_REGISTRY"/plateausnap-dev:latest
docker tag export-lambda:latest "$ECR_REGISTRY"/export-lambda:latest
docker tag geo-lambda:latest "$ECR_REGISTRY"/geo-lambda:latest
docker tag plateausnap-dev-cms:latest "$ECR_REGISTRY"/plateausnap-dev-cms:latest

docker push "$ECR_REGISTRY"/plateausnap-dev:latest
docker push "$ECR_REGISTRY"/export-lambda:latest
docker push "$ECR_REGISTRY"/geo-lambda:latest
docker push "$ECR_REGISTRY"/plateausnap-dev-cms:latest
```

### http 接続へ変更

初期デプロイでは、証明書の取得に失敗するため https ではなく http 接続でデプロイするようにします。
https.tf を [こちらのファイルに](../resources/https.tf) 差し替えます。  
Route 53にドメインの委譲が完了したら、https.tf を元に戻して再度デプロイします。

### Terraform の実行

terraform.tf で変数を設定して terraform でデプロイします。  
アーキテクチャ図のところまではデプロイされます。

```bash
terraform plan
terraform apply
```

EC2 のユーザデータによってある程度はセットアップも行われます。  
ユーザデータによるセットアップの内容は以下の通りです。  
※詳細は terraform/script/init.sh を参照してください。もしセットアップが不足している場合は、terraform/script/init.sh の該当する箇所のコマンドを実行してください。

- タイムゾーンの設定
- ロケールの設定
- キーボードレイアウトの設定
- dnf の更新
- PostgreSQL のインストール
- docker のインストール
- root じゃなくても docker を起動できるようにする
- docker サービスの起動、自動起動設定
- docker-compose のインストール
- Java のインストール
- 3DCityDB-Importer-Exporter のインストール
- ADE プラグインのインストール
- i-UR 1.4 拡張モジュールのインストール
- 3D City Database のセットアップツールのインストール

## AWS マネジメントコンソールでの作業

### ジオイド・モデルの配置

[基盤地図情報ダウンロードサービス](https://service.gsi.go.jp/kiban/app/geoid/) から gsigeo2011_ver2_2_asc.zip をダウンロードします。  
解凍した中にある gsigeo2011_ver2_2.asc をデプロイした S3 の直下にアップロードします。

## EC2 での作業

EC2 に SSH で接続して作業します。  
以降の手順で `<>` で囲われている箇所は環境にあわせて適宜設定します。

### 追加ディスクのマウント

データのサイズが大きいとディスクが足りなくなります。  
ディスク自体は terraform で作成されているのでマウントします。

#### root ユーザに切り替え

```bash
sudo su -
```

#### フォーマットして /data にマウント

```bash
file -s /dev/sdf
mkfs -t ext4 <デバイス名>
mkdir /data
chmod 777 /data
mount <デバイス名> /data
```

#### 再起動時の自動マウント設定

UUID を確認。

```bash
blkid <デバイス名>
vi /etc/fstab
```

UUID は blkid コマンドで取得したものを指定します。

```
UUID=76e66f84-855d-4e36-ab81-b2a02f2746b6     /data       ext4   defaults          1   1
```

### データベース作成

master_user でログインします。

```bash
psql -h <host> -p <port> -U <user> -d postgres
```

```sql
CREATE USER citydb_user WITH LOGIN PASSWORD '<password>';
CREATE DATABASE citydb_v4 WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE = 'ja_JP.UTF8' OWNER citydb_user;
\c citydb_v4
CREATE EXTENSION postgis;
--CREATE EXTENSION postgis_sfcgal; RDSでは未対応だった
CREATE EXTENSION postgis_raster;
```

citydb_user でログインできることを確認します。

```bash
psql -h <host> -p <port> -U citydb_user -d citydb_v4
```

### 3DCityDB の構築

#### 接続先を設定

```bash
cd ~/3DCityDB-Importer-Exporter/3dcitydb/postgresql/ShellScripts/Unix
vi CONNECTION_DETAILS.sh
```

```
export PGBIN=/usr/bin/psql
export PGHOST=<host>
export PGPORT=<port>
export CITYDB=citydb_v4
export PGUSER=citydb_user
```

#### シェル実行

```bash
chmod 755 CREATE_DB.sh
./CREATE_DB.sh
```

最初の SRID の指定で 6697 を指定し、それ以外はデフォルトとします。

#### インポートツールにパスを通す

```bash
vi ~/.bashrc
```

```
export PATH=~/3DCityDB-Importer-Exporter/bin:$PATH
```

```bash
source ~/.bashrc
```

#### インポート

拡張ディスク上で作業しないとディスクがたりなくなるため注意してください。  
※中央区だけで 10GB 程度ありました。

```bash
cd /data
wget https://assets.cms.plateau.reearth.io/assets/f4/d27eb2-1312-4406-8acf-54f6c5384ae6/13102_chuo-ku_city_2023_citygml_1_op.zip
unzip 13102_chuo-ku_city_2023_citygml_1_op.zip -d 13102_chuo-ku_city_2023_citygml_1_op
```

`impexp import` でインポートします。  
詳細は [impexp-cli-import-command](https://3dcitydb-docs.readthedocs.io/en/latest/impexp/cli/import.html#impexp-cli-import-command) を参照してください。

```bash
impexp import -H <host> -P <port> -d citydb_v4 -u citydb_user -p <password> 53394622_bldg_6697_op.gml
```

※EC2 や DB のスペックが低いとかなり時間がかかります。

#### 追加テーブル作成 & 重心データ作成

事前に SCP などで init.sql と city_boundary.csv をアップロードしておきます。  
※init.sql 実行後に追加で `impexp import` した場合、init.sql を再度実行する必要があることに注意してください。

```bash
psql -f init.sql -h <host> -p <port> -U citydb_user -d citydb_v4
```

#### 境界データインポート

事前に [境界データ作成](./cityBoundaryMan.md) を行い、city_boundary.csv を作成しておく必要があります。  
更新が不要な場合、data/city_boundary.7z を解凍して使用してください。  
※座標系がかわるなどの理由がない限りは更新不要です。

```bash
psql -c "\copy citydb.city_boundary from city_boundary.csv delimiter ',' csv;" -h <host> -p <port> -U citydb_user -d citydb_v4
```

### アプリの環境設定

#### Secrets Manager に Secret を登録

AWS マネジメントコンソールに接続し、Secret にDBの接続情報を追記します。  
Database**Database, Database**Username, Database\_\_Password に [データベース作成](#データベース作成) で登録した情報を追加します。  
JSON View で設定する場合は `"<port>"` の部分もダブルクォーテーションで囲わないといけないため注意してください。

```json
{
  "Database__Host": "<host>",
  "Database__Port": "<port>",
  "Database__Database": "citydb_v4",
  "Database__Username": "citydb_user",
  "Database__Password": "<password>",
  "S3__Bucket": "<bucket>",
  "App__ApiKey": "<api_key>"
}
```

#### ECS の再起動

Secrets Manager の変更を ECS に適用します。
AWS マネジメントコンソールに接続し、ECS の API サービスに対して新しいデプロイを強制します。
