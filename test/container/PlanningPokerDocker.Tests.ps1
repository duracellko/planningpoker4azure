Param (
    [int[]] $ServicePorts = $null,
    [string] $DockerComposePath = $null,
    [string] $DockerComposeProjectName = $null,
    [string[]] $DockerComposeServiceNames = $null
)

BeforeAll {
    $TeamStates = @{
        Initial = 0
        EstimationInProgress = 1
        EstimationFinished = 2
        EstimationCanceled = 3
    }
    
    $MessageTypes = @{
        Empty = 0
        MemberJoined = 1
        MemberDisconnected = 2
        EstimationStarted = 3
        EstimationEnded = 4
        EstimationCanceled = 5
        MemberEstimated = 6
        TimerStarted = 7
        TimerCanceled = 8
    }
    
    $MemberTypes = @{
        ScrumMaster = 'ScrumMaster'
        Member = 'Member'
        Observer = 'Observer'
    }

    $teamName = 'MyTestTeam'
    $apiPath = 'api/PlanningPokerService/'
    $sessions = @(
        @{
            Name = 'Alice'
        },
        @{
            Name = 'Bob'
        },
        @{
            Name = 'Charlie'
        }
    )

    for ($i = 0; $i -lt $sessions.Length; $i++) {
        $sessions[$i].SessionId = ''
        $sessions[$i].LastMessageId = 0

        $port = 5001 + $i
        if ($null -ne $ServicePorts) {
            $port = $ServicePorts[$i]
        }

        $sessions[$i].BaseUri = "http://localhost:$port/"
        Write-Host "Testing service: $($sessions[$i].BaseUri)"

        if ($null -ne $DockerComposeServiceNames) {
            $sessions[$i].DockerComposeServiceName = $DockerComposeServiceNames[$i]
        }
    }

    function Create-Team([int] $Index) {
        $session = $sessions[$Index]
        $uri = "$($session.BaseUri)$($apiPath)CreateTeam?teamName=$teamName&scrumMasterName=$($session.Name)&deck=Standard"
        $response = Invoke-RestMethod -Uri $uri

        $response.SessionId | Should -Not -BeNullOrEmpty
        $session.SessionId = $response.SessionId
        $session.LastMessageId = 0

        $response.ScrumTeam.Name | Should -Be $teamName
        $response.ScrumTeam.ScrumMaster.Type | Should -Be $MemberTypes.ScrumMaster
        $response.ScrumTeam.ScrumMaster.Name | Should -Be $sessions[0].Name
        $response.ScrumTeam.Members.Length | Should -Be 1
        $response.ScrumTeam.Members[0].Type | Should -Be $MemberTypes.ScrumMaster
        $response.ScrumTeam.Members[0].Name | Should -Be $sessions[0].Name
        $response.ScrumTeam.State | Should -Be $TeamStates.Initial
        return $response
    }

    function Join-Team([int] $Index) {
        $session = $sessions[$Index]
        $uri = "$($session.BaseUri)$($apiPath)JoinTeam?teamName=$teamName&memberName=$($session.Name)&asObserver=False"
        $response = Invoke-RestMethod -Uri $uri

        $response.SessionId | Should -Not -BeNullOrEmpty
        $session.SessionId = $response.SessionId
        $session.LastMessageId = 0

        $response.ScrumTeam.Name | Should -Be $teamName
        $response.ScrumTeam.ScrumMaster.Type | Should -Be $MemberTypes.ScrumMaster
        $response.ScrumTeam.ScrumMaster.Name | Should -Be $sessions[0].Name
        return $response
    }

    function Disconnect-Team([int] $Index) {
        $session = $sessions[$Index]
        $uri = "$($session.BaseUri)$($apiPath)DisconnectTeam?teamName=$teamName&memberName=$($session.Name)"
        Invoke-RestMethod -Uri $uri | Out-Null
        $session.SessionId = ''
        $session.LastMessageId = 0
    }

    function Start-Estimation([int] $Index) {
        $session = $sessions[$Index]
        $uri = "$($session.BaseUri)$($apiPath)StartEstimation?teamName=$teamName"
        Invoke-RestMethod -Uri $uri | Out-Null
    }

    function Cancel-Estimation([int] $Index) {
        $session = $sessions[$Index]
        $uri = "$($session.BaseUri)$($apiPath)CancelEstimation?teamName=$teamName"
        Invoke-RestMethod -Uri $uri | Out-Null
    }

    function Submit-Estimation([int] $Index, [double] $Estimation = [double]::NaN) {
        $session = $sessions[$Index]
        $uri = "$($session.BaseUri)$($apiPath)SubmitEstimation?teamName=$teamName&memberName=$($session.Name)"
        if (![double]::IsNaN($Estimation)) {
            $invariantEstimation = $Estimation.ToString('G17', [System.Globalization.CultureInfo]::InvariantCulture)
            $invariantEstimation = [System.Text.Encodings.Web.UrlEncoder]::Default.Encode($invariantEstimation)
            $uri = "$uri&estimation=$invariantEstimation"
        }

        Invoke-RestMethod -Uri $uri | Out-Null
    }

    function Get-Messages([int] $Index, [int] $AtLeast = 1) {
        $session = $sessions[$Index]
        $uri = "$($session.BaseUri)$($apiPath)GetMessages?teamName=$teamName&memberName=$($session.Name)&sessionId=$($session.SessionId)&lastMessageId=$($session.LastMessageId)"

        $messageCount = 0
        $response = @()
        while ($messageCount -lt $AtLeast) {
            $response = Invoke-RestMethod -Uri $uri
            $response | Should -Not -BeNullOrEmpty
            $messageCount = $response.Length
        }

        $response.Length | Should -BeGreaterOrEqual $AtLeast
        $lastMessageId = $response[-1].Id
        $lastMessageId | Should -Not -BeNullOrEmpty
        $lastMessageId | Should -BeGreaterThan 0
        $session.LastMessageId = $lastMessageId

        return $response
    }

    function VerifyServiceIsHealthy([int] $Index) {
        $session = $sessions[$Index]
        $healthStatus = ''
        $uri = "$($session.BaseUri)health"
        $maxRetryCount = 10

        for ($i = 1; $i -le $maxRetryCount; $i++) {
            try {
                $healthStatus = Invoke-RestMethod -Uri $uri
                break
            }
            catch {
                Write-Host "Service '$uri' is unhealthy. Attempt $i - $_"
                if ($i -eq $maxRetryCount) {
                    throw
                }
            }

            Start-Sleep -Milliseconds ($i * 200)
        }

        $healthStatus | Should -Be 'Healthy'
    }

    function StartPlanningPokerService([int] $Index) {
        $session = $sessions[$Index]
        $DockerComposePath | Should -Not -BeNullOrEmpty
        $DockerComposeProjectName | Should -Not -BeNullOrEmpty
        $session.DockerComposeServiceName | Should -Not -BeNullOrEmpty

        & docker compose -f $DockerComposePath -p $DockerComposeProjectName start $session.DockerComposeServiceName

        $LastExitCode | Should -Be 0
    }

    function StopPlanningPokerService([int] $Index) {
        $session = $sessions[$Index]
        $DockerComposePath | Should -Not -BeNullOrEmpty
        $DockerComposeProjectName | Should -Not -BeNullOrEmpty
        $session.DockerComposeServiceName | Should -Not -BeNullOrEmpty

        & docker compose -f $DockerComposePath -p $DockerComposeProjectName stop $session.DockerComposeServiceName

        $LastExitCode | Should -Be 0
    }
}

