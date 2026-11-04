# Tao NC_Tool_Setup.exe bang IExpress (co san trong Windows)
# Dung: powershell -NoProfile -ExecutionPolicy Bypass -File .\installer\build_installer.ps1 [-OutDir "C:\duong\dan"]
param([string]$OutDir = 'C:\Users\ADMIN\OneDrive\Desktop\SetupToolNC')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$stage = Join-Path $env:TEMP 'nc_tool_installer_stage'
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory $stage | Out-Null
Copy-Item (Join-Path $root 'NC_Tool.exe') $stage
Copy-Item (Join-Path $root 'DXF_To_NC_v2.ico') $stage
foreach ($f in 'install.cmd', 'install.ps1', 'uninstall.ps1') { Copy-Item (Join-Path $PSScriptRoot $f) $stage }
New-Item -ItemType Directory -Force $OutDir | Out-Null
$target = Join-Path $OutDir 'NC_Tool_Setup.exe'
if (Test-Path $target) { Remove-Item $target -Force }

$sed = @"
[Version]
Class=IEXPRESS
SEDVersion=3
[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=0
HideExtractAnimation=1
UseLongFileName=1
InsideCompressed=0
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=N
InstallPrompt=%InstallPrompt%
DisplayLicense=%DisplayLicense%
FinishMessage=%FinishMessage%
TargetName=%TargetName%
FriendlyName=%FriendlyName%
AppLaunched=%AppLaunched%
PostInstallCmd=%PostInstallCmd%
AdminQuietInstCmd=%AdminQuietInstCmd%
UserQuietInstCmd=%UserQuietInstCmd%
SourceFiles=SourceFiles
[Strings]
InstallPrompt=
DisplayLicense=
FinishMessage=
TargetName=$target
FriendlyName=DXF To NC Conversion Tool - Setup
AppLaunched=install.cmd
PostInstallCmd=<None>
AdminQuietInstCmd=
UserQuietInstCmd=
FILE0="NC_Tool.exe"
FILE1="DXF_To_NC_v2.ico"
FILE2="install.cmd"
FILE3="install.ps1"
FILE4="uninstall.ps1"
[SourceFiles]
SourceFiles0=$stage\
[SourceFiles0]
%FILE0%=
%FILE1%=
%FILE2%=
%FILE3%=
%FILE4%=
"@
$sedPath = Join-Path $stage 'setup.sed'
Set-Content $sedPath $sed -Encoding ASCII
$p = Start-Process iexpress.exe -ArgumentList '/N', $sedPath -Wait -PassThru   # luu y: khong dung /Q va khong boc dau ngoac kep (IExpress se loi)
if (-not (Test-Path $target)) { throw "IExpress khong tao duoc $target (ma thoat $($p.ExitCode))" }
"Da tao: $target ($([int]((Get-Item $target).Length/1KB)) KB)"
