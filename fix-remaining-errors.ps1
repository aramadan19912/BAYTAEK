# Fix all remaining namespace conflicts in Admin handlers
$adminFiles = @(
    "backend\src\HomeService.Application\Handlers\Admin\GetDashboardStatsQueryHandler.cs",
    "backend\src\HomeService.Application\Handlers\Admin\GetFinancialAnalyticsQueryHandler.cs"
)

foreach ($filePath in $adminFiles) {
    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw

        # Fix method signatures and generic parameters
        $content = $content -replace 'IEnumerable<User>', 'IEnumerable<HomeService.Domain.Entities.User>'
        $content = $content -replace 'IEnumerable<Booking>', 'IEnumerable<HomeService.Domain.Entities.Booking>'
        $content = $content -replace 'List<User>', 'List<HomeService.Domain.Entities.User>'
        $content = $content -replace 'List<Booking>', 'List<HomeService.Domain.Entities.Booking>'

        # Fix variable usage
        $content = $content -replace '([^\.])\bUser\s+', '$1HomeService.Domain.Entities.User '
        $content = $content -replace '([^\.])\bBooking\s+', '$1HomeService.Domain.Entities.Booking '

        Set-Content -Path $filePath -Value $content -NoNewline
        Write-Host "Fixed: $filePath"
    }
}

# Fix RefreshTokenCommandHandler - remove IConfiguration usage
$refreshTokenFile = "backend\src\HomeService.Application\Features\Auth\RefreshTokenCommandHandler.cs"
if (Test-Path $refreshTokenFile) {
    $lines = Get-Content $refreshTokenFile
    $newLines = @()
    $skipNextConstructorParam = $false

    foreach ($line in $lines) {
        # Skip IConfiguration field, parameter, assignment
        if ($line -match 'IConfiguration|_configuration') {
            continue
        }
        $newLines += $line
    }

    Set-Content -Path $refreshTokenFile -Value ($newLines -join "`r`n")
    Write-Host "Fixed RefreshTokenCommandHandler"
}

Write-Host "Done fixing remaining errors!"
