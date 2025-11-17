# Add missing using directive for IRepository and IUnitOfWork
$files = Get-ChildItem -Path "backend\src\HomeService.Application\Handlers" -Filter "*.cs" -Recurse
$count = 0
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $needsUpdate = $false

    # Check if file uses IRepository or IUnitOfWork but doesn't import the namespace
    if (($content -match 'IRepository|IUnitOfWork') -and ($content -notmatch 'using HomeService\.Domain\.Interfaces;')) {
        # Add the using directive after the first using statement
        $content = $content -replace '(using [^;]+;)', "`$1`nusing HomeService.Domain.Interfaces;"
        $needsUpdate = $true
    }

    if ($needsUpdate) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Fixed: $($file.Name)"
        $count++
    }
}
Write-Host "Total files fixed: $count"
