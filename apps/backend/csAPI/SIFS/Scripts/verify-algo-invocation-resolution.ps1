$ErrorActionPreference = "Stop"

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$tempRoot = Join-Path $env:TEMP ("sifs-algo-invoke-verify-" + [Guid]::NewGuid().ToString("N"))

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
using Microsoft.Extensions.Options;
using SIFS.Application.AlgoTaskApp;
using SIFS.Application.Rbac;
using SIFS.Domain.Enum;
using SIFS.Infrastructure.Database;
using SIFS.Infrastructure.External;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Infrastructure.Repositories;
using SIFS.Shared.Extensions;
using SIFS.Shared.Helpers;
using SIFS.Shared.Results;

var services = new ServiceCollection();
services.AddLogging();
services.AddHttpClient();
services.AddDbContext<SIFSContext>();
services.Configure<AiServiceOptions>(options =>
{
    options.Endpoints[AiServiceType.FLDCF] = "http://fallback.example/detect/fldcf";
});
services.Configure<AppUrlOptions>(options =>
{
    options.BaseUrl = "http://localhost";
    options.PyBaseUrl = "http://localhost";
});
services.AddSingleton<IFileUrlBuilder, FileUrlBuilder>();
services.AddScoped<IAlgoModelRepository, AlgoModelRepository>();
services.AddScoped<IAlgorithmEndpointResolver, AlgorithmEndpointResolver>();
services.AddScoped<IAiService, AiService>();
services.AddScoped<IAlgoTaskRepository, AlgoTaskRepository>();
services.AddScoped<ITaskListRepository, TaskListRepository>();
services.AddScoped<IResultFileRepository, ResultFileRepository>();
services.AddScoped<IPermissionService, FakePermissionService>();
services.AddScoped<IAlgoTaskAppService, AlgoTaskAppService>();

using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<SIFSContext>();
var resolver = scope.ServiceProvider.GetRequiredService<IAlgorithmEndpointResolver>();

var originalFldcf = await db.AlgoModels.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "FLDCF");
var originalFldcfTracked = await db.AlgoModels.FirstOrDefaultAsync(x => x.Name == "FLDCF");
var createdModels = new List<AlgoModel>();
var taskIds = new List<Guid>();
var taskListIds = new List<Guid>();
var localFileIds = new List<Guid>();
var typeMapIds = new List<Guid>();
var insertedAlgoType = false;

