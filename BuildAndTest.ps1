Param (
    [bool] $E2ETest = $false
)

$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$buildConfiguration = 'Release'
$buildProjects = Join-Path -Path $scriptDir -ChildPath 'PlanningPoker.slnx'
$webProject = Join-Path -Path $scriptDir '.\src\Duracellko.PlanningPoker.Web\Duracellko.PlanningPoker.Web.csproj'

try {

    Write-Host "Step: dotnet restore" -ForegroundColor Green
    Write-Host "dotnet restore `"$buildProjects`""
    & dotnet restore $buildProjects
    if (!$?) {
        throw "dotnet restore failed"
    }

    Write-Host "Step: dotnet build" -ForegroundColor Green
    Write-Host "dotnet build `"$buildProjects`" --configuration $buildConfiguration"
    & dotnet build $buildProjects --configuration $buildConfiguration
    if (!$?) {
        throw "dotnet build failed"
    }

    Write-Host "Step: unit tests" -ForegroundColor Green
    $testFiles = [System.Collections.Generic.List[string]]::new()
    $binPath = Join-Path -Path $scriptDir -ChildPath "artifacts\bin"
    $testDirectories = Get-ChildItem -Path $binPath -Filter '*.Test' -Directory
    $testDirectories | ForEach-Object {
        $testProjectName = Split-Path -Path $_ -Leaf
        $testFile = Join-Path -Path $_ -ChildPath $buildConfiguration
        $testFile = Join-Path -Path $testFile -ChildPath "$testProjectName.dll"
        $testFiles.Add($testFile)
    }
    $testAssemblies = $testFiles.ToArray()

    Write-Host "dotnet test $testAssemblies --logger:trx"
    & dotnet test $testAssemblies --logger:trx
    if (!$?) {
        throw "dotnet test failed"
    }

    if ($E2ETest) {

        Write-Host "Phase: End-2-End tests" -ForegroundColor Green

        Write-Host "Step: playwright install" -ForegroundColor Green
        $playwrightPath = Join-Path -Path $scriptDir -ChildPath "artifacts\bin\Duracellko.PlanningPoker.E2ETest\$buildConfiguration\playwright.ps1"
        & $playwrightPath install chromium
        if (!$?) {
            throw "playwright install failed"
        }
    
        Write-Host "Step: End-2-End tests" -ForegroundColor Green
        $e2eTestPath = Join-Path -Path $binPath -ChildPath "Duracellko.PlanningPoker.E2ETest\$buildConfiguration\Duracellko.PlanningPoker.E2ETest.dll"
        Write-Host "dotnet test `"$e2eTestPath`" --logger:trx"
        & dotnet test $e2eTestPath --logger:trx
        if (!$?) {
            throw "dotnet test failed"
        }
    }

    Write-Host "Step: dotnet publish" -ForegroundColor Green
    Write-Host "dotnet publish `"$webProject`" --configuration $buildConfiguration"
    & dotnet publish $webProject --configuration $buildConfiguration
    if (!$?) {
        throw "dotnet publish failed"
    }
}
catch {
    throw
}
