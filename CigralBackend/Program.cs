using CigralBackend.Infraestructure.Database;
using CigralBackend.Infraestructure.Database.Interfaces;
using CigralBackend.Application.Services.Interfaces;
using CigralBackend.Application.Services;
using CigralBackend.Middleware;
using Microsoft.EntityFrameworkCore;

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

            // Registrar el repositorio
            builder.Services.AddScoped<IRepository, EfRepository>();
            builder.Services.AddScoped<IProductoService, ProductoService>();
            builder.Services.AddScoped<IMarcaService, MarcaService>();
            builder.Services.AddScoped<IExistenciaService, ExistenciaService>();
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
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("PermitirTodo");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
