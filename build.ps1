# Bien dich NC_Tool.exe, in cac thay doi ma nguon (so voi lan build truoc) ra Terminal
# va tu dong ghi nhat ky thay doi vao lich_su\dd-MM-yyyy\nhat_ky_thay_doi.md
$d = $PSScriptRoot
$src = Join-Path $d 'src'
$snap = Join-Path $src '.last'
New-Item -ItemType Directory -Force $snap | Out-Null
$log = New-Object System.Collections.Generic.List[string]

Write-Host '=== THAY DOI MA NGUON ===' -ForegroundColor Cyan
$changed = $false
foreach ($f in Get-ChildItem $src -Filter *.cs) {
    $old = Join-Path $snap $f.Name
    if (-not (Test-Path $old)) { Write-Host ("[moi] " + $f.Name) -ForegroundColor Yellow; $log.Add("[moi] " + $f.Name); Copy-Item $f.FullName $old; continue }
    $a = Get-Content $old -Encoding UTF8
    $b = Get-Content $f.FullName -Encoding UTF8
    $diff = Compare-Object $a $b
    if (-not $diff) { continue }
    $changed = $true
    Write-Host ("--- " + $f.Name) -ForegroundColor Cyan
    $log.Add("--- " + $f.Name)
    foreach ($x in $diff) {
        if ($x.SideIndicator -eq '=>') { Write-Host ("+ " + $x.InputObject) -ForegroundColor Green; $log.Add("+ " + $x.InputObject) }
        else { Write-Host ("- " + $x.InputObject) -ForegroundColor Red; $log.Add("- " + $x.InputObject) }
    }
}
if (-not $changed) { Write-Host '(khong co thay doi so voi lan build truoc)' -ForegroundColor DarkGray }

Write-Host ''
Write-Host '=== BIEN DICH ===' -ForegroundColor Cyan
Get-Process NC_Tool -ErrorAction SilentlyContinue | Stop-Process -Force
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $csc /nologo /codepage:65001 /target:winexe /optimize+ /out:"$d\NC_Tool.exe" /win32icon:"$src\app.ico" /resource:"$src\xlsx_icon.png,NCTool.xlsx_icon.png" `
    /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Core.dll /r:System.IO.Compression.dll `
    /r:System.IO.Compression.FileSystem.dll /r:System.Xml.Linq.dll /r:System.Xml.dll `
    (Get-ChildItem $src -Filter *.cs | ForEach-Object { $_.FullName })
$ok = ($LASTEXITCODE -eq 0)
if ($ok) {
    Get-ChildItem $src -Filter *.cs | ForEach-Object { Copy-Item $_.FullName (Join-Path $snap $_.Name) -Force }
    Write-Host 'OK: NC_Tool.exe da duoc tao lai.' -ForegroundColor Green
} else {
    Write-Host 'LOI bien dich - xem thong bao o tren.' -ForegroundColor Red
}

# Tu dong ghi nhat ky vao thu muc cua ngay hom nay (chi ghi khi co thay doi hoac loi)
if ($changed -or $log.Count -gt 0 -or -not $ok) {
    $day = Get-Date -Format 'dd-MM-yyyy'
    $dir = Join-Path $d "lich_su\$day"
    New-Item -ItemType Directory -Force $dir | Out-Null
    $file = Join-Path $dir 'nhat_ky_thay_doi.md'
    if (-not (Test-Path $file)) { Set-Content $file "# Nhat ky thay doi ma nguon ngay $day`r`n" -Encoding UTF8 }
    $status = if ($ok) { 'bien dich thanh cong' } else { 'LOI bien dich' }
    $lines = @('', ("## " + (Get-Date -Format 'HH:mm') + " - " + $status), '```diff') + ($log | Select-Object -First 300) + @('```')
    Add-Content $file $lines -Encoding UTF8
}
