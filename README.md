# PLATEAU-SNAP-Server

![アーキテクチャ](./docs/resources/architectur.drawio.svg)

## 1. 概要

本リポジトリでは、2024 ~ 2025 年度の Project PLATEAU で開発した「PLATEAU-SNAP-Server」のソースコードを公開しています。  
「PLATEAU-SNAP-Server」は、スマートフォン撮影画像を活用した3D都市モデルへのテクスチャ付与システム (PLATEAU SNAP) のバックエンドサーバーです。

## 2. 「PLATEAU-SNAP-Server」について

スマートフォンで撮影した画像をもとに 3D 都市モデルの建物物のテクスチャ(地物の外観)を抽出・生成し、PLATEAU SNAPのデータベースに登録・蓄積可能なツールを開発しました。

本システムは、同ツールのバックエンドサーバーとして機能します。

- スマートフォン向けアプリ「PLATEAU-SNAP-App」向けには、撮影可能面一覧の取得および撮影した画像を「PLATEAU SNAP Server」にアップロードする機能を提供します。
- ブラウザ向けアプリ「PLATEAU-SNAP-CMS」向けには、撮影した画像をもとにテクスチャを更新する機能を提供します。


各リポジトリの役割は以下の通りです。
- [**PLATEAU-SNAP-App**](https://github.com/Project-PLATEAU/PLATEAU-SNAP-App): 画像の撮影・アップロード
- **PLATEAU-SNAP-Server**: データの蓄積、画像処理、モデル生成（バックエンド処理）
- [**PLATEAU-SNAP-CMS**](https://github.com/Project-PLATEAU/PLATEAU-SNAP-CMS): テクスチャの生成・貼り付け、データベース更新、データ出力


## 3. 利用手順

本システムの構築手順については、[操作マニュアル](https://project-plateau.github.io/PLATEAU-SNAP-Server/index.html)を参照してください。

## 4. システム概要

| 機能名                 | 機能説明                                                                             |
| ---------------------- | ------------------------------------------------------------------------------------ |
| 撮影可能面一覧取得機能 | 現在位置で撮影した際に、画像をテクスチャとして貼り付け可能な面を計算し、取得する機能 |
| 画像アップロード機能   | 撮影画像と貼り付け先の面情報をデータベースにアップロードする機能                     |
| 建築物モデル取得機能   | 撮影画像が登録されている建築物モデルを取得する機能                                   |
| 面一覧取得機能         | 撮影画像が登録されている面と屋根面、つまり、テクスチャ更新可能な面を取得する機能     |
| 画像取得機能           | 撮影画像を取得する機能                                                               |
| 正射変換機能           | 撮影画像を正射変換する機能                                                           |
| 屋根面生成機能         | PLATEAU-Ortho から屋根面の画像を生成する機能                                         |
| データベース更新機能   | テクスチャを更新する機能                                                             |
| データ出力機能         | データベースから CityGML を出力する機能                                              |

## 5. 利用技術

Web API

| 種別               | 名称                                                                                | バージョン  | 詳細                                                                                  |
| ------------------ | ----------------------------------------------------------------------------------- | ----------- | ------------------------------------------------------------------------------------- |
| プログラミング言語 | [C#](https://learn.microsoft.com/ja-jp/dotnet/csharp/)                              | 12~         | プログラミング言語。本ツールは画像処理部分を除き C# で実装する。                      |
| フレームワーク     | [ASP.NET Core](https://learn.microsoft.com/ja-jp/aspnet/core/?view=aspnetcore-8.0)  | 8.0~        | C#の Web アプリフレームワーク。Web API テンプレートおよび Lambda テンプレートを使用。 |
| データベース       | [PostgreSQL](https://github.com/postgres)                                           | 16~         | オープンソースのデータベース管理システム。                                            |
|                    | [PostGIS](https://github.com/postgis)                                               | 3.4~        | PostgreSQL で地理情報システム(GIS)を扱えるようにするためのオープンソースの拡張機能。  |
| ライブラリ         | [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite)            | 2.5.0~      | オープンソースの.NET 用の GIS ライブラリ。                                            |
|                    | [Npgsql](https://github.com/npgsql/npgsql)                                          | 8.0.6~      | オープンソースの ADO.NET ドライバ。                                                   |
|                    | [AWSSDK](https://www.nuget.org/packages?q=id%3AAWSSDK%20owner%3Aawsdotnet)          | 3.7.400.86~ | .NET 用の AWS にアクセスするための SDK。                                              |
|                    | [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) | 7.2.0~      | オープンソースの.NET 用の Swagger ライブラリ。                                        |

画像処理

| 種別               | 名称                                                              | バージョン | 詳細                                                         |
| ------------------ | ----------------------------------------------------------------- | ---------- | ------------------------------------------------------------ |
| プログラミング言語 | [Python](https://www.python.org/)                                 | 3.12       | プログラミング言語。画像処理部分は Python で実装する。       |
| ライブラリ         | [boto3](https://github.com/boto/boto3)                            | 1.37.20    | AWS SDK for Python。AWS サービスへのアクセスを提供。         |
|                    | [opencv-python-headless](https://github.com/opencv/opencv-python) | 4.9.0.80   | オープンソースの画像処理ライブラリ。                         |
|                    | [numpy](https://github.com/numpy/numpy)                           | 1.26.4     | オープンソースの数値計算ライブラリ。                         |
|                    | [mercantile](https://github.com/mapbox/mercantile)                | 1.2.1      | オープンソースの Web メルカトルタイルユーティリティ。        |
|                    | [shapely](https://github.com/shapely/shapely)                     | 2.0.4      | オープンソースの幾何学的オブジェクトの操作・解析ライブラリ。 |

## 6. 動作環境

| 項目         | 最小動作環境                        | 推奨動作環境       |
| ------------ | ----------------------------------- | ------------------ |
| OS           | Microsoft Windows 11 / ubuntu 22.04 | 同左               |
| CPU          | Intel Core i3 以上                  | Intel Core i5 以上 |
| メモリ       | 8GB                                 | 16GB               |
| ネットワーク | インターネット接続                  | 同左               |

## 7. 本リポジトリのフォルダ構成

| フォルダ名 | 詳細 |
| --- | --- |
|data|環境設定で必要なデータ|
|docs|ドキュメント (操作マニュアル等)|
|lambdas/src/ortho_transform|正射変換機能を提供|
|lambdas/src/roof_extraction|屋根面生成機能を提供|
|lambdas/src/shared|共通で使うユーティリティ群|
|lambdas/src/texture_building|テクスチャ生成機能を提供|
|sql|環境設定で必要なSQL|
|src/PLATEAU.Snap.Models|モデルクラス|
|src/PLATEAU.Snap.Server|Web API のアプリケーション本体|
|src/PLATEAU.Snap.Server.Entities|DB から自動生成した O/RM|
|src/PLATEAU.Snap.Server.Geoid|ジオイド高を利用して楕円体高に補正する機能を提供|
|src/PLATEAU.Snap.Server.Lambda|非同期で動作する機能を提供|
|src/PLATEAU.Snap.Server.Repositories|リポジトリ層のインターフェース|
|src/PLATEAU.Snap.Server.Repositories.PostgreSQL|リポジトリ層の PostgreSQL 実装|
|src/PLATEAU.Snap.Server.Repositories.S3|リポジトリ層の S3 実装|
|src/PLATEAU.Snap.Server.Services|サービス層のインターフェース|
|src/PLATEAU.Snap.Server.Services.Impl|サービス層の実装|
|src/PLATEAU.Snap.Server.Test|Unit テスト|
|terraform|AWS にデプロイするためのコード|

## 8. ライセンス

- ソースコード及び関連ドキュメントの著作権は国土交通省に帰属します。
- 本ドキュメントは[Project PLATEAU のサイトポリシー](https://www.mlit.go.jp/plateau/site-policy/)（CCBY4.0 及び政府標準利用規約 2.0）に従い提供されています。

## 9. 注意事項

- 本リポジトリは参考資料として提供しているものです。動作保証は行っていません。
- 本リポジトリについては予告なく変更又は削除をする可能性があります。
- 本リポジトリの利用により生じた損失及び損害等について、国土交通省はいかなる責任も負わないものとします。

## 10. 参考資料

- [技術検証レポート](https://www.mlit.go.jp/plateau/file/libraries/doc/plateau_tech_doc_0139_ver01.pdf)
