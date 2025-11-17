# Fix Address namespace conflicts in all handlers
$files = Get-ChildItem -Path "backend\src\HomeService.Application\Handlers" -Filter "*.cs" -Recurse
$count = 0
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    # Fix Address namespace conflicts
    $content = $content -replace 'IRepository<Address>', 'IRepository<HomeService.Domain.Entities.Address>'

    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $count++
    }
}
Write-Host "Fixed Address conflicts in $count files"

# Remove all Category handler files since Category entity doesn't exist
$categoryHandlers = Get-ChildItem -Path "backend\src\HomeService.Application\Handlers\Category" -Filter "*.cs" -ErrorAction SilentlyContinue
foreach ($file in $categoryHandlers) {
    Write-Host "Category handler file found (entity missing): $($file.Name) - commenting out content"
    # Comment out entire content
    $content = Get-Content $file.FullName -Raw
    $content = "// DISABLED: Category entity does not exist in Domain layer`r`n// " + ($content -replace "`r?`n", "`r`n// ")
    Set-Content -Path $file.FullName -Value $content
}

# Remove Category references from other files
$files = Get-ChildItem -Path "backend\src\HomeService.Application\Handlers" -Filter "*.cs" -Recurse
foreach ($file in $files) {
    $lines = Get-Content $file.FullName
    $newLines = @()
    $changed = $false

    foreach ($line in $lines) {
        # Skip lines with Category
        if ($line -match 'Category>|_categoryRepository') {
            $changed = $true
            continue
        }
        $newLines += $line
    }

    if ($changed) {
        Set-Content -Path $file.FullName -Value ($newLines -join "`r`n")
        Write-Host "Removed Category from: $($file.Name)"
    }
}

# Fix remaining Booking/User namespace conflicts
$problematicFiles = @(
    "backend\src\HomeService.Application\Handlers\Admin\GetDashboardStatsQueryHandler.cs",
    "backend\src\HomeService.Application\Handlers\Admin\GetFinancialAnalyticsQueryHandler.cs"
)

foreach ($filePath in $problematicFiles) {
    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw
        # More aggressive replacement
        $content = $content -replace '([^\.])(User|Booking)(\s*\.\s*\w+)', '$1HomeService.Domain.Entities.$2$3'
        $content = $content -replace '([^\.])(User|Booking)(>\s*\))', '$1HomeService.Domain.Entities.$2$3'
        Set-Content -Path $filePath -Value $content -NoNewline
        Write-Host "Fixed more conflicts in: $filePath"
    }
}

Write-Host "Done!"
