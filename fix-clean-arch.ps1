$files = Get-ChildItem -Path "backend\src\HomeService.Application" -Filter "*.cs" -Recurse
$count = 0
foreach ($file in $files) {
    $lines = Get-Content $file.FullName
    $newLines = @()
    $changed = $false

    foreach ($line in $lines) {
        # Skip lines that violate clean architecture
        if ($line -match '^using Microsoft\.EntityFrameworkCore' -or
            $line -match '^using HomeService\.Infrastructure' -or
            $line -match '^using Microsoft\.Extensions\.Configuration;') {
            $changed = $true
            continue
        }
        $newLines += $line
    }

    if ($changed) {
        $newContent = $newLines -join "`r`n"
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Fixed: $($file.Name)"
        $count++
    }
}
Write-Host "Total files fixed: $count"
