# 境界データ作成手順

境界データの作成では、[MIERUNE/create_gpkg_for_city_boundaries](https://github.com/MIERUNE/create_gpkg_for_city_boundaries) のプログラムを使用しています。

## merge_city_boundary.gpkg の作成

### 環境構築

#### 依存パッケージのインストール

事前に python 3.8.x と pipenv をインストールしておきます。  
依存パッケージをインストールします。

```bash
pipenv sync
```

Windows の場合、環境によっては GDAL や Fiona のインストールでエラーが発生することがあります。  
エラーが発生した場合、対象のパッケージを手動でインストールします。(Anaconda を使うと出ないらしい)  
※実際の作業時には以下のパッケージをローカルにダウンロードし、手動でインストールしました。

- GDAL-3.6.2-cp38-cp38-win_amd64.whl
- Fiona-1.8.22-cp38-cp38-win_amd64.whl

#### 実行

```
cd scripts/
./run.sh
```

## merge_city_boundary.gpkg のインポート

merge_city_boundary.gpkg を PostgreSQL にインポートします。  
事前に以下の環境を構築しておきます。

- 作業用の PostgreSQL
- GDAL

`ogr2ogr` コマンドでインポートします。 `<>` で囲われている箇所は環境にあわせて適宜設定します。  
※ogr2ogr コマンドの詳細は [GDAL のドキュメント](https://gdal.org/en/stable/programs/ogr2ogr.html) を参照してください。

```
ogr2ogr -f "PostgreSQL" PG:"host=<host> user=<user> dbname=<dbname> password=<password>" merge_city_boundary.gpkg
```

### エクスポート

psql の `\copy` コマンドでエクスポートします。

```
psql -h <host> -p <port> -U <user> -d <dbname>
```

```
\copy merge_city_boundary (fid, gst_css_name, system_number, area_code, pref_name, city_name, gst_name, css_name, area, x_code, y_code, geom) TO '<output_path>/city_boundary.csv' DELIMITER ',' CSV ENCODING 'UTF8' QUOTE '"' ESCAPE '''';
```