try
{
    if (originalFldcfTracked != null)
    {
        db.AlgoModels.Remove(originalFldcfTracked);
        await db.SaveChangesAsync();
    }

    var fallback = await resolver.ResolveAsync(AiServiceType.FLDCF);
    Assert(fallback.IsSuccess && fallback.Data.IsFallback && fallback.Data.ApiUrl == "http://fallback.example/detect/fldcf",
        "existing FLDCF fallback works when no DB AlgoModel exists");

    var fldcf = originalFldcf ?? new AlgoModel
    {
        Id = 0,
        Name = "FLDCF",
        Enabled = true,
        ApiUrl = "http://db.example/detect/fldcf",
        Description = "verification restore",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    fldcf.Enabled = true;
    fldcf.ApiUrl = "http://db.example/detect/fldcf";
    db.AlgoModels.Add(fldcf);
    await db.SaveChangesAsync();

    var dbResolved = await resolver.ResolveAsync(AiServiceType.FLDCF);
    Assert(dbResolved.IsSuccess && !dbResolved.Data.IsFallback && dbResolved.Data.ApiUrl == "http://db.example/detect/fldcf",
        "enabled FLDCF AlgoModel uses DB api_url first");

    var trackedFldcf = await db.AlgoModels.FirstAsync(x => x.Name == "FLDCF");
    trackedFldcf.ApiUrl = "http://db.example/detect/fldcf-updated";
    trackedFldcf.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();

    var updatedResolved = await resolver.ResolveAsync(AiServiceType.FLDCF);
    Assert(updatedResolved.IsSuccess && updatedResolved.Data.ApiUrl.EndsWith("fldcf-updated"),
        "updating AlgoModel api_url changes URL used by new tasks");

    trackedFldcf.Enabled = false;
    await db.SaveChangesAsync();
    var disabled = await resolver.ResolveAsync(AiServiceType.FLDCF);
    Assert(!disabled.IsSuccess && disabled.Code == ResultCode.Forbidden && disabled.Message.Contains("ALGORITHM_DISABLED"),
        "disabled algorithm cannot be submitted");

    trackedFldcf.Enabled = true;
    trackedFldcf.ApiUrl = "";
    await db.SaveChangesAsync();
    var empty = await resolver.ResolveAsync(AiServiceType.FLDCF);
    Assert(!empty.IsSuccess && empty.Code == ResultCode.InvalidInput && empty.Message.Contains("ALGORITHM_API_URL_EMPTY"),
        "algorithm with empty api_url is rejected");

    db.AlgoModels.Remove(trackedFldcf);
    await db.SaveChangesAsync();

    var missing = await resolver.ResolveAsync(AiServiceType.EdgeDetector);
    Assert(!missing.IsSuccess && missing.Code == ResultCode.NotFound && missing.Message.Contains("ALGORITHM_NOT_FOUND"),
        "missing algorithm returns clear error");

    var executionModel = new AlgoModel
    {
        Id = 0,
        Name = "FLDCF",
        Enabled = true,
        ApiUrl = "http://127.0.0.1:9/detect/fldcf",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    db.AlgoModels.Add(executionModel);
    await db.SaveChangesAsync();

    if (!await db.AlgoTypes.AnyAsync(x => x.Id == 0))
    {
        db.AlgoTypes.Add(new AlgoType { Id = 0, Name = "FLDCF" });
        insertedAlgoType = true;
        await db.SaveChangesAsync();
    }

    var taskListId = UuidV7.NewUuidV7();
    var algoTaskId = UuidV7.NewUuidV7();
    var localFileId = UuidV7.NewUuidV7();
    var typeMapId = UuidV7.NewUuidV7();
    taskListIds.Add(taskListId);
    taskIds.Add(algoTaskId);
    localFileIds.Add(localFileId);
    typeMapIds.Add(typeMapId);

    db.TaskLists.Add(new TaskList
    {
        Id = taskListId,
        UserId = Guid.NewGuid(),
        Status = 0,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    });
    db.AlgoTasks.Add(new AlgoTask
    {
        Id = algoTaskId,
        TaskId = taskListId,
        Status = (int)AlgoTaskStatus.pending,
        AlgoModelId = 0,
        AlgoName = "FLDCF",
        AlgoApiUrl = "http://127.0.0.1:9/detect/fldcf",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    });
    db.Localfiles.Add(new Localfile
    {
        Id = localFileId,
        AlgoTaskId = algoTaskId,
        UrlLocal = "/Files/verify.png",
        Sid = 0,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    });
    db.TaskTypeMaps.Add(new TaskTypeMap
    {
        Id = typeMapId,
        TaskId = algoTaskId,
        TypeId = 0
    });
    await db.SaveChangesAsync();

    var appService = scope.ServiceProvider.GetRequiredService<IAlgoTaskAppService>();
    await appService.ExecuteAsync(algoTaskId);

    var failedTask = await db.AlgoTasks.AsNoTracking().FirstAsync(x => x.Id == algoTaskId);
    Assert(failedTask.Status == (int)AlgoTaskStatus.failed, "algorithm call failure marks task failed");
    Assert(!string.IsNullOrWhiteSpace(failedTask.FailureReason), "algorithm call failure stores task failure reason");

    var detailRepo = scope.ServiceProvider.GetRequiredService<IAlgoTaskRepository>();
    var detail = await detailRepo.GetDetailDtoByIdAsync(algoTaskId, Guid.Empty, true);
    Assert(detail?.FailureReason == failedTask.FailureReason, "failure reason is visible in task management response");
}
finally
{
    var resultFiles = await db.ResultFiles.Where(x => taskIds.Contains(x.AlgoTaskId)).ToListAsync();
    db.ResultFiles.RemoveRange(resultFiles);
    db.TaskTypeMaps.RemoveRange(await db.TaskTypeMaps.Where(x => typeMapIds.Contains(x.Id)).ToListAsync());
    db.Localfiles.RemoveRange(await db.Localfiles.Where(x => localFileIds.Contains(x.Id)).ToListAsync());
    db.AlgoTasks.RemoveRange(await db.AlgoTasks.Where(x => taskIds.Contains(x.Id)).ToListAsync());
    db.TaskLists.RemoveRange(await db.TaskLists.Where(x => taskListIds.Contains(x.Id)).ToListAsync());
    db.AlgoModels.RemoveRange(await db.AlgoModels.Where(x => x.Name == "FLDCF").ToListAsync());
    if (insertedAlgoType)
        db.AlgoTypes.RemoveRange(await db.AlgoTypes.Where(x => x.Id == 0).ToListAsync());
    await db.SaveChangesAsync();

    if (originalFldcf != null)
    {
        db.AlgoModels.Add(originalFldcf);
        await db.SaveChangesAsync();
    }
}

Console.WriteLine("PASS algorithm invocation resolution verification");

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new Exception("FAIL " + message);

    Console.WriteLine("PASS " + message);
}

public class FakePermissionService : IPermissionService
{
    public Task<Result> AssignRolesToUserAsync(Guid userId, IEnumerable<string> roleNames) => Task.FromResult(Result.Success());
    public Task<Result<List<string>>> GetUserPermissionsAsync(Guid userId) => Task.FromResult(Result<List<string>>.Success(new List<string>()));
    public Task<Result<bool>> HasPermissionAsync(Guid userId, string permissionCode) => Task.FromResult(Result<bool>.Success(true));
    public Task<Result<bool>> HasRoleAsync(Guid userId, string roleName) => Task.FromResult(Result<bool>.Success(false));
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
