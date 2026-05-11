param(
    [string]$PostgresUser = "postgres",
    [string]$PostgresPassword,
    [string]$HostName = "localhost",
    [int]$Port = 5432
)

if ([string]::IsNullOrWhiteSpace($PostgresPassword)) {
    $securePassword = Read-Host "Введите пароль пользователя PostgreSQL" -AsSecureString
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $PostgresPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr)
}

$psqlPath = "C:\Program Files\PostgreSQL\18\bin\psql.exe"
$scriptPath = Join-Path $PSScriptRoot "..\sql\create-databases.sql"

if (-not (Test-Path $psqlPath)) {
    Write-Error "Не найден psql.exe по пути $psqlPath"
    exit 1
}

$env:PGPASSWORD = $PostgresPassword

try {
    & $psqlPath -h $HostName -p $Port -U $PostgresUser -d postgres -f $scriptPath
}
finally {
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}
