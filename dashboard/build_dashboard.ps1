# Tao dashboard HTML tu Quy_Tac_NC_Tool.md
# Dung: powershell -NoProfile -ExecutionPolicy Bypass -File .\dashboard\build_dashboard.ps1
$root = Split-Path $PSScriptRoot -Parent
$md = [IO.File]::ReadAllText((Join-Path $root 'Quy_Tac_NC_Tool.md'), [Text.Encoding]::UTF8)
$tpl = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'template.html'), [Text.Encoding]::UTF8)
# chuoi JS: thoat dau gach cheo, nhay kep, xuong dong; chen vao mau
$js = '"' + ($md.Replace('\','\\').Replace('"','\"').Replace("`r",'').Replace("`n",'\n').Replace('</','<\/')) + '"'
$out = $tpl.Replace('__MD__', $js)
$target = Join-Path $root 'Quy_Tac_NC_Tool_Dashboard.html'
[IO.File]::WriteAllText($target, $out, (New-Object Text.UTF8Encoding $false))
"Da tao: $target ($([int]((Get-Item $target).Length/1KB)) KB)"
