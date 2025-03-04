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
  timestamp timestamp  NOT NULL DEFAULT current_timestamp
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

CREATE TABLE IF NOT EXISTS citydb.surface_centroid
(
    id integer primary key,
    gmlid varchar(256) NOT NULL,
    center geometry(Geometry,4326)
);

WITH t AS (
    SELECT sg.* FROM citydb.building AS b
    JOIN surface_geometry AS sg ON b.lod1_solid_id=sg.root_id
    WHERE parent_id IS NOT NULL AND is_composite = 0
)
INSERT INTO surface_centroid
SELECT id, gmlid, ST_Centroid(ST_Transform(ST_FlipCoordinates(ST_Force2D(geometry)), 4326)) as center FROM t;

CREATE INDEX IF NOT EXISTS surface_centroid_center_geom_idx ON citydb.surface_centroid USING gist (center);
