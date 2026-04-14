using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using SIFS.Infrastructure;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.External;
using SIFS.Shared.Extensions.EventBus;
using SIFS.Shared.Helpers.JWT;
using System.Text;

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
            builder.Services.AddScoped<EventBus>();
            builder.Services.AddHttpClient();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            //jwt认证配置
            builder.Services.AddAuthorization();
            builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Bearer";
        options.DefaultChallengeScheme = "Bearer";
    })
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
            )
        };
    });
            builder.Services.AddHttpContextAccessor();

            builder.Services.Configure<AiServiceOptions>(
    builder.Configuration.GetSection("AiServiceOptions"));

            //数据库上下文注册
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

            builder.Services.AddSingleton<IAlgoTaskQueue, AlgoTaskQueue>();

            builder.Services.AddHostedService<AlgoTaskWorker>();
            builder.Services.AddHostedService<AlgoTaskRecovery>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Files")),
                RequestPath = "/Files"
            });

            app.MapControllers();

            app.Run();
        }
    }
}
