# Fix test files with incorrect enum values and property names
$files = Get-ChildItem -Path "backend\tests" -Filter "*.cs" -Recurse
$count = 0
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    # Replace invalid Region enum values with valid ones
    $content = $content -replace 'Region\.Riyadh', 'Region.SaudiArabia'
    $content = $content -replace 'Region\.Makkah', 'Region.SaudiArabia'
    $content = $content -replace 'Region\.Madinah', 'Region.SaudiArabia'
    $content = $content -replace 'Region\.Jeddah', 'Region.SaudiArabia'
    $content = $content -replace 'Region\.Dammam', 'Region.SaudiArabia'
    $content = $content -replace 'Region\.Cairo', 'Region.Egypt'
    $content = $content -replace 'Region\.Alexandria', 'Region.Egypt'

    # Fix incorrect property names
    $content = $content -replace '\.AddressLine\s*=', '.FullAddress ='
    $content = $content -replace '\.ScheduledDateTime\s*=', '.ScheduledDate ='
    $content = $content -replace 'ServiceProvider.*\.Bio\s*=', 'ServiceProvider.Description ='
    $content = $content -replace '\.Bio\.Should\(\)', '.Description.Should()'

    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Fixed: $($file.Name)"
        $count++
    }
}
Write-Host "Total test files fixed: $count"
