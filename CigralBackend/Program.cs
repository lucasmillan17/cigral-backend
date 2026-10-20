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
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

namespace CigralBackend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // PostgreSQL: DateTime se guarda como "timestamp" sin zona horaria (igual que datetime2 en SQL Server).
            // Todas las fechas se guardan en hora local de Argentina (DateTime.Now; el contenedor corre con
            // TZ=America/Argentina/Buenos_Aires), y los vencimientos son fechas sin hora.
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            
            // Configurar DbContext
            builder.Services.AddDbContext<CigralBackendContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

            // IP real del cliente detrás del proxy de Easypanel (Traefik), necesaria para el límite por IP.
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                // El contenedor solo es accesible a través del proxy, así que se confía en él.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // Límite de intentos de login por IP (protección contra fuerza bruta).
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            });

            // Registrar el repositorio
            builder.Services.AddScoped<IRepository, EfRepository>();
            builder.Services.AddScoped<IProductoService, ProductoService>();
            builder.Services.AddScoped<IMarcaService, MarcaService>();
            builder.Services.AddScoped<IExistenciaService, ExistenciaService>();
            builder.Services.AddScoped<IRemitoService, RemitoService>();
            builder.Services.AddScoped<ICorreccionRemitoService, CorreccionRemitoService>();
            builder.Services.AddScoped<IMovimientoStockService, MovimientoStockService>();
            builder.Services.AddScoped<IClienteService, ClienteService>();
            builder.Services.AddScoped<IProveedorService, ProveedorService>();
            builder.Services.AddScoped<IDepositoService, DepositoService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IPdfService, PdfService>();
            builder.Services.AddSingleton<IBarCodeParser, BarCodeParser>();
            builder.Services.AddScoped<ICatalogParserService, CsvCatalogParserService>();
            builder.Services.AddScoped<IConsignacionService, ConsignacionService>();
            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddSingleton<IEmailService, EmailService>();
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

                // Header x-cigral-auth, para poder probar desde Swagger cuando la API lo exige
                c.AddSecurityDefinition("AuthHeader", new OpenApiSecurityScheme
                {
                    Description = "Clave del header x-cigral-auth",
                    Name = AuthHeaderMiddleware.HeaderName,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "AuthHeader" } }, new List<string>() }
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

            app.UseForwardedHeaders();

            // Registrar el middleware de manejo de excepciones ANTES de otros middlewares
            app.UseGlobalExceptionHandler();

            // Swagger: habilitado en desarrollo; en otros entornos solo con Swagger:Habilitado = true
            if (app.Configuration.GetValue("Swagger:Habilitado", app.Environment.IsDevelopment()))
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();

            app.UseCors("PermitirTodo");

            // Exige x-cigral-auth (antes lo validaba el proxy de Cloudflare)
            app.UseAuthHeader();

            app.UseRateLimiter();

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

                    // 2. Roles (Administrador, Soporte) y admin inicial
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
                    foreach (var rol in Roles.Todos)
                    {
                        if (!await roleManager.RoleExistsAsync(rol))
                            await roleManager.CreateAsync(new IdentityRole<int>(rol));
                    }

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

                    // Todo usuario sin rol queda como Administrador (es el acceso que tenían antes de los roles).
                    foreach (var usuario in userManager.Users.ToList())
                    {
                        if ((await userManager.GetRolesAsync(usuario)).Count == 0)
                            await userManager.AddToRoleAsync(usuario, Roles.Administrador);
                    }
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