Describe 'Planning Poker' {
    It 'Should share team on all nodes' {
        VerifyServiceIsHealthy -Index 0
        VerifyServiceIsHealthy -Index 1
        VerifyServiceIsHealthy -Index 2

        $response = Create-Team -Index 0
        $response.ScrumTeam.EstimationResult | Should -BeNullOrEmpty
        $response.ScrumTeam.EstimationParticipants | Should -BeNullOrEmpty

        $response = Join-Team -Index 1
        $response.ScrumTeam.State | Should -Be $TeamStates.Initial
        $response.ScrumTeam.Members.Length | Should -Be 2
        $member = $response.ScrumTeam.Members | Where-Object -Property Name -EQ -Value $sessions[0].Name
        $member | Should -Not -BeNullOrEmpty
        $member.Name | Should -Be $sessions[0].Name
        $member.Type | Should -Be $MemberTypes.ScrumMaster
        $member = $response.ScrumTeam.Members | Where-Object -Property Name -EQ -Value $sessions[1].Name
        $member | Should -Not -BeNullOrEmpty
        $member.Name | Should -Be $sessions[1].Name
        $member.Type | Should -Be $MemberTypes.Member
        $response.ScrumTeam.EstimationResult | Should -BeNullOrEmpty
        $response.ScrumTeam.EstimationParticipants | Should -BeNullOrEmpty

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberJoined
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[1].Name

        # Round 1

        Start-Estimation -Index 0

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.EstimationStarted

        $response = Get-Messages -Index 1
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.EstimationStarted

        Submit-Estimation -Index 1 -Estimation 8

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[1].Name

        $response = Get-Messages -Index 1
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[1].Name

        $response = Join-Team -Index 2
        $response.ScrumTeam.State | Should -Be $TeamStates.EstimationInProgress
        $response.ScrumTeam.Members.Length | Should -Be 3
        $member = $response.ScrumTeam.Members | Where-Object -Property Name -EQ -Value $sessions[0].Name
        $member | Should -Not -BeNullOrEmpty
        $member.Name | Should -Be $sessions[0].Name
        $member.Type | Should -Be $MemberTypes.ScrumMaster
        $member = $response.ScrumTeam.Members | Where-Object -Property Name -EQ -Value $sessions[1].Name
        $member | Should -Not -BeNullOrEmpty
        $member.Name | Should -Be $sessions[1].Name
        $member.Type | Should -Be $MemberTypes.Member
        $member = $response.ScrumTeam.Members | Where-Object -Property Name -EQ -Value $sessions[2].Name
        $member | Should -Not -BeNullOrEmpty
        $member.Name | Should -Be $sessions[2].Name
        $member.Type | Should -Be $MemberTypes.Member
        $response.ScrumTeam.EstimationResult | Should -BeNullOrEmpty
        $response.ScrumTeam.EstimationParticipants | Should -Not -BeNullOrEmpty
        $response.ScrumTeam.EstimationParticipants.Length | Should -Be 2
        $estimationParticipant = $response.ScrumTeam.EstimationParticipants | Where-Object -Property MemberName -EQ -Value $sessions[0].Name
        $estimationParticipant.MemberName | Should -Be $sessions[0].Name
        $estimationParticipant.Estimated | Should -BeFalse
        $estimationParticipant = $response.ScrumTeam.EstimationParticipants | Where-Object -Property MemberName -EQ -Value $sessions[1].Name
        $estimationParticipant.MemberName | Should -Be $sessions[1].Name
        $estimationParticipant.Estimated | Should -BeTrue

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberJoined
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[2].Name

        $response = Get-Messages -Index 1
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberJoined
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[2].Name

        Submit-Estimation -Index 0 -Estimation 1

        $response = Get-Messages -Index 0 -AtLeast 2
        $response.Length | Should -Be 2
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.ScrumMaster
        $response[0].Member.Name | Should -Be $sessions[0].Name
        $response[1].Type | Should -Be $MessageTypes.EstimationEnded
        $response[1].EstimationResult | Should -Not -BeNullOrEmpty
        $response[1].EstimationResult.Length | Should -Be 2
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[0].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[0].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.ScrumMaster
        $estimationResultItem.Estimation.Value | Should -Be 1
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[1].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[1].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -Be 8

        $response = Get-Messages -Index 1 -AtLeast 2
        $response.Length | Should -Be 2
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.ScrumMaster
        $response[0].Member.Name | Should -Be $sessions[0].Name
        $response[1].Type | Should -Be $MessageTypes.EstimationEnded
        $response[1].EstimationResult | Should -Not -BeNullOrEmpty
        $response[1].EstimationResult.Length | Should -Be 2
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[0].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[0].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.ScrumMaster
        $estimationResultItem.Estimation.Value | Should -Be 1
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[1].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[1].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -Be 8

        $response = Get-Messages -Index 2 -AtLeast 2
        $response.Length | Should -Be 2
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.ScrumMaster
        $response[0].Member.Name | Should -Be $sessions[0].Name
        $response[1].Type | Should -Be $MessageTypes.EstimationEnded
        $response[1].EstimationResult | Should -Not -BeNullOrEmpty
        $response[1].EstimationResult.Length | Should -Be 2
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[0].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[0].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.ScrumMaster
        $estimationResultItem.Estimation.Value | Should -Be 1
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[1].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[1].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -Be 8

        # Round 2

        Start-Estimation -Index 0

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.EstimationStarted

        $response = Get-Messages -Index 1
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.EstimationStarted

        $response = Get-Messages -Index 2
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.EstimationStarted

        Submit-Estimation -Index 0 -Estimation 2

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.ScrumMaster
        $response[0].Member.Name | Should -Be $sessions[0].Name

        $response = Get-Messages -Index 1
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.ScrumMaster
        $response[0].Member.Name | Should -Be $sessions[0].Name

        $response = Get-Messages -Index 2
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.ScrumMaster
        $response[0].Member.Name | Should -Be $sessions[0].Name

        Submit-Estimation -Index 2 -Estimation 3

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[2].Name

        $response = Get-Messages -Index 1
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[2].Name

        $response = Get-Messages -Index 2
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[2].Name

        Cancel-Estimation -Index 0

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.EstimationCanceled

        $response = Get-Messages -Index 1
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.EstimationCanceled

        $response = Get-Messages -Index 2
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.EstimationCanceled

        # Round 3

        Start-Estimation -Index 0

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.EstimationStarted

        $response = Get-Messages -Index 1
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.EstimationStarted

        $response = Get-Messages -Index 2
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.EstimationStarted

        # Estimate positive infinity
        Submit-Estimation -Index 2 -Estimation 5.4861240687936887E+303

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[2].Name

        $response = Get-Messages -Index 1
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[2].Name

        $response = Get-Messages -Index 2
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[2].Name

        Submit-Estimation -Index 0 -Estimation 0

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.ScrumMaster
        $response[0].Member.Name | Should -Be $sessions[0].Name

        $response = Get-Messages -Index 1
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.ScrumMaster
        $response[0].Member.Name | Should -Be $sessions[0].Name

        $response = Get-Messages -Index 2
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.ScrumMaster
        $response[0].Member.Name | Should -Be $sessions[0].Name

        Submit-Estimation -Index 1 -Estimation 0.5

        $response = Get-Messages -Index 0 -AtLeast 2
        $response.Length | Should -Be 2
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[1].Name
        $response[1].Type | Should -Be $MessageTypes.EstimationEnded
        $response[1].EstimationResult | Should -Not -BeNullOrEmpty
        $response[1].EstimationResult.Length | Should -Be 3
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[0].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[0].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.ScrumMaster
        $estimationResultItem.Estimation.Value | Should -Be 0
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[1].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[1].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -Be 0.5
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[2].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[2].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -Be 5.4861240687936887E+303

        $response = Get-Messages -Index 1 -AtLeast 2
        $response.Length | Should -Be 2
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[1].Name
        $response[1].Type | Should -Be $MessageTypes.EstimationEnded
        $response[1].EstimationResult | Should -Not -BeNullOrEmpty
        $response[1].EstimationResult.Length | Should -Be 3
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[0].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[0].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.ScrumMaster
        $estimationResultItem.Estimation.Value | Should -Be 0
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[1].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[1].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -Be 0.5
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[2].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[2].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -Be 5.4861240687936887E+303

        $response = Get-Messages -Index 2 -AtLeast 2
        $response.Length | Should -Be 2
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[1].Name
        $response[1].Type | Should -Be $MessageTypes.EstimationEnded
        $response[1].EstimationResult | Should -Not -BeNullOrEmpty
        $response[1].EstimationResult.Length | Should -Be 3
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[0].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[0].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.ScrumMaster
        $estimationResultItem.Estimation.Value | Should -Be 0
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[1].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[1].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -Be 0.5
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[2].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[2].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -Be 5.4861240687936887E+303

        # Close the team

        Disconnect-Team -Index 1

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberDisconnected
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[1].Name

        $response = Get-Messages -Index 2
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberDisconnected
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[1].Name

        Disconnect-Team -Index 0
        Disconnect-Team -Index 2
    }

    It 'Should keep teams alive after container restart' {
        VerifyServiceIsHealthy -Index 0
        VerifyServiceIsHealthy -Index 1
        VerifyServiceIsHealthy -Index 2

        $response = Create-Team -Index 0
        $response.ScrumTeam.EstimationResult | Should -BeNullOrEmpty
        $response.ScrumTeam.EstimationParticipants | Should -BeNullOrEmpty

        $response = Join-Team -Index 1
        $response.ScrumTeam.State | Should -Be $TeamStates.Initial
        $response.ScrumTeam.Members.Length | Should -Be 2
        $member = $response.ScrumTeam.Members | Where-Object -Property Name -EQ -Value $sessions[0].Name
        $member | Should -Not -BeNullOrEmpty
        $member.Name | Should -Be $sessions[0].Name
        $member.Type | Should -Be $MemberTypes.ScrumMaster
        $member = $response.ScrumTeam.Members | Where-Object -Property Name -EQ -Value $sessions[1].Name
        $member | Should -Not -BeNullOrEmpty
        $member.Name | Should -Be $sessions[1].Name
        $member.Type | Should -Be $MemberTypes.Member
        $response.ScrumTeam.EstimationResult | Should -BeNullOrEmpty
        $response.ScrumTeam.EstimationParticipants | Should -BeNullOrEmpty

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberJoined
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[1].Name

        $response = Join-Team -Index 2
        $response.ScrumTeam.State | Should -Be $TeamStates.Initial
        $response.ScrumTeam.Members.Length | Should -Be 3
        $member = $response.ScrumTeam.Members | Where-Object -Property Name -EQ -Value $sessions[0].Name
        $member | Should -Not -BeNullOrEmpty
        $member.Name | Should -Be $sessions[0].Name
        $member.Type | Should -Be $MemberTypes.ScrumMaster
        $member = $response.ScrumTeam.Members | Where-Object -Property Name -EQ -Value $sessions[1].Name
        $member | Should -Not -BeNullOrEmpty
        $member.Name | Should -Be $sessions[1].Name
        $member.Type | Should -Be $MemberTypes.Member
        $member = $response.ScrumTeam.Members | Where-Object -Property Name -EQ -Value $sessions[2].Name
        $member | Should -Not -BeNullOrEmpty
        $member.Name | Should -Be $sessions[2].Name
        $member.Type | Should -Be $MemberTypes.Member
        $response.ScrumTeam.EstimationResult | Should -BeNullOrEmpty
        $response.ScrumTeam.EstimationParticipants | Should -BeNullOrEmpty

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberJoined
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[2].Name

        $response = Get-Messages -Index 1
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberJoined
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[2].Name

        Start-Estimation -Index 0

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.EstimationStarted

        $response = Get-Messages -Index 1
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.EstimationStarted

        $response = Get-Messages -Index 2
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.EstimationStarted

        Submit-Estimation -Index 0 -Estimation 2

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.ScrumMaster
        $response[0].Member.Name | Should -Be $sessions[0].Name

        $response = Get-Messages -Index 1
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.ScrumMaster
        $response[0].Member.Name | Should -Be $sessions[0].Name

        $response = Get-Messages -Index 2
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.ScrumMaster
        $response[0].Member.Name | Should -Be $sessions[0].Name

        Submit-Estimation -Index 2

        $response = Get-Messages -Index 0
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[2].Name

        $response = Get-Messages -Index 1
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[2].Name

        $response = Get-Messages -Index 2
        $response.Length | Should -Be 1
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[2].Name

        # Restart the service
        StopPlanningPokerService -Index 0
        StopPlanningPokerService -Index 2

        Start-Sleep -Seconds 3

        StartPlanningPokerService -Index 2
        StartPlanningPokerService -Index 0

        VerifyServiceIsHealthy -Index 0
        VerifyServiceIsHealthy -Index 1
        VerifyServiceIsHealthy -Index 2

        # Continue estimation
        Submit-Estimation -Index 1 -Estimation 1

        $response = Get-Messages -Index 0 -AtLeast 2
        $response.Length | Should -Be 2
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[1].Name
        $response[1].Type | Should -Be $MessageTypes.EstimationEnded
        $response[1].EstimationResult | Should -Not -BeNullOrEmpty
        $response[1].EstimationResult.Length | Should -Be 3
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[0].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[0].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.ScrumMaster
        $estimationResultItem.Estimation.Value | Should -Be 2
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[1].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[1].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -Be 1
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[2].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[2].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -BeNullOrEmpty

        $response = Get-Messages -Index 1 -AtLeast 2
        $response.Length | Should -Be 2
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[1].Name
        $response[1].Type | Should -Be $MessageTypes.EstimationEnded
        $response[1].EstimationResult | Should -Not -BeNullOrEmpty
        $response[1].EstimationResult.Length | Should -Be 3
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[0].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[0].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.ScrumMaster
        $estimationResultItem.Estimation.Value | Should -Be 2
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[1].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[1].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -Be 1
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[2].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[2].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -BeNullOrEmpty

        $response = Get-Messages -Index 2 -AtLeast 2
        $response.Length | Should -Be 2
        $response[0].Type | Should -Be $MessageTypes.MemberEstimated
        $response[0].Member.Type | Should -Be $MemberTypes.Member
        $response[0].Member.Name | Should -Be $sessions[1].Name
        $response[1].Type | Should -Be $MessageTypes.EstimationEnded
        $response[1].EstimationResult | Should -Not -BeNullOrEmpty
        $response[1].EstimationResult.Length | Should -Be 3
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[0].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[0].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.ScrumMaster
        $estimationResultItem.Estimation.Value | Should -Be 2
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[1].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[1].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -Be 1
        $estimationResultItem = $response[1].EstimationResult | Where-Object { $_.Member.Name -eq $sessions[2].Name }
        $estimationResultItem.Member.Name | Should -Be $sessions[2].Name
        $estimationResultItem.Member.Type | Should -Be $MemberTypes.Member
        $estimationResultItem.Estimation.Value | Should -BeNullOrEmpty
    }

    AfterEach {
        for ($i = 0; $i -lt $sessions.Length; $i++) {
            $session = $sessions[$i]
            if (![string]::IsNullOrEmpty($session.SessionId)) {
                Disconnect-Team -Index $i
            }
        }
    }
}
