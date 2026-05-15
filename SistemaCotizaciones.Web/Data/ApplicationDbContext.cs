using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaCotizaciones.Web.Data.Entities;

namespace SistemaCotizaciones.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Cotizacion> Cotizaciones => Set<Cotizacion>();
    public DbSet<CotizacionVersion> CotizacionVersiones => Set<CotizacionVersion>();
    public DbSet<CotizacionLinea> CotizacionLineas => Set<CotizacionLinea>();
    public DbSet<CategoriaCotizacion> CategoriasCotizacion => Set<CategoriaCotizacion>();
    public DbSet<FormaPagoCotizacion> FormasPagoCotizacion => Set<FormaPagoCotizacion>();
    public DbSet<CuentaCorrienteCotizacion> CuentasCorrientesCotizacion => Set<CuentaCorrienteCotizacion>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Cliente>(entity =>
        {
            entity.Property(x => x.Rut).HasMaxLength(20);
            entity.Property(x => x.RazonSocial).HasMaxLength(200);
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.Telefono).HasMaxLength(50);
            entity.Property(x => x.Region).HasMaxLength(100);
            entity.Property(x => x.TipoCliente).HasMaxLength(50);
            entity.Property(x => x.Sucursal).HasMaxLength(100);
            entity.Property(x => x.Vendedor).HasMaxLength(150);
            entity.Property(x => x.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.HasIndex(x => x.Rut);
            entity.HasIndex(x => x.RazonSocial);
            entity.HasMany(x => x.Cotizaciones)
                .WithOne(x => x.Cliente)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Cotizacion>(entity =>
        {
            entity.Property(x => x.NumeroBase).HasMaxLength(50);
            entity.Property(x => x.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.HasIndex(x => x.Correlativo).IsUnique();
            entity.HasMany(x => x.Versiones)
                .WithOne(x => x.Cotizacion)
                .HasForeignKey(x => x.CotizacionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CotizacionVersion>(entity =>
        {
            entity.Property(x => x.NumeroCotizacion).HasMaxLength(60);
            entity.Property(x => x.FormaPago).HasMaxLength(150);
            entity.Property(x => x.CuentaCorriente).HasMaxLength(150);
            entity.Property(x => x.PlazoEntrega).HasMaxLength(150);
            entity.Property(x => x.Moneda).HasMaxLength(10);
            entity.Property(x => x.ValidezOferta).HasMaxLength(100);
            entity.Property(x => x.PdfPath).HasMaxLength(300);
            entity.Property(x => x.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(x => x.FechaEmision).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.HasIndex(x => x.NumeroCotizacion).IsUnique();
            entity.HasIndex(x => new { x.CotizacionId, x.Version }).IsUnique();
        });

        builder.Entity<CotizacionLinea>(entity =>
        {
            entity.Property(x => x.NombreArticulo).HasMaxLength(200);
            entity.Property(x => x.Categoria).HasMaxLength(100);
        });

        builder.Entity<CategoriaCotizacion>(entity =>
        {
            entity.Property(x => x.Nombre).HasMaxLength(100);
            entity.HasIndex(x => x.Nombre).IsUnique();
            entity.HasData(
                new CategoriaCotizacion { Id = 1, Nombre = "Mantencion" },
                new CategoriaCotizacion { Id = 2, Nombre = "Reparacion" },
                new CategoriaCotizacion { Id = 3, Nombre = "Instalacion" },
                new CategoriaCotizacion { Id = 4, Nombre = "Servicio tecnico" },
                new CategoriaCotizacion { Id = 5, Nombre = "Repuestos" });
        });

        builder.Entity<FormaPagoCotizacion>(entity =>
        {
            entity.Property(x => x.Nombre).HasMaxLength(150);
            entity.HasIndex(x => x.Nombre).IsUnique();
            entity.HasData(
                new FormaPagoCotizacion { Id = 1, Nombre = "Efectivo" },
                new FormaPagoCotizacion { Id = 2, Nombre = "Transferencia" },
                new FormaPagoCotizacion { Id = 3, Nombre = "Cheque" });
        });

        builder.Entity<CuentaCorrienteCotizacion>(entity =>
        {
            entity.Property(x => x.Nombre).HasMaxLength(150);
            entity.HasIndex(x => x.Nombre).IsUnique();
        });
    }
}
