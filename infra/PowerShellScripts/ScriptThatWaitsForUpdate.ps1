$instanceLetter = "b"
$instance = "crpg01$instanceLetter"
$port = 7211

$Host.UI.RawUI.WindowTitle = "$instance"

$env:CRPG_SERVICE = "crpg-game-server"
$env:CRPG_INSTANCE = $instance
$serverPath = "$env:mb_server_path\bin\Win64_Shipping_Server"
# Load the XML content from a file


function IsLatest {
# Fetch current latest Submodule

# Create a new WebClient object
$webClient = New-Object System.Net.WebClient

# Download the XML content
$responseContent = $webClient.DownloadString("https://www.c-rpg.eu/SubModule.xml")

# Parse the XML content
try {
    [xml]$latestSubModule = $responseContent
}
catch {
    Write-Host "Error encountered parsing the xml $_"
    return $true
}
# Fetch the version value
$currentVersion = $currentSubmodule.Module.Version.value
$newVersion = $latestSubModule.Module.Version.value
if ($currentVersion -eq $newVersion)
{
    return "no"
}
else
{
    return "Current Version is $currentVersion and new Version is $newVersion"
}
}


while ($true)
{
    $Process = Start-Process -WorkingDirectory "$serverPath" `
        -FilePath "DedicatedCustomServer.Starter.exe" `
        -ArgumentList "_MODULES_*Native*Multiplayer*cRPG*_MODULES_","/dedicatedcustomserverconfigfile","..\cRPG\$instanceLetter.txt","/DisableErrorReporting","/port $port" `
        -PassThru

    # Periodically check for new version while the process is running
    do {
        Start-Sleep -Seconds 2
        [xml]$currentSubmodule = Get-Content "$env:mb_server_path\Modules\cRPG\SubModule.xml"
        $needToUpdate = IsLatest
        if (-not ($needToUpdate -eq "no")) {
            $Process.Kill()
            Write-Host "New version detected! Stopping the current process..."
            Write-Host "$needToUpdate"
            break
        }
    } while (!$Process.HasExited)
    do {
        Start-Sleep -Seconds 30
        [xml]$currentSubmodule = Get-Content "$env:mb_server_path\Modules\cRPG\SubModule.xml"
        $needToUpdate = IsLatest
        if (($needToUpdate -eq "no")) {
            $Process.Kill()
            Write-Host "Update is probably finished"
            break
        }
        Write-Host "Update is still ongoing"
    } while (!$Process.HasExited)
    Write-Host "$(Get-Date): process stopped with exit code $($Process.ExitCode)"

    $LogFolder = "C:\ProgramData\Mount and Blade II Bannerlord\logs"
    $FileName = "$($instance)-$mode-$(Get-Date -uformat %Y-%m-%dT%H.%M.%S)"
    Copy-Item -Path "$LogFolder\rgl_log_$($Process.Id).txt" -Destination "$LogFolder\$($FileName).txt"
    Copy-Item -Path "$LogFolder\rgl_log_errors_$($Process.Id).txt" -Destination "$LogFolder\$($FileName)-errors.txt"
    Start-Sleep -Seconds 5
}

