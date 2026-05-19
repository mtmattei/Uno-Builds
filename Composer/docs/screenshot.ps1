Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$p = Get-Process Composer -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $p) { Write-Host 'No Composer process'; exit 1 }

Add-Type @"
using System;
using System.Runtime.InteropServices;
public class Win {
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int n);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L,T,R,B; }
}
"@

[Win]::ShowWindow($p.MainWindowHandle, 9) | Out-Null
[Win]::SetForegroundWindow($p.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 1200

$r = New-Object Win+RECT
[Win]::GetWindowRect($p.MainWindowHandle, [ref]$r) | Out-Null
$w = $r.R - $r.L
$h = $r.B - $r.T
Write-Host "rect L=$($r.L) T=$($r.T) W=$w H=$h"

$bmp = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size $w, $h))
$out = 'C:\Users\Platform006\OneDrive - Uno Platform\Desktop\unOS\AI-builds\Composer\docs\composer-screenshot.png'
$bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose()
$bmp.Dispose()
Write-Host "saved $out"
