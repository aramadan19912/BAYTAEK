$files = Get-ChildItem -Path "backend" -Filter "*.csproj" -Recurse
$count = 0
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    # Fix AutoMapper version
    $content = $content -replace 'AutoMapper\.Extensions\.Microsoft\.DependencyInjection" Version="13\.0\.1"', 'AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1"'
    $content = $content -replace 'AutoMapper" Version="13\.0\.1"', 'AutoMapper" Version="12.0.1"'

    # Fix AspNetCore.HealthChecks.AzureStorage version
    $content = $content -replace 'AspNetCore\.HealthChecks\.AzureStorage" Version="8\.0\.1"', 'AspNetCore.HealthChecks.AzureStorage" Version="7.0.0"'

    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Fixed: $($file.Name)"
        $count++
    }
}
Write-Host "Total .csproj files fixed: $count"
