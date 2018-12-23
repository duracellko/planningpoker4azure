Param (
    [string] $VersionNumber
)

if ($VersionNumber -ne "") {
    $scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
    $sharedProjectPath = Join-Path $ScriptDir "..\src\Duracellko.PlanningPoker.Shared\Duracellko.PlanningPoker.Shared.projitems"

    $document = [System.Xml.XmlDocument]::new()
    $document.Load($sharedProjectPath) | Out-Null
    $namespaceManager = [System.Xml.XmlNamespaceManager]::new($document.NameTable)
    $namespaceManager.AddNamespace('sdk', 'http://schemas.microsoft.com/developer/msbuild/2003')
    $pattern = "(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)\.(?<rev>\d+)"
    $replace = "`${major}.`${minor}.$VersionNumber.`${rev}"

    $assemblyVersionXPath = '/sdk:Project/sdk:PropertyGroup/sdk:AssemblyVersion'
    $versionElement = $document.SelectSingleNode($assemblyVersionXPath, $namespaceManager)
    $versionText = $versionElement.InnerText
    $versionText = $versionText -replace $pattern, $replace
    $versionElement.InnerText = $versionText
    Write-Host "AssemblyVersion changed: $versionText"

    $fileVersionXPath = '/sdk:Project/sdk:PropertyGroup/sdk:FileVersion'
    $versionElement = $document.SelectSingleNode($fileVersionXPath, $namespaceManager)
    $versionText = $versionElement.InnerText
    $versionText = $versionText -replace $pattern, $replace
    $versionElement.InnerText = $versionText
    Write-Host "FileVersion changed: $versionText"

    $document.Save($sharedProjectPath) | Out-Null
}
