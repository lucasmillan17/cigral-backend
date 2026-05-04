using CigralBackend.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace CigralBackend.Infraestructure.Database
{
    public class CigralBackendContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        IHttpContextAccessor _userContext;
        public CigralBackendContext(DbContextOptions<CigralBackendContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _userContext = httpContextAccessor;
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
        public DbSet<EntidadResumen> EntidadesResumen { get; set; }
        public DbSet<Consignacion> Consignaciones { get; set; }
        public DbSet<RegistroAuditoria> Auditorias { get; set; }

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
                entity.Property(e => e.GTIN).HasMaxLength(14).IsRequired(false);
                entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Marca)
                      .WithMany()
                      .HasForeignKey(e => e.MarcaId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Lotes)
                      .WithOne(l => l.Producto)
                      .HasForeignKey(l => l.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.GTIN).IsUnique().HasFilter("[GTIN] IS NOT NULL");
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.HasIndex(e => e.CodigoGenerico).IsUnique();
                entity.HasIndex(e => e.CodigoInterno).IsUnique();
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

            modelBuilder.Entity<Consignacion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ClienteId, e.ExistenciaId }).IsUnique();
                entity.Property(e => e.FechaModificacion).IsRequired();
                entity.HasOne(e => e.Existencia)
                      .WithMany()
                      .HasForeignKey(e => e.ExistenciaId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Cliente)
                      .WithMany()
                      .HasForeignKey(e => e.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RegistroAuditoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Entidad).HasMaxLength(100).IsRequired();
                entity.Property(e => e.EntidadId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Accion).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Campo).HasMaxLength(100);
                entity.Property(e => e.Usuario).HasMaxLength(100);
                entity.HasIndex(e => e.Entidad);
                entity.HasIndex(e => e.Fecha);
            });

            modelBuilder.Entity<EntidadResumen>()
                .ToView("vw_Entidades_Resumen")
                .HasNoKey();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entradasAuditoria = new List<RegistroAuditoria>();

            // Obtenemos todas las entidades que han sido modificadas
            var entradasModificadas = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added || e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in entradasModificadas)
            {
                // Evitamos auditar la tabla de auditoría para no crear un bucle infinito
                if (entry.Entity is RegistroAuditoria)
                    continue;

                var nombreEntidad = entry.Entity.GetType().Name;
                var pk = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString();

                // Si es una actualización, buscamos campo por campo cuáles cambiaron
                if (entry.State == EntityState.Modified)
                {
                    foreach (var property in entry.OriginalValues.Properties)
                    {
                        var valorAnterior = entry.OriginalValues[property]?.ToString();
                        var valorActual = entry.CurrentValues[property]?.ToString();

                        // Solo guardamos si el valor realmente cambió
                        if (valorAnterior != valorActual)
                        {
                            entradasAuditoria.Add(new RegistroAuditoria
                            {
                                Entidad = nombreEntidad,
                                EntidadId = pk,
                                Accion = "Update",
                                Campo = property.Name, // Aquí usamos el campo genérico
                                ValorAnterior = valorAnterior, // Lo que antes era stockAnterior
                                ValorActual = valorActual,     // Lo que antes era stockPosterior
                                Fecha = DateTime.UtcNow,
                                Usuario = _userContext.HttpContext?.User?.Identity?.Name ?? "Sistema" // TODO: Puedes inyectar IHttpContextAccessor para obtener el email/ID del usuario logueado
                            });
                        }
                    }
                }
                else if (entry.State == EntityState.Added)
                {
                    // Lógica opcional para guardar inserciones
                    entradasAuditoria.Add(new RegistroAuditoria
                    {
                        Entidad = nombreEntidad,
                        EntidadId = "Nuevo", // El ID real se genera después del SaveChanges
                        Accion = "Insert",
                        Campo = "Todos",
                        ValorAnterior = null,
                        ValorActual = "Nuevo Registro Creado",
                        Fecha = DateTime.UtcNow,
                        Usuario = _userContext.HttpContext?.User?.Identity?.Name ?? "Sistema"
                    });
                }
            }

            // Guardamos las auditorías en el contexto antes de hacer el commit final
            if (entradasAuditoria.Any())
            {
                Auditorias.AddRange(entradasAuditoria);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
