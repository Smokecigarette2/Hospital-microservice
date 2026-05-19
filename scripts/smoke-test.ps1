param(
    [string]$GatewayBaseUrl = "http://localhost:7133",
    [string]$CatalogBaseUrl = "http://localhost:7260",
    [string]$BookingBaseUrl = "http://localhost:7050",
    [string]$IdentityBaseUrl = "http://localhost:7081",
    [string]$AdminUsername = "admin",
    [string]$AdminPassword = "Admin123!"
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "==> $Message"
}

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Uri,
        [object]$Body = $null,
        [string]$Token = $null
    )

    $headers = @{}

    if ($Token) {
        $headers.Authorization = "Bearer $Token"
    }

    $parameters = @{
        Method = $Method
        Uri = $Uri
        Headers = $headers
        ContentType = "application/json"
    }

    if ($null -ne $Body) {
        $parameters.Body = ($Body | ConvertTo-Json -Depth 10)
    }

    return Invoke-RestMethod @parameters
}

function Test-Swagger {
    param([string]$BaseUrl)

    $credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${AdminUsername}:${AdminPassword}"))
    Invoke-WebRequest -Uri "$BaseUrl/swagger/index.html" -Headers @{ Authorization = "Basic $credentials" } -UseBasicParsing | Out-Null
}

Write-Step "Checking health endpoints"
Invoke-WebRequest -Uri "$GatewayBaseUrl/health" -UseBasicParsing | Out-Null
Invoke-WebRequest -Uri "$CatalogBaseUrl/health" -UseBasicParsing | Out-Null
Invoke-WebRequest -Uri "$BookingBaseUrl/health" -UseBasicParsing | Out-Null
Invoke-WebRequest -Uri "$IdentityBaseUrl/health" -UseBasicParsing | Out-Null

Write-Step "Checking protected Swagger pages"
Test-Swagger -BaseUrl $CatalogBaseUrl
Test-Swagger -BaseUrl $BookingBaseUrl
Test-Swagger -BaseUrl $IdentityBaseUrl

Write-Step "Logging in as default admin"
$adminLogin = Invoke-Api -Method POST -Uri "$GatewayBaseUrl/api/Auth/login" -Body @{
    username = $AdminUsername
    password = $AdminPassword
}
$adminToken = $adminLogin.token

Write-Step "Registering and logging in as a regular user"
$userName = "smoke_user_$([Guid]::NewGuid().ToString('N').Substring(0, 8))"
Invoke-Api -Method POST -Uri "$GatewayBaseUrl/api/Auth/register" -Body @{
    username = $userName
    password = "User123!"
} | Out-Null
$userLogin = Invoke-Api -Method POST -Uri "$GatewayBaseUrl/api/Auth/login" -Body @{
    username = $userName
    password = "User123!"
}
$userToken = $userLogin.token

Write-Step "Creating, reading, filtering and updating a doctor"
$doctor = Invoke-Api -Method POST -Uri "$GatewayBaseUrl/api/Doctors" -Token $adminToken -Body @{
    fullName = "Dr. Smoke Test"
    specialization = "Cardiology"
    department = "Heart Center"
}

Invoke-Api -Method GET -Uri "$GatewayBaseUrl/api/Doctors" | Out-Null
Invoke-Api -Method GET -Uri "$GatewayBaseUrl/api/Doctors/$($doctor.id)" | Out-Null
Invoke-Api -Method GET -Uri "$GatewayBaseUrl/api/Doctors/filter?specialization=card&department=heart&available=true&search=smoke&sortBy=department" | Out-Null

Invoke-Api -Method PUT -Uri "$GatewayBaseUrl/api/Doctors/$($doctor.id)" -Token $adminToken -Body @{
    fullName = "Dr. Smoke Test Updated"
    specialization = "Cardiology"
    department = "Heart Center"
    isAvailable = $true
} | Out-Null

Write-Step "Creating, reading, listing, updating and cancelling an appointment"
$firstDate = [DateTime]::UtcNow.AddDays(1).ToString("o")
$secondDate = [DateTime]::UtcNow.AddDays(2).ToString("o")

$appointment = Invoke-Api -Method POST -Uri "$GatewayBaseUrl/api/Appointments" -Token $userToken -Body @{
    doctorId = $doctor.id
    patientName = "Smoke Patient"
    appointmentDate = $firstDate
}

Invoke-Api -Method GET -Uri "$GatewayBaseUrl/api/Appointments/$($appointment.id)" -Token $userToken | Out-Null
Invoke-Api -Method GET -Uri "$GatewayBaseUrl/api/Appointments" -Token $adminToken | Out-Null

Invoke-Api -Method PUT -Uri "$GatewayBaseUrl/api/Appointments/$($appointment.id)" -Token $adminToken -Body @{
    doctorId = $doctor.id
    patientName = "Smoke Patient Updated"
    appointmentDate = $secondDate
} | Out-Null

Invoke-Api -Method DELETE -Uri "$GatewayBaseUrl/api/Appointments/$($appointment.id)" -Token $adminToken | Out-Null
Invoke-Api -Method DELETE -Uri "$GatewayBaseUrl/api/Doctors/$($doctor.id)" -Token $adminToken | Out-Null

Write-Host "Smoke test passed."
