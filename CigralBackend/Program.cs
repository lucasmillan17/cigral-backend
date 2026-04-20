using Azure.Core;
using CigralBackend.Application.Services;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Domain;
using CigralBackend.Infraestructure.Database;
using CigralBackend.Infraestructure.Database.Interfaces;
using CigralBackend.Infraestructure.Services;
using CigralBackend.Infraestructure.Services.Interfaces;
using CigralBackend.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace CigralBackend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            
            // Configurar DbContext
            builder.Services.AddDbContext<CigralBackendContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configurar Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
            {
                // Configuración de contraseñas
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;

                // Configuración de usuarios
                options.User.RequireUniqueEmail = false;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // Configuración de lockout
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<CigralBackendContext>()
            .AddDefaultTokenProviders();

            // Configurar JWT Authentication
            var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada");
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CigralBackend";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CigralBackend";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

            builder.Services.AddAuthorization();

            // Registrar el repositorio
            builder.Services.AddScoped<IRepository, EfRepository>();
            builder.Services.AddScoped<IProductoService, ProductoService>();
            builder.Services.AddScoped<IMarcaService, MarcaService>();
            builder.Services.AddScoped<IExistenciaService, ExistenciaService>();
            builder.Services.AddScoped<IRemitoService, RemitoService>();
            builder.Services.AddScoped<IMovimientoStockService, MovimientoStockService>();
            builder.Services.AddScoped<IClienteService, ClienteService>();
            builder.Services.AddScoped<IProveedorService, ProveedorService>();
            builder.Services.AddScoped<IDepositoService, DepositoService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<CigralBackend.Domain.Services.IPdfService, CigralBackend.Infraestructure.Services.PdfService>();
            builder.Services.AddSingleton<IBarCodeParser, BarCodeParser>();
            builder.Services.AddScoped<ICatalogParserService, CsvCatalogParserService>();
            builder.Services.AddScoped<IConsignacionService, ConsignacionService>();
            builder.Services.AddHttpContextAccessor();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "CigralBackend", Version = "v1" });

                // Definimos el esquema de seguridad (Bearer)
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // Le decimos a Swagger que use ese esquema
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
            });

            //Autorizamos 3ros en desarrollo
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("PermitirTodo",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            var app = builder.Build();

            // Registrar el middleware de manejo de excepciones ANTES de otros middlewares
            app.UseGlobalExceptionHandler();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //}

            //app.UseHttpsRedirection();

            app.UseCors("PermitirTodo");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<CigralBackendContext>();

                    // 1. Migrar la base de datos (ya lo tenías)
                    context.Database.Migrate();

                    // 2. INICIO NUEVO CÓDIGO: Crear Admin si no existe
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                    // Verificamos si la tabla de usuarios está vacía
                    if (!context.Users.Any())
                    {
                        var adminUser = new ApplicationUser
                        {
                            UserName = "adminCigral",
                            NombreCompleto = "Admin Cigral",
                            Email = "admin@cigral.com",
                            EmailConfirmed = true, // Usar como flag "Activo"
                            EsAdmin = true,
                            FechaCreacion = DateTime.Now
                            // Agrega aquí otros campos obligatorios de tu entidad ApplicationUser si los tienes
                        };

                        // Creamos el usuario con la contraseña que tú elijas
                        // ¡IMPORTANTE! La contraseña debe cumplir tus reglas (mayúscula, minúscula, número, no alfanumérico)
                        var result = await userManager.CreateAsync(adminUser, "b6@$1[E3>8£{");

                        if (result.Succeeded)
                        {
                            // Opcional: Si usas roles, aquí podrías asignarle el rol de Admin
                            // await userManager.AddToRoleAsync(adminUser, "Admin");
                            var logger = services.GetRequiredService<ILogger<Program>>();
                            logger.LogInformation("Usuario Admin creado exitosamente.");
                        }
                        else
                        {
                            var logger = services.GetRequiredService<ILogger<Program>>();
                            logger.LogError("Error al crear el usuario Admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                    // FIN NUEVO CÓDIGO
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Ocurrió un error durante la migración o el seeding.");
                }
            }

            app.Run();
        }
    }
}
