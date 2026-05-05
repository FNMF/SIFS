$ErrorActionPreference = "Stop"

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$tempRoot = Join-Path $env:TEMP ("sifs-eventbus-verify-" + [Guid]::NewGuid().ToString("N"))

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
using Microsoft.Extensions.DependencyInjection;
using SIFS.Shared.Extensions.EventBus;

var services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<IEventBus, EventBus>();

using var provider = services.BuildServiceProvider();
var eventBus = provider.GetRequiredService<IEventBus>();

var calls = new List<string>();

eventBus.Register(AppEventTypes.TaskCreated, e => calls.Add("first:" + e.EventType));
eventBus.Publish(new AppEvent { EventType = AppEventTypes.TaskCreated, TargetType = "task", TargetId = "1" });

Assert(calls.Count == 1 && calls[0] == "first:TASK_CREATED", "single listener is called");

eventBus.Register(AppEventTypes.TaskCreated, e => calls.Add("second"));
eventBus.Publish(new AppEvent { EventType = AppEventTypes.TaskCreated });

Assert(calls[^2] == "first:TASK_CREATED" && calls[^1] == "second", "multiple listeners are called in order");

eventBus.Register(AppEventTypes.UserLogin, e => throw new InvalidOperationException("expected listener failure"));
eventBus.Register(AppEventTypes.UserLogin, e => calls.Add("after-failure"));

eventBus.Publish(new AppEvent { EventType = AppEventTypes.UserLogin, ActorId = Guid.NewGuid() });

Assert(calls.Contains("after-failure"), "failing listener does not stop later listeners");

eventBus.Publish(new AppEvent { EventType = AppEventTypes.TaskDeleted });

var defaultDateEvent = new AppEvent { EventType = AppEventTypes.TaskRetried, CreatedAt = default };
eventBus.Publish(defaultDateEvent);
Assert(defaultDateEvent.CreatedAt != default, "created_at is normalized");

Console.WriteLine("PASS eventbus verification");

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
