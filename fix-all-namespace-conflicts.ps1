# Comprehensive fix for all namespace conflicts
$providerFiles = @(
    "backend\src\HomeService.Application\Handlers\Provider\GetProviderDashboardQueryHandler.cs",
    "backend\src\HomeService.Application\Handlers\Provider\GetProviderEarningsQueryHandler.cs"
)

foreach ($filePath in $providerFiles) {
    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw

        # Fix all entity namespace conflicts comprehensively
        $content = $content -replace 'IEnumerable<Booking>', 'IEnumerable<HomeService.Domain.Entities.Booking>'
        $content = $content -replace 'IEnumerable<User>', 'IEnumerable<HomeService.Domain.Entities.User>'
        $content = $content -replace 'IEnumerable<Service>', 'IEnumerable<HomeService.Domain.Entities.Service>'
        $content = $content -replace 'IEnumerable<Review>', 'IEnumerable<HomeService.Domain.Entities.Review>'

        $content = $content -replace 'List<Booking>', 'List<HomeService.Domain.Entities.Booking>'
        $content = $content -replace 'List<User>', 'List<HomeService.Domain.Entities.User>'
        $content = $content -replace 'List<Service>', 'List<HomeService.Domain.Entities.Service>'
        $content = $content -replace 'List<Review>', 'List<HomeService.Domain.Entities.Review>'

        # Fix when used as standalone types
        $content = $content -replace '([^\w])Booking([^\w])', '$1HomeService.Domain.Entities.Booking$2'
        $content = $content -replace '([^\w])User([^\w])', '$1HomeService.Domain.Entities.User$2'
        $content = $content -replace '([^\w])Service([^\w])', '$1HomeService.Domain.Entities.Service$2'
        $content = $content -replace '([^\w])Review([^\w])', '$1HomeService.Domain.Entities.Review$2'

        # Fix double replacements
        $content = $content -replace 'HomeService\.Domain\.Entities\.HomeService\.Domain\.Entities\.', 'HomeService.Domain.Entities.'

        Set-Content -Path $filePath -Value $content -NoNewline
        Write-Host "Fixed: $filePath"
    }
}

# Fix duplicate UserDto
$registerFile = "backend\src\HomeService.Application\Handlers\Users\RegisterUserCommandHandler.cs"
if (Test-Path $registerFile) {
    $content = Get-Content $registerFile -Raw
    # Use fully qualified name
    $content = $content -replace '(?<!\.DTOs\.)UserDto(?!;)', 'HomeService.Application.DTOs.UserDto'
    $content = $content -replace 'HomeService\.Application\.DTOs\.HomeService\.Application\.DTOs\.', 'HomeService.Application.DTOs.'
    Set-Content -Path $registerFile -Value $content -NoNewline
    Write-Host "Fixed RegisterUserCommandHandler UserDto ambiguity"
}

Write-Host "Done!"
