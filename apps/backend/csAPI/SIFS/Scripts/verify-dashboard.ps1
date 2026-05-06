$ErrorActionPreference = "Stop"

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$tempRoot = Join-Path $env:TEMP ("sifs-dashboard-verify-" + [Guid]::NewGuid().ToString("N"))

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
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SIFS.Api.Admin;
using SIFS.Application.Dashboard;
using SIFS.Domain.Enum;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Infrastructure.Authorization;
using SIFS.Infrastructure.Realtime;
using SIFS.Shared.Extensions.EventBus;
using SIFS.Shared.Helpers;

var services = new ServiceCollection();
services.AddLogging();
services.AddHttpClient();
services.AddSignalR();
services.AddDbContext<SIFSContext>();
services.AddSingleton<IEventBus, EventBus>();
services.AddSingleton<DashboardRealtimeListener>();
services.AddScoped<IDashboardService, DashboardService>();

using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<SIFSContext>();
var dashboard = scope.ServiceProvider.GetRequiredService<IDashboardService>();

var userId = UuidV7.NewUuidV7();
var taskIds = new List<Guid>();
var algoTaskIds = new List<Guid>();
var logIds = new List<Guid>();
var algoIds = new List<int>();
var prefix = "verify-dashboard-" + Guid.NewGuid().ToString("N");

