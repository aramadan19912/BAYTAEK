$files = Get-ChildItem -Path "backend\src\HomeService.Application" -Filter "*.cs" -Recurse
$count = 0
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match 'using HomeService\.Application\.Common\.Models;') {
        $newContent = $content -replace 'using HomeService\.Application\.Common\.Models;', 'using HomeService.Application.Common;'
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        Write-Host "Fixed: $($file.Name)"
        $count++
    }
}
Write-Host "Total files fixed: $count"
