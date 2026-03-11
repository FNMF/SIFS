using SIFS.Infrastructure.External;

namespace SIFS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.Configure<AiServiceOptions>(
    builder.Configuration.GetSection("AiServices"));
            //通用名称服务注册
            builder.Services.Scan(scan => scan
                .FromApplicationDependencies(dep => dep.FullName.StartsWith("SIFS"))
                .AddClasses(classes =>
                    classes.Where(type =>
                        type.Name.EndsWith("Service") || type.Name.EndsWith("Repository") || type.Name.EndsWith("Factory")))
                            .AsImplementedInterfaces()
                            .WithScopedLifetime());

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
