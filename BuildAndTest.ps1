Param (
    [bool] $E2ETest = $false
)

$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$buildConfiguration = 'Release'
$buildProjects = Join-Path -Path $scriptDir -ChildPath 'PlanningPokerCore.sln'

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
    $testPath = Join-Path -Path $scriptDir -ChildPath "Build\bin\$buildConfiguration\netcoreapp3.1"
    Get-ChildItem -Path $testPath -Filter '*.Test.dll' | ForEach-Object { $testFiles.Add('"' + $_.FullName + '"') }
    $testAssemblies = $testFiles.ToArray()
    $testAssembliesString = [string]::Join(' ', $testAssemblies)

    Write-Host "dotnet vstest $testAssembliesString --logger:trx"
    & dotnet vstest $testAssemblies --logger:trx
    if (!$?) {
        throw "dotnet vstest failed"
    }

    if ($E2ETest) {

        Write-Host "Phase: End-2-End tests" -ForegroundColor Green

        Write-Host "Step: npm install" -ForegroundColor Green
        Write-Host "npm install"
        & npm install
        if (!$?) {
            throw "npm install failed"
        }
    
        Write-Host "Step: install Selenium" -ForegroundColor Green
        $seleniumPath = Join-Path -Path $scriptDir -ChildPath 'node_modules\.bin\selenium-standalone'
        Write-Host "`"$seleniumPath`" install"
        & $seleniumPath install
        if (!$?) {
            throw "selenium-standalone install failed"
        }

        Write-Host "Step: End-2-End tests" -ForegroundColor Green
        $e2eTestPath = Join-Path -Path $testPath -ChildPath 'Duracellko.PlanningPoker.E2ETest.dll'
        Write-Host "dotnet vstest `"$e2eTestPath`" --logger:trx"
        & dotnet vstest $e2eTestPath --logger:trx
        if (!$?) {
            throw "dotnet vstest failed"
        }
    }

    Write-Host "Step: dotnet publish" -ForegroundColor Green
    Write-Host "dotnet publish `"$buildProjects`" --configuration $buildConfiguration"
    & dotnet publish $buildProjects --configuration $buildConfiguration
    if (!$?) {
        throw "dotnet publish failed"
    }

    $publishFolder = Join-Path -Path $scriptDir -ChildPath "Build\web\$buildConfiguration\netcoreapp3.1\publish"
    $dockerAppFolder = Join-Path -Path $scriptDir -ChildPath "docker\app"
    if (!(Test-Path -Path $dockerAppFolder)) {
        New-Item -Path $dockerAppFolder -ItemType Directory
    }

    Get-ChildItem -Path $dockerAppFolder | Remove-Item -Recurse -Force
    Get-ChildItem -Path $publishFolder | Copy-Item -Destination $dockerAppFolder -Recurse
}
catch {
    throw
}
