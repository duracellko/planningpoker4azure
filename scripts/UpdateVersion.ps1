Param (
    [string] $VersionNumber
)

if ($VersionNumber -ne "") {
    $scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
    $buildPropsPath = Join-Path $ScriptDir "..\Directory.Build.props"

    $document = [System.Xml.XmlDocument]::new()
    $document.Load($buildPropsPath) | Out-Null
    $pattern = "(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)\.(?<rev>\d+)"
    $replace = "`${major}.`${minor}.$VersionNumber.`${rev}"

    $assemblyVersionXPath = '/Project/PropertyGroup/AssemblyVersion'
    $versionElement = $document.SelectSingleNode($assemblyVersionXPath)
    $versionText = $versionElement.InnerText
    $versionText = $versionText -replace $pattern, $replace
    $versionElement.InnerText = $versionText
    Write-Host "AssemblyVersion changed: $versionText"

    $fileVersionXPath = '/Project/PropertyGroup/FileVersion'
    $versionElement = $document.SelectSingleNode($fileVersionXPath)
    $versionText = $versionElement.InnerText
    $versionText = $versionText -replace $pattern, $replace
    $versionElement.InnerText = $versionText
    Write-Host "FileVersion changed: $versionText"

    $document.Save($buildPropsPath) | Out-Null
}
