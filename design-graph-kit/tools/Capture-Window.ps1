<#
.SYNOPSIS
  Capture a window belonging to a running process to PNG.

.DESCRIPTION
  Uno Skia desktop windows are GL-composited, so a naive screen grab records
  whatever desktop pixels happen to be on top. PrintWindow with
  PW_RENDERFULLCONTENT is occlusion-proof and is the primary path here.

  Two traps this script guards against, both observed on net10.0-desktop:

  1. PrintWindow can return TRUE and still hand back a blank bitmap. A success
     return is not evidence of pixels, so the bitmap is sampled; if it is
     effectively uniform the script foregrounds the window and falls back to
     CopyFromScreen (not occlusion-proof, hence a fallback, and reported as
     such in the result object).
  2. Add-Type under .NET 10 / PowerShell 7 cannot resolve System.Drawing types
     from a -MemberDefinition source string (they sit behind type-forwards to
     System.Private.Windows.GdiPlus). So the C# string carries only Win32
     interop with fully-qualified attributes, and all Bitmap/Graphics work
     happens in PowerShell, where Add-Type -AssemblyName System.Drawing works.

.PARAMETER ProcessName
  Process whose MainWindowHandle is captured.

.PARAMETER OutFile
  Destination .png path.

.PARAMETER DelaySeconds
  Settle time before capture, to let the first frame render.

.EXAMPLE
  .\Capture-Window.ps1 -ProcessName Orbital -OutFile .\orbital.png -DelaySeconds 6
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ProcessName,
    [Parameter(Mandatory)][string]$OutFile,
    [int]$DelaySeconds = 5,
    [double]$UniformityThreshold = 0.995
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

if (-not ('DgkNative' -as [type])) {
    Add-Type -Name DgkNative -Namespace Dgk -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool PrintWindow(System.IntPtr hwnd, System.IntPtr hdc, uint flags);

[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool GetWindowRect(System.IntPtr hwnd, out RECT rect);

[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool SetForegroundWindow(System.IntPtr hwnd);

[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool ShowWindow(System.IntPtr hwnd, int cmd);

[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool IsWindowVisible(System.IntPtr hwnd);

public struct RECT { public int Left, Top, Right, Bottom; }
'@
}

if ($DelaySeconds -gt 0) { Start-Sleep -Seconds $DelaySeconds }

$proc = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue |
        Where-Object { $_.MainWindowHandle -ne 0 } |
        Select-Object -First 1

if (-not $proc) { throw "No process '$ProcessName' with a window handle. Debug builds on dev Uno.Sdk can keep the window hidden when launched without a devserver - try a Release build." }

$hwnd = $proc.MainWindowHandle
if (-not [Dgk.DgkNative]::IsWindowVisible($hwnd)) { throw "Window for '$ProcessName' exists but is not visible (hwnd $hwnd)." }

$rect = New-Object Dgk.DgkNative+RECT
[void][Dgk.DgkNative]::GetWindowRect($hwnd, [ref]$rect)
$w = $rect.Right - $rect.Left
$h = $rect.Bottom - $rect.Top
if ($w -le 0 -or $h -le 0) { throw "Window rect is empty ($w x $h)." }

# Measures the share of pixels taken by the single most common sampled colour.
# A genuine UI screenshot spreads across many colours; a failed PrintWindow is
# one flat colour, which is what this is here to catch.
function Get-Uniformity([System.Drawing.Bitmap]$bmp) {
    $counts = @{}
    $stepX = [Math]::Max(1, [int]($bmp.Width / 60))
    $stepY = [Math]::Max(1, [int]($bmp.Height / 60))
    $n = 0
    for ($x = 0; $x -lt $bmp.Width; $x += $stepX) {
        for ($y = 0; $y -lt $bmp.Height; $y += $stepY) {
            $key = $bmp.GetPixel($x, $y).ToArgb()
            $counts[$key] = 1 + ($counts[$key] ?? 0)
            $n++
        }
    }
    if ($n -eq 0) { return 1.0 }
    return (($counts.Values | Measure-Object -Maximum).Maximum / $n)
}

function Save-Bitmap([System.Drawing.Bitmap]$bmp, [string]$path) {
    $dir = Split-Path $path -Parent
    if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
}

$method = 'PrintWindow'
$bmp = New-Object System.Drawing.Bitmap($w, $h)
$gfx = [System.Drawing.Graphics]::FromImage($bmp)
$hdc = $gfx.GetHdc()
$ok = [Dgk.DgkNative]::PrintWindow($hwnd, $hdc, 2)   # PW_RENDERFULLCONTENT
$gfx.ReleaseHdc($hdc)
$gfx.Dispose()

$uniformity = Get-Uniformity $bmp

if (-not $ok -or $uniformity -ge $UniformityThreshold) {
    # PrintWindow reported success but produced a flat image, or outright failed.
    $bmp.Dispose()
    [void][Dgk.DgkNative]::ShowWindow($hwnd, 5)      # SW_SHOW
    [void][Dgk.DgkNative]::SetForegroundWindow($hwnd)
    Start-Sleep -Milliseconds 900

    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $gfx = [System.Drawing.Graphics]::FromImage($bmp)
    $gfx.CopyFromScreen($rect.Left, $rect.Top, 0, 0, (New-Object System.Drawing.Size($w, $h)))
    $gfx.Dispose()
    $method = 'CopyFromScreen (fallback; NOT occlusion-proof)'
    $uniformity = Get-Uniformity $bmp
}

Save-Bitmap $bmp $OutFile
$bmp.Dispose()

[pscustomobject]@{
    Process    = $ProcessName
    Hwnd       = $hwnd
    Size       = "${w}x${h}"
    Method     = $method
    Uniformity = [math]::Round($uniformity, 4)
    OutFile    = (Resolve-Path $OutFile).Path
}
