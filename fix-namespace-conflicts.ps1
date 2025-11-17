$files = Get-ChildItem -Path "backend\src\HomeService.Application\Handlers" -Filter "*.cs" -Recurse
$count = 0

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    # Fix namespace conflicts by fully qualifying entity types
    # Only replace if it's in generic type parameter position
    $content = $content -replace 'IRepository<Booking>', 'IRepository<HomeService.Domain.Entities.Booking>'
    $content = $content -replace 'IRepository<User>', 'IRepository<HomeService.Domain.Entities.User>'
    $content = $content -replace 'IRepository<Service>', 'IRepository<HomeService.Domain.Entities.Service>'
    $content = $content -replace 'IRepository<Category>', 'IRepository<HomeService.Domain.Entities.Category>'
    $content = $content -replace 'IRepository<Review>', 'IRepository<HomeService.Domain.Entities.Review>'
    $content = $content -replace 'IRepository<Payment>', 'IRepository<HomeService.Domain.Entities.Payment>'
    $content = $content -replace 'IRepository<PromoCode>', 'IRepository<HomeService.Domain.Entities.PromoCode>'
    $content = $content -replace 'IRepository<Notification>', 'IRepository<HomeService.Domain.Entities.Notification>'

    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Fixed: $($file.Name)"
        $count++
    }
}

Write-Host "Total handler files fixed: $count"
