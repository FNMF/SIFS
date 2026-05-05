param(
    [string]$BaseUrl = "http://localhost:5021",
    [Parameter(Mandatory = $true)]
    [string]$AdminToken,
    [Parameter(Mandatory = $true)]
    [string]$UserToken
)

$ErrorActionPreference = "Stop"

function Invoke-Json {
    param(
        [string]$Method,
        [string]$Path,
        [string]$Token,
        [object]$Body = $null
    )

    $headers = @{}
    if (-not [string]::IsNullOrWhiteSpace($Token)) {
        $headers["Authorization"] = "Bearer $Token"
    }

    $params = @{
        Uri = "$BaseUrl$Path"
        Method = $Method
        Headers = $headers
    }

    if ($null -ne $Body) {
        $params["ContentType"] = "application/json"
        $params["Body"] = ($Body | ConvertTo-Json -Depth 10)
    }

    try {
        $response = Invoke-WebRequest @params
        return @{
            Status = [int]$response.StatusCode
            Body = if ($response.Content) { $response.Content | ConvertFrom-Json } else { $null }
        }
    }
    catch {
        if ($_.Exception.Response -ne $null) {
            return @{
                Status = [int]$_.Exception.Response.StatusCode
                Body = $null
            }
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

$name = "HTTP_VERIFY_ALGO_$([Guid]::NewGuid().ToString("N"))"

Assert-Status "unauthenticated create is rejected" 401 (Invoke-Json "POST" "/api/admin/algos" "" @{ name = $name; api_url = "http://127.0.0.1:19000/detect" }).Status
Assert-Status "ordinary user create is forbidden" 403 (Invoke-Json "POST" "/api/admin/algos" $UserToken @{ name = $name; api_url = "http://127.0.0.1:19000/detect" }).Status

$create = Invoke-Json "POST" "/api/admin/algos" $AdminToken @{
    name = $name
    api_url = "http://127.0.0.1:19000/detect"
    description = "http verification"
    reserved_json = @{ threshold = 0.5 }
}
Assert-Status "admin can create algorithm" 200 $create.Status

$id = $create.Body.id

Assert-Status "admin can update algorithm" 200 (Invoke-Json "PUT" "/api/admin/algos/$id" $AdminToken @{ description = "updated"; reserved_json = @{ threshold = 0.8 } }).Status
Assert-Status "admin can enable algorithm" 200 (Invoke-Json "POST" "/api/admin/algos/$id/enable" $AdminToken).Status
Assert-Status "admin can disable algorithm" 200 (Invoke-Json "POST" "/api/admin/algos/$id/disable" $AdminToken).Status
Assert-Status "admin can list algorithms" 200 (Invoke-Json "GET" "/api/admin/algos?name=$name&page=1&pageSize=10" $AdminToken).Status
Assert-Status "admin can get algorithm detail" 200 (Invoke-Json "GET" "/api/admin/algos/$id" $AdminToken).Status
Assert-Status "admin can soft delete algorithm" 200 (Invoke-Json "DELETE" "/api/admin/algos/$id" $AdminToken).Status
