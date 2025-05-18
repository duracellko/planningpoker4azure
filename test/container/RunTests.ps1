Param (
    [string] $PlanningPokerImageTag = '',
    [string] $TestResultPath = ''
)

$projectPath = $PSScriptRoot
$pesterVersion = '5.7.1'
$rabbitmqVersion = '4.1'

$imageTag = 'local-test'
if (![string]::IsNullOrEmpty($PlanningPokerImageTag)) {
    $imageTag = $PlanningPokerImageTag
}

$composeProjectName = 'planningpoker'
$rabbitmqPassword = (New-Guid).ToString()
$applicationPorts = @(5001, 5002, 5003)

function RandomizeApplicationPorts() {
    $random = [System.Random]::new()
    for ($i = 0; $i -lt $applicationPorts.Length; $i++) {
        $applicationPorts[$i] = $random.Next(5000, 20000)
    }
}

function SetupEnvironmentVariables() {
    param (
        [Parameter(Mandatory = $true)]
        [string] $AppImageTag,
        [Parameter(Mandatory = $true)]
        [int[]] $ApplicationPorts,
        [Parameter(Mandatory = $true)]
        [string] $RabbitMQPassword
    )

    $env:PLANNINGPOKER_IMAGENAME = 'duracellko/planningpoker:' + $AppImageTag
    $env:PLANNINGPOKER_APP1_PORT = $ApplicationPorts[0]
    $env:PLANNINGPOKER_APP2_PORT = $ApplicationPorts[1]
    $env:PLANNINGPOKER_APP3_PORT = $ApplicationPorts[2]
    $env:PLANNINGPOKER_APP_RABBITMQ_PASSWORD = $RabbitMQPassword
    $env:PLANNINGPOKER_RABBITMQ_VERSION = $rabbitmqVersion
}

function ComposeDockerUp() {
    param (
        [Parameter(Mandatory = $true)]
        [string] $ComposePath,
        [Parameter(Mandatory = $true)]
        [string] $ProjectName
    )

    & docker compose -f $ComposePath -p $ProjectName up -d

    if ($LastExitCode -ne 0) {
        throw "Docker Compose Up failed."
    }
}

function ComposeDockerDown() {
    param (
        [Parameter(Mandatory = $true)]
        [string] $ComposePath,
        [Parameter(Mandatory = $true)]
        [string] $ProjectName
    )

    & docker compose -f $ComposePath -p $ProjectName down

    if ($LastExitCode -ne 0) {
        throw "Docker Compose Down failed."
    }
}

# Install Pester

$pesterModule = Get-Module -Name 'Pester' -ListAvailable | Where-Object -Property 'Version' -EQ -Value $pesterVersion
if ($null -eq $pesterModule) {
    $moduleInfo = Install-Module -Name 'Pester' -MinimumVersion $pesterVersion -Scope CurrentUser -Force -PassThru
    $pesterModule = Get-Module -Name $moduleInfo.Name -ListAvailable | Where-Object -Property 'Version' -EQ -Value $moduleInfo.Version
}

$pesterModule | Import-Module

# Run tests

$composeFilePath = Join-Path -Path $projectPath -ChildPath 'compose.yml'

try {
    RandomizeApplicationPorts
    SetupEnvironmentVariables -AppImageTag $imageTag -ApplicationPorts $applicationPorts -RabbitMQPassword $rabbitmqPassword

    ComposeDockerUp -ComposePath $composeFilePath -ProjectName $composeProjectName

    # 3 seconds is configured timeout for looking for existing instance. So first initialization takes 3 seconds.
    Start-Sleep -Seconds 3

    $testScriptPath = Join-Path -Path $projectPath -ChildPath 'PlanningPokerDocker.Tests.ps1'
    $pesterData = @{
        ServicePorts = $applicationPorts
        DockerComposePath = $composeFilePath
        DockerComposeProjectName = $composeProjectName
        DockerComposeServiceNames = @(
            'planningpoker-r1'
            'planningpoker-r2'
            'planningpoker-r3'
        )
    }
    $pesterContainer = New-PesterContainer -Path $testScriptPath -Data $pesterData

    $pesterConfiguration = New-PesterConfiguration
    $pesterConfiguration.Run.Path = $projectPath
    $pesterConfiguration.Run.PassThru = $true
    if (![string]::IsNullOrEmpty($TestResultPath)) {
        $pesterConfiguration.TestResult.Enabled = $true
        $pesterConfiguration.TestResult.OutputPath = $TestResultPath
        $pesterConfiguration.TestResult.TestSuiteName = 'PlanningPoker-Docker'
    }

    $pesterConfiguration.Run.Container = $pesterContainer
    
    $result = Invoke-Pester -Configuration $pesterConfiguration

    if ($result.Result -ne 'Passed') {
        throw "Planning Poker Docker tests have failed."
    }
}
finally {
    ComposeDockerDown -ComposePath $composeFilePath -ProjectName $composeProjectName
}