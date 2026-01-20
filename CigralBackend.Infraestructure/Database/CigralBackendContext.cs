using CigralBackend.Domain;
using CigralBackend.Domain.Bases;
using Microsoft.EntityFrameworkCore;

namespace CigralBackend.Infraestructure.Database
{
    public class CigralBackendContext : DbContext
    {
        public CigralBackendContext(DbContextOptions<CigralBackendContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<Deposito> Depositos { get; set; }
        public DbSet<Existencia> Existencias { get; set; }
        public DbSet<DetalleRemito> DetallesRemito { get; set; }
        public DbSet<RemitoCliente> RemitosCliente { get; set; }
        public DbSet<RemitoProveedor> RemitosProveedor { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RazonSocial).HasMaxLength(200);
                entity.Property(e => e.GLN).HasMaxLength(13).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Cuit).HasMaxLength(11);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Direccion).HasMaxLength(200);
            });

            // Configuración de Proveedor
            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RazonSocial).HasMaxLength(200);
                entity.Property(e => e.GLN).HasMaxLength(13).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Cuit).HasMaxLength(11);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.HasMany(e => e.Remitos)
                      .WithOne(r => r.Proveedor)
                      .HasForeignKey(r => r.ProveedorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.GTIN).HasMaxLength(14).IsRequired();
                entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
                entity.HasMany(e => e.Lotes)
                      .WithOne(l => l.Producto)
                      .HasForeignKey(l => l.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Lote
            modelBuilder.Entity<Lote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CodigoLote).HasMaxLength(50).IsRequired();
                entity.Property(e => e.FechaVencimiento).IsRequired();
                entity.HasOne(e => e.Producto)
                      .WithMany(p => p.Lotes)
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Deposito
            modelBuilder.Entity<Deposito>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Codigo).HasMaxLength(20).IsRequired();
            });

            // Configuración de Existencia
            modelBuilder.Entity<Existencia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumSerie).HasMaxLength(100);
                entity.HasOne(e => e.Deposito)
                      .WithMany()
                      .HasForeignKey(e => e.DepositoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Producto)
                      .WithMany()
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Lote)
                      .WithMany()
                      .HasForeignKey(e => e.LoteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de DetalleRemito
            modelBuilder.Entity<DetalleRemito>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Producto)
                      .WithMany()
                      .HasForeignKey("ProductoId")
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Lote)
                      .WithMany()
                      .HasForeignKey("LoteId")
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de RemitoBase (compartida)
            modelBuilder.Entity<RemitoBase>(entity =>
            {
                entity.Property(e => e.Observaciones).HasMaxLength(500);
                entity.Property(e => e.NumeroRemito).HasMaxLength(50);
            });

            // Configuración de RemitoCliente
            modelBuilder.Entity<RemitoCliente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Cliente)
                      .WithMany()
                      .HasForeignKey(e => e.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Detalles)
                      .WithOne()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de RemitoProveedor
            modelBuilder.Entity<RemitoProveedor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Proveedor)
                      .WithMany(p => p.Remitos)
                      .HasForeignKey(e => e.ProveedorId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Detalles)
                      .WithOne()
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
