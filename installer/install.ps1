# Cai dat DXF To NC Conversion Tool cho nguoi dung hien tai (khong can quyen quan tri)
# Co the doi thu muc bang bien moi truong de thu: NC_INSTALL_DIR, NC_SHORTCUT_DIR, NC_REGKEY
$ErrorActionPreference = 'Stop'
$src  = $PSScriptRoot
$name = 'DXF To NC Conversion Tool'
$dir  = if ($env:NC_INSTALL_DIR) { $env:NC_INSTALL_DIR } else { Join-Path $env:LOCALAPPDATA 'NC_Tool' }
$key  = if ($env:NC_REGKEY) { $env:NC_REGKEY } else { 'NC_Tool' }
$desk = if ($env:NC_SHORTCUT_DIR) { $env:NC_SHORTCUT_DIR } else { [Environment]::GetFolderPath('Desktop') }
$menu = if ($env:NC_SHORTCUT_DIR) { $env:NC_SHORTCUT_DIR } else { [Environment]::GetFolderPath('Programs') }
Add-Type -AssemblyName System.Windows.Forms

try {
    # dong ban dang chay de ghi de file
    Get-Process NC_Tool -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq (Join-Path $dir 'NC_Tool.exe') } | Stop-Process -Force
    Start-Sleep -Milliseconds 300

    New-Item -ItemType Directory -Force $dir | Out-Null
    foreach ($f in 'NC_Tool.exe', 'DXF_To_NC_v2.ico', 'uninstall.ps1') { Copy-Item (Join-Path $src $f) (Join-Path $dir $f) -Force }
    Set-Content (Join-Path $dir 'uninstall.cmd') "@echo off`r`npowershell -NoProfile -ExecutionPolicy Bypass -File `"%~dp0uninstall.ps1`"" -Encoding ASCII

    # shortcut Desktop va Start Menu
    $ws = New-Object -ComObject WScript.Shell
    foreach ($folder in @($desk, $menu) | Select-Object -Unique) {
        New-Item -ItemType Directory -Force $folder | Out-Null
        $l = $ws.CreateShortcut((Join-Path $folder "$name.lnk"))
        $l.TargetPath = Join-Path $dir 'NC_Tool.exe'; $l.WorkingDirectory = $dir
        $l.IconLocation = (Join-Path $dir 'NC_Tool.exe') + ',0'; $l.Description = $name; $l.Save()
    }
    if (-not $env:NC_SHORTCUT_DIR) { }

    # muc Add/Remove Programs (Settings > Apps)
    $rk = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\$key"
    New-Item -Path $rk -Force | Out-Null
    $size = [int]((Get-Item (Join-Path $dir 'NC_Tool.exe')).Length / 1KB)
    Set-ItemProperty $rk DisplayName $name
    Set-ItemProperty $rk DisplayVersion ((Get-Item (Join-Path $dir 'NC_Tool.exe')).LastWriteTime.ToString('yyyy.MM.dd'))
    Set-ItemProperty $rk Publisher 'NC Tool'
    Set-ItemProperty $rk InstallLocation $dir
    Set-ItemProperty $rk DisplayIcon ((Join-Path $dir 'NC_Tool.exe') + ',0')
    Set-ItemProperty $rk UninstallString ('"' + (Join-Path $dir 'uninstall.cmd') + '"')
    Set-ItemProperty $rk NoModify 1 -Type DWord
    Set-ItemProperty $rk NoRepair 1 -Type DWord
    Set-ItemProperty $rk EstimatedSize $size -Type DWord

    if (-not $env:NC_SILENT) {
        $r = [System.Windows.Forms.MessageBox]::Show("Đã cài đặt $name.`n`nThư mục: $dir`nShortcut: Desktop và Start Menu.`n`nMở phần mềm ngay bây giờ?", $name, 'YesNo', 'Information')
        if ($r -eq 'Yes') { Start-Process (Join-Path $dir 'NC_Tool.exe') }
    }
}
catch {
    [void][System.Windows.Forms.MessageBox]::Show("Cài đặt không thành công:`n$($_.Exception.Message)", $name, 'OK', 'Error')
    exit 1
}
