<#
.SYNOPSIS
  Build, launch, and capture each A/B implementation arm.

.DESCRIPTION
  Runs every arm through the same sequence - launch, size the window to match
  the real Orbital capture, screenshot - so the resulting PNGs differ only by
  the arm's own markup. Records whether each arm survived to first frame; a
  crash on launch is a result, not an error to hide, so the script keeps going
  and reports it in the summary table.

.EXAMPLE
  .\run-arms.ps1 -OutDir .\parity
#>
[CmdletBinding()]
param(
    [string]$OutDir = "$PSScriptRoot\parity",
    [int]$Width = 1600,
    [int]$Height = 1000,
    [string[]]$Arms = @('A', 'B', 'C', 'B2')
)

$ErrorActionPreference = 'Stop'
$exe = Join-Path $PSScriptRoot 'ArmHost\ArmHost\bin\Release\net10.0-desktop\ArmHost.exe'
if (-not (Test-Path $exe)) { throw "ArmHost not built. Run: dotnet build ArmHost\ArmHost.csproj -f net10.0-desktop -c Release" }
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

if (-not ('DgkRun' -as [type])) {
    Add-Type -Name DgkRun -Namespace DgkArms -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool SetWindowPos(System.IntPtr hwnd, System.IntPtr after, int x, int y, int cx, int cy, uint flags);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool SetForegroundWindow(System.IntPtr hwnd);
'@
}

$results = foreach ($arm in $Arms) {
    Get-Process ArmHost -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 500

    $errFile = Join-Path $env:TEMP "arm-$arm.err.txt"
    $proc = Start-Process -FilePath $exe -ArgumentList $arm -PassThru -RedirectStandardError $errFile
    Start-Sleep -Seconds 8

    $live = Get-Process -Id $proc.Id -ErrorAction SilentlyContinue
    if (-not $live) {
        $firstLine = (Get-Content $errFile -ErrorAction SilentlyContinue | Select-Object -First 2) -join ' '
        [pscustomobject]@{ Arm = $arm; Started = $false; Capture = ''; Note = $firstLine }
        continue
    }

    [void][DgkArms.DgkRun]::SetWindowPos($live.MainWindowHandle, [IntPtr]::Zero, 60, 40, $Width, $Height, 0x14)
    [void][DgkArms.DgkRun]::SetForegroundWindow($live.MainWindowHandle)
    Start-Sleep -Seconds 3   # past the 0/100/200/300ms entrance stagger

    $png = Join-Path $OutDir ("arm-{0}.png" -f $arm)
    $cap = & (Join-Path $PSScriptRoot '..\..\tools\Capture-Window.ps1') -ProcessName ArmHost -OutFile $png -DelaySeconds 1

    [pscustomobject]@{ Arm = $arm; Started = $true; Capture = $cap.Method; Note = "uniformity=$($cap.Uniformity)" }
}

Get-Process ArmHost -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
$results | Format-Table -AutoSize
