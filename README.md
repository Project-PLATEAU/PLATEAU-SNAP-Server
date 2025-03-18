# PLATEAU-SNAP-Server
![アーキテクチャ](./docs/images/architectur.drawio.svg)

## 1. 概要

本リポジトリでは、2024 年度の Project PLATEAU で開発した「PLATEAU-SNAP-Server」のソースコードを公開しています。  
「PLATEAU-SNAP-Server」は、デジタルツインの実現に向けたクラウドソーシング型 3D 都市モデル作成システム (PLATEAU SNAP) のバックエンドサーバーです。

## 2. 「PLATEAU-SNAP-Server」について

スマートフォンで撮影した画像をもとに 3D 都市モデルの建物地物のテクスチャ(地物の外観)を抽出・生成し、3D 都市モデルのデータベースに登録・蓄積可能なツール「PLATEAU SNAP」を開発しました。

本システムは、同ツールのバックエンドサーバーとして機能し、撮影可能面一覧の取得および撮影した画像をアップロードする機能を提供します。フロントエンドのリポジトリは 「[PLATEAU-SNAP-App](https://github.com/Project-PLATEAU/PLATEAU-SNAP-App)」にて管理されています。フロントエンドの詳細については、そちらのリポジトリをご確認ください。

## 3. 利用手順

本システムの構築手順については、[操作マニュアル](https://project-plateau.github.io/PLATEAU-SNAP-Server/index.html)を参照してください。

## 4. システム概要

| 機能名                 | 機能説明                                                                             |
| ---------------------- | ------------------------------------------------------------------------------------ |
| 撮影可能面一覧取得機能 | 現在位置で撮影した際に、画像をテクスチャとして貼り付け可能な面を計算し、取得する機能 |
| 画像アップロード機能   | 撮影画像と貼り付け先の面情報をデータベースにアップロードする機能                     |

## 5. 利用技術

| 種別               | 名称                                                                                | バージョン  | 詳細                                                                                 |
| ------------------ | ----------------------------------------------------------------------------------- | ----------- | ------------------------------------------------------------------------------------ |
| プログラミング言語 | [C#](https://learn.microsoft.com/ja-jp/dotnet/csharp/)                              | 12~         | プログラミング言語。本ツールは全て C#で実装する。                                    |
| フレームワーク     | [ASP.NET Core](https://learn.microsoft.com/ja-jp/aspnet/core/?view=aspnetcore-8.0)  | 8.0~        | C#の Web アプリフレームワーク。Web API テンプレートを使用。                          |
| データベース       | [PostgreSQL](https://GitHub.com/postgres)                                           | 16~         | オープンソースのデータベース管理システム。                                           |
|                    | [PostGIS](https://GitHub.com/postgis)                                               | 3.4~        | PostgreSQL で地理情報システム(GIS)を扱えるようにするためのオープンソースの拡張機能。 |
| ライブラリ         | [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite)            | 2.5.0~      | オープンソースの.NET 用の GIS ライブラリ。                                           |
|                    | [Npgsql](https://github.com/npgsql/npgsql)                                          | 8.0.6~      | オープンソースの ADO.NET ドライバ。                                                  |
|                    | [AWSSDK](https://www.nuget.org/packages?q=id%3AAWSSDK%20owner%3Aawsdotnet)          | 3.7.400.86~ | .NET 用の AWS にアクセスするための SDK。                                             |
|                    | [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) | 7.2.0~      | オープンソースの.NET 用の Swagger ライブラリ。                                       |

## 6. 動作環境

| 項目         | 最小動作環境                        | 推奨動作環境       |
| ------------ | ----------------------------------- | ------------------ |
| OS           | Microsoft Windows 11 / ubuntu 22.04 | 同左               |
| CPU          | Intel Core i3 以上                  | Intel Core i5 以上 |
| メモリ       | 8GB                                 | 16GB               |
| ネットワーク | インターネット接続                  | 同左               |

## 7. 本リポジトリのフォルダ構成

本リポジトリのソースコードは src 内に以下のモジュールごとに配置されています。
| フォルダ名 | 詳細 |
| --- | --- |
|PLATEAU.Snap.Models|モデルクラス|
|PLATEAU.Snap.Server|Web API のアプリケーション本体|
|PLATEAU.Snap.Server.Entities|DB から自動生成した O/RM|
|PLATEAU.Snap.Server.Geoid|ジオイド高を利用して楕円体高に補正する機能を提供|
|PLATEAU.Snap.Server.Repositories|リポジトリ層のインターフェース|
|PLATEAU.Snap.Server.Repositories.PostgreSQL|リポジトリ層の PostgreSQL 実装|
|PLATEAU.Snap.Server.Repositories.S3|リポジトリ層の S3 実装|
|PLATEAU.Snap.Server.Services|サービス層のインターフェース|
|PLATEAU.Snap.Server.Services.Impl|サービス層の実装|

## 8. ライセンス

- ソースコード及び関連ドキュメントの著作権は国土交通省に帰属します。
- 本ドキュメントは[Project PLATEAUのサイトポリシー](https://www.mlit.go.jp/plateau/site-policy/)（CCBY4.0及び公共データ利用規約第1.0版）に従い提供されています。

## 9. 注意事項

- 本リポジトリは参考資料として提供しているものです。動作保証は行っていません。
- 本リポジトリについては予告なく変更又は削除をする可能性があります。
- 本リポジトリの利用により生じた損失及び損害等について、国土交通省はいかなる責任も負わないものとします。

## 10. 参考資料

- 技術検証レポート: https://www.mlit.go.jp/plateau/file/libraries/doc/plateau_tech_doc_0119_ver01.pdf
