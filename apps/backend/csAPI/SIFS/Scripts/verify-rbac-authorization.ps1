param(
    [string]$BaseUrl = "http://localhost:5021",
    [Parameter(Mandatory = $true)]
    [string]$AdminToken,
    [Parameter(Mandatory = $true)]
    [string]$UserToken,
    [Parameter(Mandatory = $true)]
    [string]$OwnTaskId,
    [Parameter(Mandatory = $true)]
    [string]$OtherTaskId
)

$ErrorActionPreference = "Stop"

function Invoke-Status {
    param(
        [string]$Path,
        [string]$Token
    )

    $headers = @{}
    if (-not [string]::IsNullOrWhiteSpace($Token)) {
        $headers["Authorization"] = "Bearer $Token"
    }

    try {
        $response = Invoke-WebRequest -Uri "$BaseUrl$Path" -Method Get -Headers $headers
        return [int]$response.StatusCode
    }
    catch {
        if ($_.Exception.Response -ne $null) {
            return [int]$_.Exception.Response.StatusCode
        }

        throw
    }
}

function Assert-Status {
    param(
        [string]$Name,
        [int]$Expected,
        [int]$Actual
    )

    if ($Expected -ne $Actual) {
        throw "$Name expected HTTP $Expected but got HTTP $Actual"
    }

    Write-Host "PASS $Name -> HTTP $Actual"
}

Assert-Status "unauthenticated management request" 401 (Invoke-Status "/api/admin/rbac/permissions/me" "")
Assert-Status "authenticated user without admin access" 403 (Invoke-Status "/api/admin/rbac/permissions/me" $UserToken)
Assert-Status "admin accesses management endpoint" 200 (Invoke-Status "/api/admin/rbac/permissions/me" $AdminToken)
Assert-Status "ordinary user accesses own task" 200 (Invoke-Status "/api/de-task/$OwnTaskId" $UserToken)
Assert-Status "ordinary user accesses another user task" 403 (Invoke-Status "/api/de-task/$OtherTaskId" $UserToken)
Assert-Status "permission check works independently of role endpoint" 200 (Invoke-Status "/api/admin/rbac/permissions/me" $AdminToken)
Assert-Status "role check helper works" 200 (Invoke-Status "/api/admin/rbac/roles/admin-check" $AdminToken)
