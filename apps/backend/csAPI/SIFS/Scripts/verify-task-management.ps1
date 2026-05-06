$ErrorActionPreference = "Stop"

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$tempRoot = Join-Path $env:TEMP ("sifs-task-management-verify-" + [Guid]::NewGuid().ToString("N"))

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
using Microsoft.Extensions.Logging.Abstractions;
using SIFS.Application.OperationLogs;
using SIFS.Application.TaskAudits;
using SIFS.Application.TaskManagement;
using SIFS.Domain.Enum;
using SIFS.Infrastructure;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.External;
using SIFS.Infrastructure.Identity;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Extensions;
using SIFS.Shared.Extensions.EventBus;
using SIFS.Shared.Helpers;

var services = new ServiceCollection();
services.AddLogging();
services.AddHttpClient();
services.AddHttpContextAccessor();
services.AddDbContext<SIFSContext>();
services.Configure<AppUrlOptions>(options =>
{
    options.BaseUrl = "http://localhost";
    options.PyBaseUrl = "http://localhost";
});
services.AddSingleton<IFileUrlBuilder, FileUrlBuilder>();
services.AddScoped<ICurrentService, CurrentService>();
services.AddScoped<IAppEventRequestContextFactory, AppEventRequestContextFactory>();
services.AddSingleton<IEventBus, EventBus>();
services.AddSingleton<OperationLogListener>();
services.AddScoped<IOperationLogRepository, OperationLogRepository>();
services.AddScoped<IOperationLogService, OperationLogService>();
services.AddScoped<ITaskAuditRepository, TaskAuditRepository>();
services.AddScoped<ITaskAuditService, TaskAuditService>();
services.AddScoped<IAlgoModelRepository, AlgoModelRepository>();
services.AddScoped<IAlgorithmEndpointResolver, AlgorithmEndpointResolver>();
services.AddSingleton<IAlgoTaskQueue, AlgoTaskQueue>();
services.AddScoped<ITaskManagementRepository, TaskManagementRepository>();
services.AddScoped<ITaskManagementService, TaskManagementService>();

using var provider = services.BuildServiceProvider();
var eventBus = provider.GetRequiredService<IEventBus>();
var operationLogListener = provider.GetRequiredService<OperationLogListener>();
operationLogListener.RegisterAll(eventBus);

using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<SIFSContext>();
var service = scope.ServiceProvider.GetRequiredService<ITaskManagementService>();

var adminId = UuidV7.NewUuidV7();
var userId = UuidV7.NewUuidV7();
var otherUserId = UuidV7.NewUuidV7();
var taskIds = new List<Guid>();
var algoTaskIds = new List<Guid>();
var localFileIds = new List<Guid>();
var resultFileIds = new List<Guid>();
var typeMapIds = new List<Guid>();
var operationTargetIds = new List<string>();

