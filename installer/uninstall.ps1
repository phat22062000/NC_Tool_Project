# Go cai dat DXF To NC Conversion Tool (giu lai file cai dat nguoi dung %APPDATA%\NC_Tool.ini)
$name = 'DXF To NC Conversion Tool'
$dir  = $PSScriptRoot
$key  = if ($env:NC_REGKEY) { $env:NC_REGKEY } else { 'NC_Tool' }
$desk = if ($env:NC_SHORTCUT_DIR) { $env:NC_SHORTCUT_DIR } else { [Environment]::GetFolderPath('Desktop') }
$menu = if ($env:NC_SHORTCUT_DIR) { $env:NC_SHORTCUT_DIR } else { [Environment]::GetFolderPath('Programs') }
Add-Type -AssemblyName System.Windows.Forms

if (-not $env:NC_SILENT) {
    $r = [System.Windows.Forms.MessageBox]::Show("Gỡ cài đặt $name khỏi máy này?", $name, 'YesNo', 'Question')
    if ($r -ne 'Yes') { exit 0 }
}
Get-Process NC_Tool -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq (Join-Path $dir 'NC_Tool.exe') } | Stop-Process -Force
foreach ($folder in @($desk, $menu) | Select-Object -Unique) { Remove-Item (Join-Path $folder "$name.lnk") -Force -ErrorAction SilentlyContinue }
Remove-Item "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\$key" -Recurse -Force -ErrorAction SilentlyContinue
# xoa thu muc cai dat sau khi script nay ket thuc
Start-Process cmd.exe -ArgumentList '/c', "ping -n 3 127.0.0.1 >nul & rmdir /s /q `"$dir`"" -WindowStyle Hidden
if (-not $env:NC_SILENT) { [void][System.Windows.Forms.MessageBox]::Show("Đã gỡ cài đặt $name.", $name, 'OK', 'Information') }
