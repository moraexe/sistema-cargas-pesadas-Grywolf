using Microsoft.EntityFrameworkCore;
using TransporteCargaPesada.Web.Models;
using TransporteCargaPesada.Web.Services.Reglas;

namespace TransporteCargaPesada.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Carga> Cargas => Set<Carga>();
    public DbSet<Objeto> Objetos => Set<Objeto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Parametro> Parametros => Set<Parametro>();

    /// <summary>Las fechas se guardan en UTC; SQLite no conserva el Kind, así que se restituye al leer.</summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<FechaUtcConverter>();
    }

    private sealed class FechaUtcConverter() : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var tiposCategoria = string.Join(", ", TiposCategoria.Todos.Select(t => $"'{t.Codigo}'"));
        var tiposDato = string.Join(", ", TiposDato.Todos.Select(t => $"'{t.Codigo}'"));

        modelBuilder.Entity<Categoria>(e =>
        {
            e.ToTable("Categorias", t =>
                t.HasCheckConstraint("CK_Categorias_TipoCategoria", $"TipoCategoria IN ({tiposCategoria})"));
            e.Property(c => c.TipoCategoria).HasMaxLength(30).IsRequired();
            e.Property(c => c.Codigo).HasMaxLength(50).IsRequired();
            e.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
            e.Property(c => c.Descripcion).HasMaxLength(300);
            // RN-11: el código es único dentro de su tipo de categoría.
            e.HasIndex(c => new { c.TipoCategoria, c.Codigo }).IsUnique();
        });

        modelBuilder.Entity<Carga>(e =>
        {
            e.ToTable("Cargas", t =>
            {
                t.HasCheckConstraint("CK_Cargas_PesoTotalKg", "CAST(PesoTotalKg AS REAL) > 0");
                t.HasCheckConstraint("CK_Cargas_VolumenTotalM3", "VolumenTotalM3 IS NULL OR CAST(VolumenTotalM3 AS REAL) > 0");
            });
            e.Property(c => c.Codigo).HasMaxLength(20).IsRequired();
            e.Property(c => c.Descripcion).HasMaxLength(300).IsRequired();
            e.Property(c => c.Origen).HasMaxLength(100).IsRequired();
            e.Property(c => c.Destino).HasMaxLength(100).IsRequired();
            e.Property(c => c.Observaciones).HasMaxLength(500);
            // RN-01: el código de carga es único.
            e.HasIndex(c => c.Codigo).IsUnique();

            // RN-12/RN-14: una categoría referenciada no puede borrarse (Restrict).
            e.HasOne(c => c.TipoCarga).WithMany().HasForeignKey(c => c.TipoCargaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.TipoVehiculo).WithMany().HasForeignKey(c => c.TipoVehiculoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.TipoEntrega).WithMany().HasForeignKey(c => c.TipoEntregaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Estado).WithMany().HasForeignKey(c => c.EstadoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Objeto>(e =>
        {
            e.ToTable("Objetos", t =>
            {
                t.HasCheckConstraint("CK_Objetos_Cantidad", "Cantidad > 0");
                t.HasCheckConstraint("CK_Objetos_PesoUnitarioKg", "CAST(PesoUnitarioKg AS REAL) > 0");
                t.HasCheckConstraint("CK_Objetos_VolumenUnitarioM3", "VolumenUnitarioM3 IS NULL OR CAST(VolumenUnitarioM3 AS REAL) > 0");
            });
            e.Property(o => o.Nombre).HasMaxLength(100).IsRequired();
            e.Property(o => o.Descripcion).HasMaxLength(300);
            e.Property(o => o.Observaciones).HasMaxLength(500);
            // RN-13: una carga con objetos no se elimina (Restrict, no Cascade).
            e.HasOne(o => o.Carga).WithMany(c => c.Objetos).HasForeignKey(o => o.CargaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.Categoria).WithMany().HasForeignKey(o => o.CategoriaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Parametro>(e =>
        {
            e.ToTable("Parametros", t =>
                t.HasCheckConstraint("CK_Parametros_TipoDato", $"TipoDato IN ({tiposDato})"));
            e.Property(p => p.Codigo).HasMaxLength(50).IsRequired();
            e.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
            e.Property(p => p.Descripcion).HasMaxLength(300);
            e.Property(p => p.TipoDato).HasMaxLength(20).IsRequired();
            e.Property(p => p.Valor).HasMaxLength(200).IsRequired();
            e.Property(p => p.UnidadMedida).HasMaxLength(20);
            e.HasIndex(p => p.Codigo).IsUnique();
            e.HasOne(p => p.Categoria).WithMany().HasForeignKey(p => p.CategoriaId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
