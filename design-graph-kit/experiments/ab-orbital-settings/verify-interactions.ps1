<#
.SYNOPSIS
  Drive the two transient behaviors of an arm and capture proof they render.

.DESCRIPTION
  Compiling and launching an arm proves its markup is sound. It does not prove
  the behaviors the Design Graph declared actually fire. Two are transient and
  can only be seen by driving them:

    state.profile.saved       - Save button content flips to "Saved!" for 1.5 s
    dialog.recents-cleared    - Clear Recent Projects opens a ContentDialog

  Timing matters for the first: the flash reverts after 1500 ms, so the capture
  is taken immediately after the click with no settle delay. A capture that
  misses the window looks identical to a behavior that never fired, so the
  script also captures the reverted state afterwards - seeing "Save" return is
  what distinguishes "flash worked" from "capture was late".

  Pointer moves are injected through the input stream (mouse_event with
  MOUSEEVENTF_ABSOLUTE); SetCursorPos moves are invisible to Uno Skia desktop.

  Two traps this script exists to avoid, both cost real debugging time:

  1. SetForegroundWindow FAILS (returns false) when the calling process does
     not own the foreground - which is the normal case when driving an app
     from a script. The arm then stays *behind* whatever window is on top, and
     synthesized clicks land on that other window instead. PrintWindow is
     occlusion-proof, so the captures still look perfectly correct while
     nothing is being clicked. Raise the window in z-order with
     SetWindowPos(HWND_TOPMOST) instead, which needs no foreground rights.
  2. A capture is only meaningful if the window is the size you think it is.
     Assert the rect after positioning, and compare file hashes across steps:
     four identical hashes mean nothing happened, however plausible the
     screenshots look individually.

.EXAMPLE
  .\verify-interactions.ps1 -Arm B
#>
[CmdletBinding()]
param(
    [ValidateSet('A', 'B', 'C', 'B2')][string]$Arm = 'B',
    [string]$OutDir = "$PSScriptRoot\parity\interactions",
    [int]$WinX = 60, [int]$WinY = 40, [int]$Width = 1600, [int]$Height = 1000,
    # Client-relative hit points, read off the 1600x1000 arm captures.
    [int]$SaveX = 1512, [int]$SaveY = 251,
    [int]$ClearX = 925, [int]$ClearY = 623
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$exe = Join-Path $PSScriptRoot 'ArmHost\ArmHost\bin\Release\net10.0-desktop\ArmHost.exe'
if (-not (Test-Path $exe)) { throw "ArmHost not built." }
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

if (-not ('DgkIx' -as [type])) {
    Add-Type -Name DgkIx -Namespace DgkInteract -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern void mouse_event(uint flags, int dx, int dy, uint data, System.IntPtr extra);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool SetWindowPos(System.IntPtr hwnd, System.IntPtr after, int x, int y, int cx, int cy, uint flags);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool GetWindowRect(System.IntPtr hwnd, out RECT r);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern System.IntPtr WindowFromPoint(POINT p);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern int GetWindowThreadProcessId(System.IntPtr hwnd, out int pid);
public struct RECT { public int Left, Top, Right, Bottom; }
public struct POINT { public int X, Y; }
'@
}

function Invoke-ClientClick([int]$cx, [int]$cy) {
    $sx = $WinX + $cx
    $sy = $WinY + $cy
    $sw = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Width
    $sh = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Height
    $ax = [int]($sx * 65535 / $sw); $ay = [int]($sy * 65535 / $sh)
    [DgkInteract.DgkIx]::mouse_event(0x8001, $ax, $ay, 0, [IntPtr]::Zero); Start-Sleep -Milliseconds 200
    [DgkInteract.DgkIx]::mouse_event(0x8001, $ax, $ay, 0, [IntPtr]::Zero); Start-Sleep -Milliseconds 200
    [DgkInteract.DgkIx]::mouse_event(0x0002, 0, 0, 0, [IntPtr]::Zero)
    Start-Sleep -Milliseconds 100
    [DgkInteract.DgkIx]::mouse_event(0x0004, 0, 0, 0, [IntPtr]::Zero)
}

Get-Process ArmHost -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500

$proc = Start-Process -FilePath $exe -ArgumentList $Arm -PassThru
Start-Sleep -Seconds 8
$live = Get-Process -Id $proc.Id -ErrorAction SilentlyContinue
if (-not $live) { throw "Arm $Arm exited before showing a window." }

$hwnd = $live.MainWindowHandle
# HWND_TOPMOST (-1) + SWP_SHOWWINDOW: raises the arm above other windows
# without requiring foreground rights this process does not have.
[void][DgkInteract.DgkIx]::SetWindowPos($hwnd, [IntPtr](-1), $WinX, $WinY, $Width, $Height, 0x40)
Start-Sleep -Seconds 3   # past the entrance stagger

$rect = New-Object DgkInteract.DgkIx+RECT
[void][DgkInteract.DgkIx]::GetWindowRect($hwnd, [ref]$rect)
$actual = "$($rect.Right - $rect.Left)x$($rect.Bottom - $rect.Top)"
if ($actual -ne "${Width}x${Height}") { throw "Window is $actual, expected ${Width}x${Height}; hit coordinates would be meaningless." }

# Prove the pointer will actually land on this arm before clicking anything.
$probe = New-Object DgkInteract.DgkIx+POINT
$probe.X = $WinX + $SaveX; $probe.Y = $WinY + $SaveY
$under = [DgkInteract.DgkIx]::WindowFromPoint($probe)
$ownerPid = 0
[void][DgkInteract.DgkIx]::GetWindowThreadProcessId($under, [ref]$ownerPid)
if ($ownerPid -ne $live.Id) { throw "Point ($($probe.X),$($probe.Y)) belongs to pid $ownerPid, not the arm (pid $($live.Id)). Another window is covering it." }

$cap = Join-Path $PSScriptRoot '..\..\tools\Capture-Window.ps1'

# --- state.profile.saved -------------------------------------------------
Invoke-ClientClick $SaveX $SaveY
& $cap -ProcessName ArmHost -OutFile (Join-Path $OutDir "$Arm-1-save-flash.png") -DelaySeconds 0 | Out-Null
Start-Sleep -Seconds 3   # past the 1500 ms revert
& $cap -ProcessName ArmHost -OutFile (Join-Path $OutDir "$Arm-2-save-reverted.png") -DelaySeconds 0 | Out-Null

# --- dialog.recents-cleared ----------------------------------------------
Invoke-ClientClick $ClearX $ClearY
Start-Sleep -Seconds 2
& $cap -ProcessName ArmHost -OutFile (Join-Path $OutDir "$Arm-3-cleared-dialog.png") -DelaySeconds 0 | Out-Null

Get-Process ArmHost -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# Distinct hashes are the evidence that anything happened at all.
$shots = Get-ChildItem $OutDir -Filter "$Arm-*.png" | Sort-Object Name
$shots | ForEach-Object {
    "  {0,-28} {1,8:N0} bytes  {2}" -f $_.Name, $_.Length, (Get-FileHash $_.FullName).Hash.Substring(0, 12)
}
if (($shots | ForEach-Object { (Get-FileHash $_.FullName).Hash } | Select-Object -Unique).Count -lt $shots.Count) {
    Write-Warning "Two or more captures are byte-identical: an interaction did not register, or a capture was mistimed."
}
