using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.External;
using SIFS.Shared.Extensions.EventBus;
using SIFS.Shared.Helpers.JWT;

namespace SIFS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            DotNetEnv.Env.Load();

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
            var jwtKey = builder.Configuration["Jwt:SecretKey"];
            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.Configure<AiServiceOptions>(
    builder.Configuration.GetSection("AiServices"));

            builder.Services.AddDbContext<SIFSContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
    
            //通用名称服务注册
            builder.Services.Scan(scan => scan
                .FromApplicationDependencies(dep => dep.FullName.StartsWith("SIFS"))
                .AddClasses(classes =>
                    classes.Where(type =>
                        type.Name.EndsWith("Service") || type.Name.EndsWith("Repository") || type.Name.EndsWith("Factory")))
                            .AsImplementedInterfaces()
                            .WithScopedLifetime());

            builder.Services.AddScoped<EventBus>();
            builder.Services.AddHttpClient();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Files")),
                RequestPath = "/Files"
            });

            app.MapControllers();

            app.Run();
        }
    }
}
