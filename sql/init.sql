DROP TABLE IF EXISTS citydb.surface_centroid;

CREATE TABLE IF NOT EXISTS citydb.images (
  id bigserial primary key,
  uri varchar(256) NOT NULL,
  from_latitude double precision NOT NULL,
  from_longitude double precision NOT NULL,
  from_altitude double precision NOT NULL,
  to_latitude double precision NOT NULL,
  to_longitude double precision NOT NULL,
  to_altitude double precision NOT NULL,
  roll double precision NOT NULL,
  coordinates geometry(Polygon) NOT NULL,
  thumbnail bytea NOT NULL,
  timestamp timestamp with time zone NOT NULL DEFAULT current_timestamp
);

CREATE TABLE IF NOT EXISTS citydb.image_surface_relations (
  id bigserial primary key,
  image_id bigint NOT NULL,
  gmlid varchar(256) NOT NULL,
  FOREIGN KEY (image_id) REFERENCES citydb.images(id),
  UNIQUE (image_id, gmlid)
);

CREATE TABLE IF NOT EXISTS citydb.city_boundary
(
    fid serial primary key,
    gst_css_name character varying COLLATE pg_catalog."default",
    system_number character varying COLLATE pg_catalog."default",
    area_code character varying COLLATE pg_catalog."default",
    pref_name character varying COLLATE pg_catalog."default",
    city_name character varying COLLATE pg_catalog."default",
    gst_name character varying COLLATE pg_catalog."default",
    css_name character varying COLLATE pg_catalog."default",
    area double precision,
    x_code double precision,
    y_code double precision,
    geom geometry(Geometry,4326)
);
CREATE INDEX IF NOT EXISTS city_boundary_geom_geom_idx ON citydb.city_boundary USING gist (geom);

CREATE TABLE IF NOT EXISTS citydb.town_boundary
(
    fid serial primary key,
    geom geometry(Geometry,4326),
    key_code character varying COLLATE pg_catalog."default",
    pref character varying COLLATE pg_catalog."default",
    city character varying COLLATE pg_catalog."default",
    s_area character varying COLLATE pg_catalog."default",
    pref_name character varying COLLATE pg_catalog."default",
    city_name character varying COLLATE pg_catalog."default",
    s_name character varying COLLATE pg_catalog."default",
    kigo_e character varying COLLATE pg_catalog."default",
    hcode bigint,
    area double precision,
    perimeter double precision,
    h27kaxx_ bigint,
    h27kaxx_id bigint,
    ken character varying COLLATE pg_catalog."default",
    ken_name character varying COLLATE pg_catalog."default",
    sityo_name character varying COLLATE pg_catalog."default",
    gst_name character varying COLLATE pg_catalog."default",
    css_name character varying COLLATE pg_catalog."default",
    kihon1 character varying COLLATE pg_catalog."default",
    dummy1 character varying COLLATE pg_catalog."default",
    kihon2 character varying COLLATE pg_catalog."default",
    keycode1 character varying COLLATE pg_catalog."default",
    keycode2 character varying COLLATE pg_catalog."default",
    area_max_f character varying COLLATE pg_catalog."default",
    kigo_d character varying COLLATE pg_catalog."default",
    n_ken character varying COLLATE pg_catalog."default",
    n_city character varying COLLATE pg_catalog."default",
    kigo_i character varying COLLATE pg_catalog."default",
    moji character varying COLLATE pg_catalog."default",
    kbsum bigint,
    jinko bigint,
    setai bigint,
    x_code double precision,
    y_code double precision,
    kcode1 character varying COLLATE pg_catalog."default",
    area_code character varying COLLATE pg_catalog."default"
);
CREATE INDEX IF NOT EXISTS town_boundary_geom_geom_idx ON citydb.town_boundary USING gist (geom);

CREATE TABLE IF NOT EXISTS citydb.surface_centroid
(
    id integer primary key,
    gmlid varchar(256) NOT NULL,
    center geometry(Geometry,4326) NOT NULL,
    building_id integer NOT NULL
);

CREATE TABLE IF NOT EXISTS jobs (
    id bigserial PRIMARY KEY,
    type text NOT NULL,
    status text NOT NULL,
    parameter text,
    result_parameter text,
    message text,
    created_at timestamp with time zone NOT NULL DEFAULT current_timestamp,
    updated_at timestamp with time zone NOT NULL DEFAULT current_timestamp
);

WITH t AS (
    SELECT sg.*,b.id AS building_id FROM citydb.building AS b
    JOIN surface_geometry AS sg ON b.lod2_solid_id=sg.root_id
    WHERE parent_id IS NOT NULL AND is_composite = 0
)
INSERT INTO surface_centroid
SELECT id, gmlid, ST_Centroid(ST_Transform(ST_FlipCoordinates(ST_Force2D(geometry)), 4326)) as center, building_id FROM t;

CREATE INDEX IF NOT EXISTS surface_centroid_center_geom_idx ON citydb.surface_centroid USING gist (center);
CREATE INDEX IF NOT EXISTS surface_centroid_building_id_idx ON citydb.surface_centroid (building_id);

DROP view IF EXISTS building_appearance;
DROP view IF EXISTS building_faces;
DROP view IF EXISTS roof_surfaces;
DROP view IF EXISTS surface_images;

CREATE VIEW surface_images AS
SELECT
  b.id AS building_id,
  sg.id AS face_id,
  i.id AS image_id,
  sg.gmlid,
  i.thumbnail,
  i.coordinates,
  i.timestamp,
  i.uri,
  ST_Centroid(ST_Transform(ST_FlipCoordinates(ST_Force2D(geometry)), 4326)) as center
FROM images AS i
JOIN image_surface_relations AS r ON i.id=r.image_id
JOIN surface_geometry AS sg ON r.gmlid=sg.gmlid
JOIN thematic_surface ts ON sg.root_id = ts.lod2_multi_surface_id
JOIN building b ON ts.building_id = b.id;

CREATE VIEW roof_surfaces AS
SELECT
  b.id AS building_id,
  sg.id AS face_id,
  sg.gmlid,
  ST_Transform(ST_FlipCoordinates(ST_Force2D(sg.geometry)), 4326) As geom
FROM surface_geometry sg
JOIN thematic_surface ts ON sg.root_id = ts.lod2_multi_surface_id
JOIN building b ON ts.building_id = b.id
WHERE ts.objectclass_id = (
  SELECT id FROM objectclass WHERE classname = 'BuildingRoofSurface'
) AND sg.parent_id IS NOT NULL;

CREATE VIEW building_faces AS
SELECT building_id, face_id, image_id, gmlid, false AS is_ortho, thumbnail, coordinates, timestamp FROM surface_images
UNION ALL
SELECT building_id, face_id, NULL, gmlid, true AS is_ortho, NULL, NULL, NULL FROM roof_surfaces;

CREATE VIEW building_appearance AS
SELECT DISTINCT b.id AS building_id, a.id AS appearance_id FROM surface_geometry AS sg
JOIN textureparam AS tp ON sg.id=tp.surface_geometry_id
JOIN surface_data AS sd ON sd.id=tp.surface_data_id
JOIN appear_to_surface_data AS ats ON sd.id=ats.surface_data_id
JOIN appearance AS a ON ats.appearance_id=a.id
JOIN thematic_surface ts ON sg.root_id = ts.lod2_multi_surface_id
JOIN building b ON ts.building_id = b.id;
