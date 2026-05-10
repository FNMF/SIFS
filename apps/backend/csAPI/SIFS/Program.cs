using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SIFS.Infrastructure;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.External;
using SIFS.Infrastructure.Realtime;
using SIFS.Shared.Extensions;
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

            var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
            if (File.Exists(envPath))
            {
                DotNetEnv.Env.Load(envPath);
            }

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required. Configure ConnectionStrings__DefaultConnection in .env.");
            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Satellite Image Forensics System API",
                    Version = "v1",
                    Description = "Backend APIs for satellite image forensics, RBAC, task management, algorithm management, and dashboard data."
                });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter JWT Bearer token, for example: Bearer {token}",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                options.AddSecurityDefinition("Bearer", securityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securityScheme, Array.Empty<string>() }
                });
            });
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IEventBus, EventBus>();
            builder.Services.AddSingleton<AppEventLoggingListener>();
            builder.Services.AddSingleton<OperationLogListener>();
            builder.Services.AddSingleton<DashboardRealtimeListener>();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IAlgorithmEndpointResolver, AlgorithmEndpointResolver>();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            // JWT authentication
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
        if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
            throw new InvalidOperationException("Jwt:SecretKey is required. Configure Jwt__SecretKey in .env.");

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

        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/admin/dashboard/hub") || path.StartsWithSegments("/task-notifications/hub")))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    })
    .AddJwtBearer("ExpiredAllowed", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
        if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
            throw new InvalidOperationException("Jwt:SecretKey is required. Configure Jwt__SecretKey in .env.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
            )
        };
    });
            builder.Services.AddScoped<IJwtHelper, JwtHelper>();

            builder.Services.AddHttpContextAccessor();

            // File URL builder
            builder.Services.Configure<AppUrlOptions>(
                builder.Configuration.GetSection("AppUrlOptions"));
            builder.Services.AddSingleton<IFileUrlBuilder, FileUrlBuilder>();

            // Database context
            builder.Services.AddDbContext<SIFSContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
    
            // Conventional service registration
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
            builder.Services.AddHostedService<ModelHealthCheckWorker>();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:5173", "http://localhost:5174")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            var app = builder.Build();

            var eventBus = app.Services.GetRequiredService<IEventBus>();
            app.Services.GetRequiredService<AppEventLoggingListener>().RegisterAll(eventBus);
            app.Services.GetRequiredService<OperationLogListener>().RegisterAll(eventBus);
            app.Services.GetRequiredService<DashboardRealtimeListener>().Register(eventBus);

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "SIFS API v1");
                options.RoutePrefix = "swagger";
            });

            app.UseCors("Frontend");

            app.UseAuthentication();
            app.UseAuthorization();

            var appDataPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "data"));
            Directory.CreateDirectory(appDataPath);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(appDataPath),
                RequestPath = "/data",
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "http://localhost:5173");
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
                }
            });


            app.MapControllers();
            app.MapHub<DashboardHub>("/admin/dashboard/hub");
            app.MapHub<TaskNotificationHub>("/task-notifications/hub");

            app.Run();
        }
    }
}