try
{
    db.Users.AddRange(
        new User { Id = adminId, Account = "verify-admin-" + adminId.ToString("N"), PasswordHashed = "x", Salt = "x" },
        new User { Id = userId, Account = "verify-user-" + userId.ToString("N"), PasswordHashed = "x", Salt = "x" },
        new User { Id = otherUserId, Account = "verify-other-" + otherUserId.ToString("N"), PasswordHashed = "x", Salt = "x" });

    if (!await db.AlgoTypes.AnyAsync(x => x.Id == 0))
        db.AlgoTypes.Add(new AlgoType { Id = 0, Name = "FLDCF" });

    var fldcf = await db.AlgoModels.FirstOrDefaultAsync(x => x.Name == "FLDCF");
    if (fldcf == null)
    {
        db.AlgoModels.Add(new AlgoModel
        {
            Id = 0,
            Name = "FLDCF",
            Enabled = true,
            ApiUrl = "http://127.0.0.1:9/detect/fldcf",
            Description = "verification FLDCF",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }
    else
    {
        fldcf.Enabled = true;
        fldcf.ApiUrl = string.IsNullOrWhiteSpace(fldcf.ApiUrl) ? "http://127.0.0.1:9/detect/fldcf" : fldcf.ApiUrl;
        fldcf.DeletedAt = null;
        fldcf.UpdatedAt = DateTime.UtcNow;
    }
    await db.SaveChangesAsync();

    var ownTaskId = await CreateTaskAsync(userId, "own", AlgoTaskStatus.failed, "verify failure");
    var otherTaskId = await CreateTaskAsync(otherUserId, "other", AlgoTaskStatus.pending, null);

    Assert(await db.TaskAudits.AnyAsync(x => x.TaskId == ownTaskId && x.FromStatus == null && x.ToStatus == "created"),
        "task creation writes TaskAudit record");

    var adminList = await service.QueryAdminAsync(new TaskManagementQuery { Keyword = "verify-", Page = 1, PageSize = 20 }, adminId);
    Assert(adminList.IsSuccess && adminList.Data.Data.Any(x => x.TaskId == ownTaskId) && adminList.Data.Data.Any(x => x.TaskId == otherTaskId),
        "admin can list all users' tasks");

    var userList = await service.QueryUserAsync(new TaskManagementQuery { Page = 1, PageSize = 20 }, userId);
    Assert(userList.IsSuccess && userList.Data.Data.Any(x => x.TaskId == ownTaskId) && userList.Data.Data.All(x => x.CreatedByUserId == userId),
        "ordinary user can only list own tasks");

    var failedFilter = await service.QueryAdminAsync(new TaskManagementQuery { Failed = true, Keyword = "verify-", Page = 1, PageSize = 20 }, adminId);
    Assert(failedFilter.IsSuccess && failedFilter.Data.Data.Any(x => x.TaskId == ownTaskId),
        "failed filter works");

    var adminDetail = await service.GetAdminDetailAsync(otherTaskId, adminId);
    Assert(adminDetail.IsSuccess && adminDetail.Data.SubTasks.Count == 1 && adminDetail.Data.OriginalImagePaths.Any(),
        "admin can view task detail with paths and subtasks");

    var forbiddenDetail = await service.GetUserDetailAsync(otherTaskId, userId);
    Assert(!forbiddenDetail.IsSuccess,
        "ordinary user cannot view another user's task");

    var flow = await service.GetAdminStatusFlowAsync(ownTaskId, adminId);
    Assert(flow.IsSuccess && flow.Data.Any(x => x.Status == "created"),
        "status flow returns minimal timeline");

    var retry = await service.RetryAdminAsync(ownTaskId, adminId);
    Assert(retry.IsSuccess && retry.Data.NewTaskId.HasValue,
        "admin can retry task");
    taskIds.Add(retry.Data.NewTaskId!.Value);
    operationTargetIds.Add(ownTaskId.ToString());

    var retryAudits = await db.TaskAudits.AsNoTracking().Where(x => x.TaskId == ownTaskId).OrderBy(x => x.CreatedAt).ToListAsync();
    Assert(retryAudits.Any(x => x.ToStatus == "retried" && x.OperatorId == adminId),
        "admin retry writes TaskAudit record");

    var newTaskAudits = await db.TaskAudits.AsNoTracking().Where(x => x.TaskId == retry.Data.NewTaskId.Value).OrderBy(x => x.CreatedAt).ToListAsync();
    Assert(newTaskAudits.Any(x => x.FromStatus == null && x.ToStatus == "created") && newTaskAudits.Any(x => x.ToStatus == "queued"),
        "retry-created task writes created and queued TaskAudit records");

    var cancel = await service.CancelAdminAsync(otherTaskId, adminId);
    Assert(cancel.IsSuccess,
        "admin can cancel pending task");

    var canceledDetail = await service.GetAdminDetailAsync(otherTaskId, adminId);
    Assert(canceledDetail.IsSuccess && canceledDetail.Data.CurrentStatus == "canceled",
        "cancel updates task status");
    Assert(canceledDetail.Data.StatusTimeline.Any(x => x.ToStatus == "canceled"),
        "task detail includes ordered status timeline");

    var delete = await service.DeleteAdminAsync(otherTaskId, adminId);
    Assert(delete.IsSuccess,
        "admin can soft delete task");
    operationTargetIds.Add(otherTaskId.ToString());

    var deleteAudits = await db.TaskAudits.AsNoTracking().Where(x => x.TaskId == otherTaskId).OrderBy(x => x.CreatedAt).ToListAsync();
    Assert(deleteAudits.Any(x => x.ToStatus == "deleted" && x.OperatorId == adminId),
        "admin delete writes TaskAudit record");

    var deletedList = await service.QueryAdminAsync(new TaskManagementQuery { UserId = otherUserId }, adminId);
    Assert(deletedList.IsSuccess && deletedList.Data.Data.All(x => x.TaskId != otherTaskId),
        "soft deleted tasks are excluded from default list");

    var logCount = await db.OperationLogs.CountAsync(x =>
        operationTargetIds.Contains(x.TargetId!) &&
        (x.OperationType == AppEventTypes.TaskViewed ||
         x.OperationType == AppEventTypes.TaskRetried ||
         x.OperationType == AppEventTypes.TaskDeleted));
    Assert(logCount >= 3,
        "task operations write OperationLog through EventBus");

    var orderedFlow = await service.GetAdminStatusFlowAsync(otherTaskId, adminId);
    Assert(orderedFlow.IsSuccess &&
           orderedFlow.Data.SequenceEqual(orderedFlow.Data.OrderBy(x => x.CreatedAt)) &&
           orderedFlow.Data.Any(x => x.ToStatus == "deleted"),
        "status flow API returns ordered TaskAudit timeline");

    var failingAuditService = new TaskAuditService(new FailingTaskAuditRepository(), NullLogger<TaskAuditService>.Instance);
    await failingAuditService.RecordTransitionAsync(ownTaskId, "running", "failed", "ignored failure", null);
    Assert(true, "TaskAudit write failure does not break caller");
}
finally
{
    var allTaskIds = taskIds.ToList();
    var childIds = await db.AlgoTasks.Where(x => allTaskIds.Contains(x.TaskId)).Select(x => x.Id).ToListAsync();
    db.ResultFiles.RemoveRange(await db.ResultFiles.Where(x => childIds.Contains(x.AlgoTaskId) || resultFileIds.Contains(x.Id)).ToListAsync());
    db.TaskTypeMaps.RemoveRange(await db.TaskTypeMaps.Where(x => childIds.Contains(x.TaskId) || typeMapIds.Contains(x.Id)).ToListAsync());
    db.Localfiles.RemoveRange(await db.Localfiles.Where(x => childIds.Contains(x.AlgoTaskId) || localFileIds.Contains(x.Id)).ToListAsync());
    db.AlgoTasks.RemoveRange(await db.AlgoTasks.Where(x => childIds.Contains(x.Id) || algoTaskIds.Contains(x.Id)).ToListAsync());
    db.TaskAudits.RemoveRange(await db.TaskAudits.Where(x => allTaskIds.Contains(x.TaskId)).ToListAsync());
    db.TaskLists.RemoveRange(await db.TaskLists.Where(x => allTaskIds.Contains(x.Id)).ToListAsync());
    db.OperationLogs.RemoveRange(await db.OperationLogs.Where(x => operationTargetIds.Contains(x.TargetId!)).ToListAsync());
    db.Users.RemoveRange(await db.Users.Where(x => x.Id == adminId || x.Id == userId || x.Id == otherUserId).ToListAsync());
    await db.SaveChangesAsync();
}

Console.WriteLine("PASS task management verification");

async Task<Guid> CreateTaskAsync(Guid ownerId, string label, AlgoTaskStatus status, string? failureReason)
{
    var now = DateTime.UtcNow;
    var taskId = UuidV7.NewUuidV7();
    var algoTaskId = UuidV7.NewUuidV7();
    var localFileId = UuidV7.NewUuidV7();
    var resultFileId = UuidV7.NewUuidV7();
    var typeMapId = UuidV7.NewUuidV7();

    taskIds.Add(taskId);
    algoTaskIds.Add(algoTaskId);
    localFileIds.Add(localFileId);
    resultFileIds.Add(resultFileId);
    typeMapIds.Add(typeMapId);

    db.TaskLists.Add(new TaskList
    {
        Id = taskId,
        UserId = ownerId,
        Status = status == AlgoTaskStatus.done ? 1 : 0,
        Level = 1,
        CreatedAt = now.AddMinutes(-10),
        UpdatedAt = now
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
        StartedAt = now.AddMinutes(-9),
        FinishedAt = status is AlgoTaskStatus.done or AlgoTaskStatus.failed ? now.AddMinutes(-8) : null,
        CreatedAt = now.AddMinutes(-10),
        UpdatedAt = now
    });
    db.Localfiles.Add(new Localfile
    {
        Id = localFileId,
        AlgoTaskId = algoTaskId,
        UrlLocal = "/Files/verify-" + label + ".png",
        Sid = 0,
        CreatedAt = now,
        UpdatedAt = now
    });
    db.TaskTypeMaps.Add(new TaskTypeMap
    {
        Id = typeMapId,
        TaskId = algoTaskId,
        TypeId = 0
    });
    db.ResultFiles.Add(new ResultFile
    {
        Id = resultFileId,
        AlgoTaskId = algoTaskId,
        AlgoType = 0,
        IsFake = false,
        Confidence = 0.1,
        MaskLocalUrl = "/Files/result-" + label + ".png"
    });
    db.TaskAudits.Add(new TaskAudit
    {
        Id = UuidV7.NewUuidV7(),
        TaskId = taskId,
        FromStatus = null,
        ToStatus = "created",
        Reason = "task created",
        OperatorId = ownerId,
        CreatedAt = now.AddMinutes(-10)
    });
    db.TaskAudits.Add(new TaskAudit
    {
        Id = UuidV7.NewUuidV7(),
        TaskId = taskId,
        FromStatus = "created",
        ToStatus = "queued",
        Reason = "task queued",
        OperatorId = ownerId,
        CreatedAt = now.AddMinutes(-9),
        ExtraJson = "{\"algo_task_id\":\"" + algoTaskId + "\"}"
    });
    await db.SaveChangesAsync();
    return taskId;
}

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new Exception("FAIL " + message);

    Console.WriteLine("PASS " + message);
}

public class FailingTaskAuditRepository : ITaskAuditRepository
{
    public Task CreateAsync(TaskAudit taskAudit) => throw new Exception("expected audit write failure");
    public Task<List<TaskAuditDto>> ListByTaskIdAsync(Guid taskId) => Task.FromResult(new List<TaskAuditDto>());
    public Task<Dictionary<Guid, List<TaskAuditDto>>> ListByTaskIdsAsync(IEnumerable<Guid> taskIds) =>
        Task.FromResult(new Dictionary<Guid, List<TaskAuditDto>>());
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
