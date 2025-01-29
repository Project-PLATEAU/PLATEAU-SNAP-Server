using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PLATEAU.Snap.Server.Entities.Models;

namespace PLATEAU.Snap.Server.Entities;

public partial class CitydbV4DbContext : DbContext
{
    public CitydbV4DbContext()
    {
    }

    public CitydbV4DbContext(DbContextOptions<CitydbV4DbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CityBoundary> CityBoundaries { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<ImageSurfaceRelation> ImageSurfaceRelations { get; set; }

    public virtual DbSet<SurfaceGeometry> SurfaceGeometries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=25432;Database=citydb_v4;Username=postgres;Password=password", x => x.UseNetTopologySuite());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("postgis")
            .HasPostgresExtension("postgis_raster");

        modelBuilder.Entity<CityBoundary>(entity =>
        {
            entity.HasKey(e => e.Fid).HasName("city_boundary_pkey");

            entity.ToTable("city_boundary", "citydb");

            entity.HasIndex(e => e.Geom, "city_boundary_geom_geom_idx").HasMethod("gist");

            entity.Property(e => e.Fid)
                .HasDefaultValueSql("nextval('city_boundary_fid_seq'::regclass)")
                .HasColumnName("fid");
            entity.Property(e => e.Area).HasColumnName("area");
            entity.Property(e => e.AreaCode)
                .HasColumnType("character varying")
                .HasColumnName("area_code");
            entity.Property(e => e.CityName)
                .HasColumnType("character varying")
                .HasColumnName("city_name");
            entity.Property(e => e.CssName)
                .HasColumnType("character varying")
                .HasColumnName("css_name");
            entity.Property(e => e.Geom)
                .HasColumnType("geometry(Geometry,4326)")
                .HasColumnName("geom");
            entity.Property(e => e.GstCssName)
                .HasColumnType("character varying")
                .HasColumnName("gst_css_name");
            entity.Property(e => e.GstName)
                .HasColumnType("character varying")
                .HasColumnName("gst_name");
            entity.Property(e => e.PrefName)
                .HasColumnType("character varying")
                .HasColumnName("pref_name");
            entity.Property(e => e.SystemNumber)
                .HasColumnType("character varying")
                .HasColumnName("system_number");
            entity.Property(e => e.XCode).HasColumnName("x_code");
            entity.Property(e => e.YCode).HasColumnName("y_code");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("images_pkey");

            entity.ToTable("images", "citydb");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('images_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.FromAltitude).HasColumnName("from_altitude");
            entity.Property(e => e.FromLatitude).HasColumnName("from_latitude");
            entity.Property(e => e.FromLongitude).HasColumnName("from_longitude");
            entity.Property(e => e.Roll).HasColumnName("roll");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timestamp");
            entity.Property(e => e.ToAltitude).HasColumnName("to_altitude");
            entity.Property(e => e.ToLatitude).HasColumnName("to_latitude");
            entity.Property(e => e.ToLongitude).HasColumnName("to_longitude");
            entity.Property(e => e.Uri)
                .HasMaxLength(256)
                .HasColumnName("uri");
        });

        modelBuilder.Entity<ImageSurfaceRelation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("image_surface_relations_pkey");

            entity.ToTable("image_surface_relations", "citydb");

            entity.HasIndex(e => new { e.ImageId, e.Gmlid }, "image_surface_relations_image_id_gmlid_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('image_surface_relations_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Gmlid)
                .HasMaxLength(256)
                .HasColumnName("gmlid");
            entity.Property(e => e.ImageId).HasColumnName("image_id");

            entity.HasOne(d => d.Image).WithMany(p => p.ImageSurfaceRelations)
                .HasForeignKey(d => d.ImageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("image_surface_relations_image_id_fkey");
        });

        modelBuilder.Entity<SurfaceGeometry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("surface_geometry_pk");

            entity.ToTable("surface_geometry", "citydb");

            entity.HasIndex(e => e.CityobjectId, "surface_geom_cityobj_fkx").HasAnnotation("Npgsql:StorageParameter:fillfactor", "90");

            entity.HasIndex(e => new { e.Gmlid, e.GmlidCodespace }, "surface_geom_inx").HasAnnotation("Npgsql:StorageParameter:fillfactor", "90");

            entity.HasIndex(e => e.ParentId, "surface_geom_parent_fkx").HasAnnotation("Npgsql:StorageParameter:fillfactor", "90");

            entity.HasIndex(e => e.RootId, "surface_geom_root_fkx").HasAnnotation("Npgsql:StorageParameter:fillfactor", "90");

            entity.HasIndex(e => e.Geometry, "surface_geom_spx").HasMethod("gist");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('surface_geometry_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.CityobjectId).HasColumnName("cityobject_id");
            entity.Property(e => e.Geometry)
                .HasColumnType("geometry(PolygonZ,6697)")
                .HasColumnName("geometry");
            entity.Property(e => e.Gmlid)
                .HasMaxLength(256)
                .HasColumnName("gmlid");
            entity.Property(e => e.GmlidCodespace)
                .HasMaxLength(1000)
                .HasColumnName("gmlid_codespace");
            entity.Property(e => e.ImplicitGeometry)
                .HasColumnType("geometry(PolygonZ)")
                .HasColumnName("implicit_geometry");
            entity.Property(e => e.IsComposite).HasColumnName("is_composite");
            entity.Property(e => e.IsReverse).HasColumnName("is_reverse");
            entity.Property(e => e.IsSolid).HasColumnName("is_solid");
            entity.Property(e => e.IsTriangulated).HasColumnName("is_triangulated");
            entity.Property(e => e.IsXlink).HasColumnName("is_xlink");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.RootId).HasColumnName("root_id");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("surface_geom_parent_fk");

            entity.HasOne(d => d.Root).WithMany(p => p.InverseRoot)
                .HasForeignKey(d => d.RootId)
                .HasConstraintName("surface_geom_root_fk");
        });
        modelBuilder.HasSequence("address_seq", "citydb")
            .HasMin(0L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("ade_seq", "citydb")
            .HasMin(0L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("appearance_seq", "citydb")
            .HasMin(0L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("citymodel_seq", "citydb")
            .HasMin(0L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("cityobject_genericatt_seq", "citydb")
            .HasMin(0L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("cityobject_seq", "citydb")
            .HasMin(0L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("external_ref_seq", "citydb")
            .HasMin(0L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("grid_coverage_seq", "citydb")
            .HasMin(0L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("implicit_geometry_seq", "citydb")
            .HasMin(0L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("schema_seq", "citydb")
            .HasMin(0L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("surface_data_seq", "citydb")
            .HasMin(0L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("surface_geometry_seq", "citydb")
            .HasMin(0L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence("tex_image_seq", "citydb")
            .HasMin(0L)
            .HasMax(2147483647L);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
