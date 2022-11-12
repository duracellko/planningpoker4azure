Param (
    [bool] $E2ETest = $false,
    [string] $ChromeVersion = ''
)

$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$buildConfiguration = 'Release'
$buildProjects = Join-Path -Path $scriptDir -ChildPath 'PlanningPokerCore.sln'
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
    $testPath = Join-Path -Path $scriptDir -ChildPath "Build\bin\$buildConfiguration\net7.0"
    Get-ChildItem -Path $testPath -Filter '*.Test.dll' | ForEach-Object { $testFiles.Add($_.FullName) }
    $testAssemblies = $testFiles.ToArray()

    Write-Host "dotnet test $testAssembliesString --logger:trx"
    & dotnet test $testAssemblies --logger:trx
    if (!$?) {
        throw "dotnet test failed"
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
        $chromeVersionParameter = ''
        if (![string]::IsNullOrEmpty($ChromeVersion)) {
            $chromeVersionParameter = '--drivers.chrome.version=' + $ChromeVersion
        }

        Write-Host "`"$seleniumPath`" install $chromeVersionParameter"
        & $seleniumPath install $chromeVersionParameter
        if (!$?) {
            throw "selenium-standalone install failed"
        }

        Write-Host "Step: End-2-End tests" -ForegroundColor Green
        $e2eTestPath = Join-Path -Path $testPath -ChildPath 'Duracellko.PlanningPoker.E2ETest.dll'
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

    $publishFolder = Join-Path -Path $scriptDir -ChildPath "Build\web\$buildConfiguration\net7.0\publish"
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
