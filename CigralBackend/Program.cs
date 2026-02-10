using CigralBackend.Infraestructure.Database;
using CigralBackend.Infraestructure.Database.Interfaces;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Application.Services;
using CigralBackend.Middleware;
using CigralBackend.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CigralBackend
{
    public class Program
    {
        public static void Main(string[] args)
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

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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
           // }

            app.UseHttpsRedirection();

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
                    // Esto aplica todas las migraciones pendientes.
                    // Si la BD no existe, la crea. Si existe pero está vieja, la actualiza.
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Ocurrió un error al migrar la base de datos.");
                }
            }

            app.Run();
        }
    }
}
