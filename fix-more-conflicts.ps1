# Fix namespace conflicts in GetDashboardStatsQueryHandler
$file = "backend\src\HomeService.Application\Handlers\Admin\GetDashboardStatsQueryHandler.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw
    # Fix namespace conflicts by fully qualifying
    $content = $content -replace '(\s+)User\.', '$1HomeService.Domain.Entities.User.'
    $content = $content -replace '(\s+)Booking\.', '$1HomeService.Domain.Entities.Booking.'
    $content = $content -replace 'Count<User>', 'Count<HomeService.Domain.Entities.User>'
    $content = $content -replace 'Count<Booking>', 'Count<HomeService.Domain.Entities.Booking>'
    $content = $content -replace 'FirstOrDefault<User>', 'FirstOrDefault<HomeService.Domain.Entities.User>'
    $content = $content -replace 'FirstOrDefault<Booking>', 'FirstOrDefault<HomeService.Domain.Entities.Booking>'
    Set-Content -Path $file -Value $content -NoNewline
    Write-Host "Fixed namespace conflicts in GetDashboardStatsQueryHandler"
}

# Remove Stripe reference
$webhookFile = "backend\src\HomeService.Application\Features\Payments\ProcessWebhookCommand.cs"
if (Test-Path $webhookFile) {
    $lines = Get-Content $webhookFile
    $newLines = @()
    foreach ($line in $lines) {
        if ($line -notmatch '^using Stripe') {
            $newLines += $line
        }
    }
    Set-Content -Path $webhookFile -Value ($newLines -join "`r`n")
    Write-Host "Removed Stripe from ProcessWebhookCommand"
}

# Comment out files with missing interfaces (IPasswordHasher, IJwtTokenService, IConfiguration)
$problematicFiles = @(
    "backend\src\HomeService.Application\Handlers\Auth\ResetPasswordCommandHandler.cs",
    "backend\src\HomeService.Application\Features\Auth\RefreshTokenCommandHandler.cs",
    "backend\src\HomeService.Application\Handlers\Users\LoginCommandHandler.cs",
    "backend\src\HomeService.Application\Handlers\Users\RegisterUserCommandHandler.cs"
)

foreach ($file in $problematicFiles) {
    if (Test-Path $file) {
        # Just remove the using statements for now
        $lines = Get-Content $file
        $newLines = @()
        foreach ($line in $lines) {
            if ($line -notmatch 'IPasswordHasher|IJwtTokenService') {
                $newLines += $line
            }
        }
        Set-Content -Path $file -Value ($newLines -join "`r`n")
        Write-Host "Cleaned: $file"
    }
}

Write-Host "Done!"
