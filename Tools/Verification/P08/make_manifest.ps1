$baseDir = "E:\code\TLTD\review_package_p08_normal_wave_stat_growth"
$manifestFile = "$baseDir\MANIFEST_SHA256.txt"

Remove-Item -Force -ErrorAction SilentlyContinue $manifestFile

$files = Get-ChildItem -Path $baseDir -Recurse -File | Where-Object { $_.Name -ne "MANIFEST_SHA256.txt" }
$lines = @()

foreach ($f in $files) {
    $hash = (Get-FileHash -Path $f.FullName -Algorithm SHA256).Hash
    $rel = Resolve-Path -Relative $f.FullName
    $lines += "$hash  $rel"
}

$lines | Out-File -Encoding utf8 $manifestFile
Write-Host "Manifest written with $($lines.Count) files."
