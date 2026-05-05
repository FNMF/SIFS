$ErrorActionPreference = "Stop"

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$tempRoot = Join-Path $env:TEMP ("sifs-operation-log-verify-" + [Guid]::NewGuid().ToString("N"))

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
using SIFS.Application.OperationLogs;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Extensions.EventBus;

var services = new ServiceCollection();
services.AddLogging();
services.AddDbContext<SIFSContext>();
services.AddScoped<IOperationLogRepository, OperationLogRepository>();
services.AddScoped<IOperationLogService, OperationLogService>();
services.AddSingleton<IEventBus, EventBus>();
services.AddSingleton<OperationLogListener>();

using var provider = services.BuildServiceProvider();
var eventBus = provider.GetRequiredService<IEventBus>();
provider.GetRequiredService<OperationLogListener>().RegisterAll(eventBus);

var targetId = "verify-" + Guid.NewGuid().ToString("N");
var actorId = Guid.NewGuid();

eventBus.Publish(new AppEvent
{
    EventType = AppEventTypes.TaskCreated,
    ActorId = actorId,
    TargetType = "verification_task",
    TargetId = targetId,
    Payload = new Dictionary<string, object?> { ["source"] = "verify-operation-logs" },
    RequestContext = new Dictionary<string, object?>
    {
        ["actor_username"] = "verify-user",
        ["request_ip"] = "127.0.0.1",
        ["request_method"] = "POST",
        ["request_path"] = "/verify",
        ["request_summary"] = "operation log verification"
    },
    Success = true
});

using var scope = provider.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<IOperationLogService>();

var queryResult = await service.QueryLogsAsync(new OperationLogQuery
{
    ActorId = actorId,
    ActorUsername = "verify-user",
    OperationType = AppEventTypes.TaskCreated,
    Success = true,
    TargetType = "verification_task",
    StartTime = DateTime.UtcNow.AddMinutes(-5),
    EndTime = DateTime.UtcNow.AddMinutes(5),
    Page = 1,
    PageSize = 10
});

Assert(queryResult.IsSuccess, "query returns success");
Assert(queryResult.Data.Total >= 1, "query filters find persisted log");
Assert(queryResult.Data.Data.Any(x => x.TargetId == targetId), "persisted log has target id");
Assert(queryResult.Data.PageNumber == 1 && queryResult.Data.PageSize == 10, "pagination metadata is returned");

var brokenServices = new ServiceCollection();
brokenServices.AddLogging();
brokenServices.AddSingleton<OperationLogListener>();
using var brokenProvider = brokenServices.BuildServiceProvider();
brokenProvider.GetRequiredService<OperationLogListener>().Handle(new AppEvent
{
    EventType = AppEventTypes.TaskDeleted,
    TargetType = "verification_task",
    TargetId = targetId
});
Console.WriteLine("PASS listener failure is isolated");

var db = scope.ServiceProvider.GetRequiredService<SIFSContext>();
var createdRows = await db.OperationLogs.Where(x => x.TargetId == targetId).ToListAsync();
db.OperationLogs.RemoveRange(createdRows);
await db.SaveChangesAsync();
Console.WriteLine("PASS verification rows cleaned");

Console.WriteLine("PASS operation log verification");

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new Exception("FAIL " + message);

    Console.WriteLine("PASS " + message);
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
