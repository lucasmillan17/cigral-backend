using CigralBackend.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CigralBackend.Infraestructure.Database
{
    public class CigralBackendContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public CigralBackendContext(DbContextOptions<CigralBackendContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<Deposito> Depositos { get; set; }
        public DbSet<Existencia> Existencias { get; set; }
        public DbSet<DetalleRemito> DetallesRemito { get; set; }
        public DbSet<RemitoEgreso> RemitosEgreso { get; set; }
        public DbSet<RemitoIngreso> RemitosIngreso { get; set; }
        public DbSet<MovimientoStock> MovimientosStock { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RazonSocial).HasMaxLength(200);
                entity.Property(e => e.GLN).HasMaxLength(13);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Cuit).HasMaxLength(11);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasMany(e => e.Remitos)
                      .WithOne(r => r.Cliente)
                      .HasForeignKey(r => r.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasQueryFilter(c => c.Activo);
            });

            // Configuración de Proveedor
            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RazonSocial).HasMaxLength(200);
                entity.Property(e => e.GLN).HasMaxLength(13);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Cuit).HasMaxLength(11);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasMany(e => e.Remitos)
                      .WithOne(r => r.Proveedor)
                      .HasForeignKey(r => r.ProveedorId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasQueryFilter(c => c.Activo);
            });

            // Configuración de Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.GTIN).HasMaxLength(14).IsRequired();
                entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Marca)
                      .WithMany()
                      .HasForeignKey(e => e.MarcaId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Lotes)
                      .WithOne(l => l.Producto)
                      .HasForeignKey(l => l.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.GTIN).IsUnique();
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasQueryFilter(c => c.Activo);
            });

            //Configuración de Marca
            modelBuilder.Entity<Marca>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasQueryFilter(c => c.Activo);
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
                entity.HasIndex(e => e.CodigoLote).IsUnique();
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasQueryFilter(c => c.Activo);
            });

            // Configuración de Deposito
            modelBuilder.Entity<Deposito>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Codigo).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasQueryFilter(c => c.Activo);
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
                entity.Property(e => e.NumeroSerie).HasMaxLength(100);
                entity.HasOne(e => e.Producto)
                      .WithMany()
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Lote)
                      .WithMany()
                      .HasForeignKey(e => e.LoteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de RemitoEgreso
            modelBuilder.Entity<RemitoEgreso>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Observaciones).HasMaxLength(500);
                entity.Property(e => e.NumeroRemito).HasMaxLength(50);
                entity.Property(e => e.Fecha).IsRequired();
                
                entity.HasOne(e => e.Cliente)
                      .WithMany(c => c.Remitos)
                      .HasForeignKey(e => e.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Detalles)
                      .WithOne(d => d.RemitoEgreso)
                      .HasForeignKey(d => d.RemitoEgresoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Deposito)
                      .WithMany()
                      .HasForeignKey(e => e.DepositoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.NumeroRemito).IsUnique();
            });

            // Configuración de RemitoIngreso
            modelBuilder.Entity<RemitoIngreso>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Observaciones).HasMaxLength(500);
                entity.Property(e => e.NumeroRemito).HasMaxLength(50);
                entity.Property(e => e.Fecha).IsRequired();
                
                entity.HasOne(e => e.Proveedor)
                      .WithMany(p => p.Remitos)
                      .HasForeignKey(e => e.ProveedorId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Detalles)
                      .WithOne(d => d.RemitoIngreso)
                      .HasForeignKey(d => d.RemitoIngresoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Deposito)
                      .WithMany()   
                      .HasForeignKey(e => e.DepositoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.NumeroRemito).IsUnique();
            });

            // Configuración de MovimientoStock
            modelBuilder.Entity<MovimientoStock>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FechaMovimiento).IsRequired();
                entity.Property(e => e.Tipo).IsRequired();
                entity.Property(e => e.NumeroSerie).HasMaxLength(100);
                entity.Property(e => e.Usuario).HasMaxLength(100);
                entity.Property(e => e.Observaciones).HasMaxLength(500);

                entity.HasOne(e => e.Producto)
                      .WithMany()
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Deposito)
                      .WithMany()
                      .HasForeignKey(e => e.DepositoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Lote)
                      .WithMany()
                      .HasForeignKey(e => e.LoteId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RemitoIngreso)
                      .WithMany()
                      .HasForeignKey(e => e.RemitoIngresoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RemitoEgreso)
                      .WithMany()
                      .HasForeignKey(e => e.RemitoEgresoId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Índices para consultas rápidas
                entity.HasIndex(e => e.FechaMovimiento);
                entity.HasIndex(e => new { e.ProductoId, e.DepositoId });
                entity.HasIndex(e => e.Tipo);
            });
        }
    }
}