try
{
    db.Users.Add(new User { Id = userId, Account = prefix + "-user", PasswordHashed = "x", Salt = "x" });
    await db.SaveChangesAsync();

    var enabledOffline = new AlgoModel
    {
        Name = prefix + "-enabled-offline",
        Enabled = true,
        ApiUrl = "not-a-valid-url",
        Description = "dashboard verification",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    var disabled = new AlgoModel
    {
        Name = prefix + "-disabled",
        Enabled = false,
        ApiUrl = "http://127.0.0.1:9/health",
        Description = "dashboard verification",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    db.AlgoModels.AddRange(enabledOffline, disabled);
    await db.SaveChangesAsync();
    algoIds.Add(enabledOffline.Id);
    algoIds.Add(disabled.Id);

    var baseTime = DateTime.UtcNow.AddDays(1);
    var pendingTask = await CreateTaskAsync(userId, "pending", AlgoTaskStatus.pending, baseTime.AddMinutes(1), null);
    var runningTask = await CreateTaskAsync(userId, "running", AlgoTaskStatus.running, baseTime.AddMinutes(2), null);
    var failedTask = await CreateTaskAsync(userId, "failed", AlgoTaskStatus.failed, baseTime.AddMinutes(3), "dashboard failure");
    var doneTask = await CreateTaskAsync(userId, "done", AlgoTaskStatus.done, baseTime.AddMinutes(4), null);

    var log1 = UuidV7.NewUuidV7();
    var log2 = UuidV7.NewUuidV7();
    logIds.Add(log1);
    logIds.Add(log2);
    db.OperationLogs.AddRange(
        new OperationLog
        {
            Id = log1,
            ActorId = userId,
            ActorUsername = prefix + "-user",
            OperationType = "VERIFY_OLD",
            TargetType = "dashboard",
            TargetId = pendingTask.ToString(),
            RequestPath = "/verify/old",
            Success = true,
            CreatedAt = baseTime.AddMinutes(5)
        },
        new OperationLog
        {
            Id = log2,
            ActorId = userId,
            ActorUsername = prefix + "-user",
            OperationType = "VERIFY_NEW",
            TargetType = "dashboard",
            TargetId = failedTask.ToString(),
            RequestPath = "/verify/new",
            Success = false,
            FailureReason = "verify failure",
            CreatedAt = baseTime.AddMinutes(6)
        });
    await db.SaveChangesAsync();

    var summary = await dashboard.GetSummaryAsync();
    Assert(summary.TodayTaskCount >= 4, "admin can query dashboard summary with today task count");
    Assert(summary.TotalTaskCount >= 4 && summary.RunningTaskCount >= 1 && summary.WaitingTaskCount >= 1 && summary.FailedTaskCount >= 1 && summary.SuccessTaskCount >= 1,
        "summary includes task status counters");
    Assert(summary.AlgoTotalCount >= 2 && summary.AlgoEnabledCount >= 1 && summary.AlgoOfflineCount >= 1,
        "summary includes algorithm counters and offline count");

    var recent = await dashboard.GetRecentTasksAsync(4);
    Assert(recent.Count >= 4 && recent.First().TaskId == doneTask,
        "recent tasks returns newest tasks first");

    var recentFailed = await dashboard.GetRecentFailedTasksAsync(10);
    Assert(recentFailed.Any(x => x.TaskId == failedTask) && recentFailed.All(x => x.Status == "failed"),
        "recent failed tasks only returns failed tasks");

    var recentLogs = await dashboard.GetRecentLogsAsync(2);
    Assert(recentLogs.Count == 2 && recentLogs.First().Id == log2,
        "recent logs returns newest logs first");

    var taskStatusCount = await dashboard.GetTaskStatusCountAsync();
    Assert(taskStatusCount.Items.Any(x => x.Status == "queued" && x.Count >= 1) &&
           taskStatusCount.Items.Any(x => x.Status == "running" && x.Count >= 1) &&
           taskStatusCount.Items.Any(x => x.Status == "failed" && x.Count >= 1) &&
           taskStatusCount.Items.Any(x => x.Status == "done" && x.Count >= 1),
        "task status count groups by status");

    var algoStatus = await dashboard.GetAlgoStatusCountAsync();
    Assert(algoStatus.Enabled >= 1 && algoStatus.Disabled >= 1 && algoStatus.Offline >= 1,
        "algorithm status count includes enabled disabled and offline");

    var authorizeAttribute = typeof(DashboardHub).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Any();
    Assert(authorizeAttribute, "SignalR hub requires authenticated clients");
    Assert(typeof(DashboardController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Any(),
        "dashboard APIs require authenticated admin request");
    Assert(typeof(DashboardController).GetCustomAttributes(typeof(RequirePermissionAttribute), inherit: true).Any(),
        "dashboard APIs require admin permission");

    var eventBus = provider.GetRequiredService<IEventBus>();
    provider.GetRequiredService<DashboardRealtimeListener>().Register(eventBus);
    eventBus.Publish(new AppEvent
    {
        EventType = AppEventTypes.TaskCreated,
        TargetType = "detection_task",
        TargetId = pendingTask.ToString()
    });
    Assert(true, "EventBus-triggered dashboard realtime push does not break caller");
}
finally
{
    db.OperationLogs.RemoveRange(await db.OperationLogs.Where(x => logIds.Contains(x.Id)).ToListAsync());
    db.TaskAudits.RemoveRange(await db.TaskAudits.Where(x => taskIds.Contains(x.TaskId)).ToListAsync());
    db.ResultFiles.RemoveRange(await db.ResultFiles.Where(x => algoTaskIds.Contains(x.AlgoTaskId)).ToListAsync());
    db.TaskTypeMaps.RemoveRange(await db.TaskTypeMaps.Where(x => algoTaskIds.Contains(x.TaskId)).ToListAsync());
    db.Localfiles.RemoveRange(await db.Localfiles.Where(x => algoTaskIds.Contains(x.AlgoTaskId)).ToListAsync());
    db.AlgoTasks.RemoveRange(await db.AlgoTasks.Where(x => algoTaskIds.Contains(x.Id)).ToListAsync());
    db.TaskLists.RemoveRange(await db.TaskLists.Where(x => taskIds.Contains(x.Id)).ToListAsync());
    db.AlgoModels.RemoveRange(await db.AlgoModels.Where(x => algoIds.Contains(x.Id)).ToListAsync());
    db.Users.RemoveRange(await db.Users.Where(x => x.Id == userId).ToListAsync());
    await db.SaveChangesAsync();
}

Console.WriteLine("PASS dashboard verification");

async Task<Guid> CreateTaskAsync(Guid ownerId, string label, AlgoTaskStatus status, DateTime createdAt, string? failureReason)
{
    var taskId = UuidV7.NewUuidV7();
    var algoTaskId = UuidV7.NewUuidV7();
    taskIds.Add(taskId);
    algoTaskIds.Add(algoTaskId);

    db.TaskLists.Add(new TaskList
    {
        Id = taskId,
        UserId = ownerId,
        Status = 0,
        Level = 1,
        CreatedAt = createdAt,
        UpdatedAt = createdAt
    });
    db.AlgoTasks.Add(new AlgoTask
    {
        Id = algoTaskId,
        TaskId = taskId,
        Status = (int)status,
        AlgoModelId = 0,
        AlgoName = "FLDCF",
        AlgoApiUrl = "http://127.0.0.1:9/detect/fldcf",
        FailureReason = failureReason,
        StartedAt = status == AlgoTaskStatus.pending ? null : createdAt.AddSeconds(10),
        FinishedAt = status is AlgoTaskStatus.done or AlgoTaskStatus.failed ? createdAt.AddSeconds(20) : null,
        CreatedAt = createdAt,
        UpdatedAt = createdAt.AddSeconds(20)
    });
    db.Localfiles.Add(new Localfile
    {
        Id = UuidV7.NewUuidV7(),
        AlgoTaskId = algoTaskId,
        UrlLocal = "/Files/" + label + ".png",
        Sid = 0,
        CreatedAt = createdAt,
        UpdatedAt = createdAt
    });
    db.TaskTypeMaps.Add(new TaskTypeMap
    {
        Id = UuidV7.NewUuidV7(),
        TaskId = algoTaskId,
        TypeId = 0
    });
    if (status == AlgoTaskStatus.done)
    {
        db.ResultFiles.Add(new ResultFile
        {
            Id = UuidV7.NewUuidV7(),
            AlgoTaskId = algoTaskId,
            AlgoType = 0,
            IsFake = false,
            Confidence = 0.1,
            MaskLocalUrl = "/Files/" + label + "-result.png"
        });
    }
    await db.SaveChangesAsync();
    return taskId;
}

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
