# Remove duplicate using directives and fix malformed files
$files = Get-ChildItem -Path "backend\src\HomeService.Application" -Filter "*.cs" -Recurse
$count = 0
foreach ($file in $files) {
    $lines = Get-Content $file.FullName
    $newLines = @()
    $usings = @{}
    $inUsingsSection = $true

    foreach ($line in $lines) {
        # Check if we're still in the using section
        if ($line -match '^using ') {
            # Only add if not duplicate
            if (!$usings.ContainsKey($line)) {
                $usings[$line] = $true
                $newLines += $line
            }
        } elseif ($line -match '^namespace ' -or $line -match '^\s*$' -or $line -match '^//') {
            $newLines += $line
        } else {
            # We've left the using section
            $inUsingsSection = $false
            # Skip any "using HomeService.Domain.Interfaces;" that appears in code
            if ($line -notmatch '^\s*using HomeService\.Domain\.Interfaces;\s*$') {
                $newLines += $line
            }
        }
    }

    $newContent = $newLines -join "`r`n"
    Set-Content -Path $file.FullName -Value $newContent
    $count++
}
Write-Host "Total files cleaned: $count"
