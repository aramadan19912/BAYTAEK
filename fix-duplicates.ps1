# Fix duplicate BookingDto in command files
$bookingCommandFiles = @(
    "backend\src\HomeService.Application\Commands\Booking\AcceptBookingCommand.cs",
    "backend\src\HomeService.Application\Features\Bookings\AcceptBookingCommand.cs"
)

foreach ($file in $bookingCommandFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        # Remove BookingDto class definition
        $content = $content -replace '(?s)\r?\npublic class BookingDto\s*\{[^}]*\}[^}]*$', ''
        # Add using DTOs if not present
        if ($content -notmatch 'using HomeService\.Application\.DTOs;') {
            $content = $content -replace '(using HomeService\.Application\.Common;)', "`$1`r`nusing HomeService.Application.DTOs;"
        }
        Set-Content -Path $file -Value $content -NoNewline
        Write-Host "Fixed BookingDto in: $file"
    }
}

# Remove Category references (entity doesn't exist)
$files = Get-ChildItem -Path "backend\src\HomeService.Application\Handlers" -Filter "*Admin*.cs" -Recurse
foreach ($file in $files) {
    $lines = Get-Content $file.FullName
    $newLines = @()
    $changed = $false

    foreach ($line in $lines) {
        # Skip lines with Category repository
        if ($line -match 'IRepository<.*Category>|_categoryRepository') {
            $changed = $true
            continue
        }
        $newLines += $line
    }

    if ($changed) {
        $newContent = $newLines -join "`r`n"
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Removed Category from: $($file.Name)"
    }
}

Write-Host "Done!"
