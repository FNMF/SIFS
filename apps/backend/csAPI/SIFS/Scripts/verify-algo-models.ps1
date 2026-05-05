$ErrorActionPreference = "Stop"

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$tempRoot = Join-Path $env:TEMP ("sifs-algo-model-verify-" + [Guid]::NewGuid().ToString("N"))

New-Item -ItemType Directory -Path $tempRoot | Out-Null

try {
    dotnet new console -o $tempRoot --force | Out-Null

    $projectName = Split-Path $tempRoot -Leaf
    $csprojPath = Join-Path $tempRoot "$projectName.csproj"
    $csproj = Get-Content -Path $csprojPath -Raw
    $projectReference = @"

  <ItemGroup>
    <ProjectReference Include="$projectRoot\SIFS.csproj" />
  </ItemGroup>
"@
    $csproj = $csproj.Replace("</Project>", "$projectReference`r`n</Project>")
    Set-Content -Path $csprojPath -Value $csproj

    $program = @'
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SIFS.Application.AlgoModels;
using SIFS.Application.OperationLogs;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Extensions.EventBus;

var services = new ServiceCollection();
services.AddLogging();
services.AddHttpContextAccessor();
services.AddDbContext<SIFSContext>();
services.AddScoped<IAlgoModelRepository, AlgoModelRepository>();
services.AddScoped<IAlgoModelService, AlgoModelService>();
services.AddScoped<IOperationLogRepository, OperationLogRepository>();
services.AddScoped<IOperationLogService, OperationLogService>();
services.AddSingleton<IEventBus, EventBus>();
services.AddSingleton<IAppEventRequestContextFactory, NullRequestContextFactory>();
services.AddSingleton<OperationLogListener>();

using var provider = services.BuildServiceProvider();
var eventBus = provider.GetRequiredService<IEventBus>();
provider.GetRequiredService<OperationLogListener>().RegisterAll(eventBus);

using var scope = provider.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<IAlgoModelService>();
var db = scope.ServiceProvider.GetRequiredService<SIFSContext>();

var actorId = Guid.NewGuid();
var name = "VERIFY_ALGO_" + Guid.NewGuid().ToString("N");

var create = await service.CreateAlgoAsync(new AlgoModelCreateDto
{
    Name = name,
    ApiUrl = "http://127.0.0.1:18000/detect/verify",
    Description = "verification",
    ReservedJson = new { threshold = 0.75, schema = new { image_url = "string" } }
}, actorId);
Assert(create.IsSuccess, "admin service can create algorithm with API URL");
Assert(create.Data.ReservedJson?.Contains("threshold") == true, "reserved_json stores arbitrary JSON");

var update = await service.UpdateAlgoAsync(create.Data.Id, new AlgoModelUpdateDto
{
    ApiUrl = "http://127.0.0.1:18001/detect/verify",
    Description = "updated",
    ReservedJson = new { mode = "fast" }
}, actorId);
Assert(update.IsSuccess && update.Data.ApiUrl.Contains("18001"), "admin service can update algorithm");

var disabledUsage = await service.GetEnabledAlgoByIdAsync(create.Data.Id);
Assert(!disabledUsage.IsSuccess, "disabled algorithm is rejected for business usage");

var enable = await service.EnableAlgoAsync(create.Data.Id, actorId);
Assert(enable.IsSuccess && enable.Data.Enabled, "admin service can enable algorithm");

var enabledUsage = await service.GetEnabledAlgoByIdAsync(create.Data.Id);
Assert(enabledUsage.IsSuccess, "enabled algorithm can be retrieved for business usage");

var disable = await service.DisableAlgoAsync(create.Data.Id, actorId);
Assert(disable.IsSuccess && !disable.Data.Enabled, "admin service can disable algorithm");

var list = await service.ListAlgosAsync(new AlgoModelQuery { Name = name, Page = 1, PageSize = 10 });
Assert(list.IsSuccess && list.Data.Total == 1, "list filters and pagination work");

var logTypes = await db.OperationLogs
    .Where(x => x.TargetType == "algo" && x.TargetId == create.Data.Id.ToString())
    .Select(x => x.OperationType)
    .ToListAsync();

Assert(logTypes.Contains(AppEventTypes.AlgoCreated), "create triggers operation log");
Assert(logTypes.Contains(AppEventTypes.AlgoUpdated), "update triggers operation log");
Assert(logTypes.Contains(AppEventTypes.AlgoEnabled), "enable triggers operation log");
Assert(logTypes.Contains(AppEventTypes.AlgoDisabled), "disable triggers operation log");

var defaultFldcf = await service.GetEnabledAlgoByIdAsync(0);
Assert(defaultFldcf.IsSuccess && defaultFldcf.Data.Name == "FLDCF", "default FLDCF is enabled for existing business flow");

var logs = await db.OperationLogs.Where(x => x.TargetType == "algo" && x.TargetId == create.Data.Id.ToString()).ToListAsync();
db.OperationLogs.RemoveRange(logs);

var createdModel = await db.AlgoModels.FirstAsync(x => x.Id == create.Data.Id);
db.AlgoModels.Remove(createdModel);
await db.SaveChangesAsync();
Console.WriteLine("PASS verification rows cleaned");

Console.WriteLine("PASS algo model verification");

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new Exception("FAIL " + message);

    Console.WriteLine("PASS " + message);
}

public class NullRequestContextFactory : IAppEventRequestContextFactory
{
    public Dictionary<string, object?> Create(string? requestSummary = null, string? actorUsername = null)
    {
        return new Dictionary<string, object?>
        {
            ["actor_username"] = actorUsername ?? "verify-admin",
            ["request_ip"] = "127.0.0.1",
            ["request_method"] = "VERIFY",
            ["request_path"] = "/verify/algo-models",
            ["request_summary"] = requestSummary
        };
    }
}
'@

    Set-Content -Path (Join-Path $tempRoot "Program.cs") -Value $program
    dotnet run --project $csprojPath
}
finally {
    if (Test-Path $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}
