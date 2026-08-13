param(
    [string]$Configuration = "Release",
    [string]$Version = "0.1.0"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = $PSScriptRoot
$publishDirectory = Join-Path $repositoryRoot "artifacts\publish"
$iscc = Get-Command iscc.exe -ErrorAction SilentlyContinue

if (-not $iscc) {
    $userInstall = Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe"
    $machineInstall = Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"
    $isccPath = @($userInstall, $machineInstall) | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $isccPath) {
        throw "Inno Setup is required. Install it with: winget install --id JRSoftware.InnoSetup --exact"
    }
}
else {
    $isccPath = $iscc.Source
}

dotnet publish "$repositoryRoot\VanguardVolume.App\VanguardVolume.App.csproj" `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=false `
    -p:Version=$Version `
    --output $publishDirectory

& $isccPath "/DMyAppVersion=$Version" "$repositoryRoot\installer\VanguardVolume.Setup.iss"
