# Get the most recent tag
$latestTag = git tag --sort=-v:refname | Select-Object -First 1

if (-not $latestTag) {
    Write-Host "No tags found. Comparing from the beginning of history."
    $range = "HEAD"
    $titleVersion = "Initial Release"
} else {
    # Check if HEAD is the latest tag
    $headHash = git rev-parse HEAD
    $tagHash = git rev-parse $latestTag

    if ($headHash -eq $tagHash) {
        # We are on the tag, so we want the changes IN this tag
        $previousTag = git tag --sort=-v:refname | Select-Object -Skip 1 -First 1
        if (-not $previousTag) {
            $range = $latestTag
        } else {
            $range = "$previousTag..$latestTag"
        }
        $titleVersion = $latestTag
    } else {
        # We are ahead of the tag, so we want changes since the tag
        $range = "$latestTag..HEAD"
        $titleVersion = "New Release" # Will be overridden if version is known
    }
}

Write-Host "Generating release notes for range: $range"

$commits = git log $range --oneline

$feats = @()
$fixes = @()
$refactors = @()
$docs = @()
$ci = @()
$others = @()

foreach ($line in $commits) {
    if ($line.Length -lt 9) { continue }
    $commit = $line.Substring(8) # Skip hash
    if ($commit -match "^feat:") { $feats += "- " + ($commit -replace "^feat:\s*", "") }
    elseif ($commit -match "^fix:") { $fixes += "- " + ($commit -replace "^fix:\s*", "") }
    elseif ($commit -match "^refactor:") { $refactors += "- " + ($commit -replace "^refactor:\s*", "") }
    elseif ($commit -match "^docs:") { $docs += "- " + ($commit -replace "^docs:\s*", "") }
    elseif ($commit -match "^ci:") { $ci += "- " + ($commit -replace "^ci:\s*", "") }
    elseif ($commit -match "^chore:") { $others += "- " + ($commit -replace "^chore:\s*", "") }
    else { $others += "- " + $commit }
}

$releaseNotes = "" # No title, GitHub Release will provide it

if ($feats.Count -gt 0) {
    $releaseNotes += "### Features`n"
    foreach ($f in $feats) { $releaseNotes += "$f`n" }
    $releaseNotes += "`n"
}

if ($fixes.Count -gt 0) {
    $releaseNotes += "### Bug Fixes`n"
    foreach ($f in $fixes) { $releaseNotes += "$f`n" }
    $releaseNotes += "`n"
}

if ($refactors.Count -gt 0) {
    $releaseNotes += "### Refactors`n"
    foreach ($r in $refactors) { $releaseNotes += "$r`n" }
    $releaseNotes += "`n"
}

if ($docs.Count -gt 0) {
    $releaseNotes += "### Documentation`n"
    foreach ($d in $docs) { $releaseNotes += "$d`n" }
    $releaseNotes += "`n"
}

if ($ci.Count -gt 0) {
    $releaseNotes += "### CI/CD`n"
    foreach ($c in $ci) { $releaseNotes += "$c`n" }
    $releaseNotes += "`n"
}

if ($others.Count -gt 0) {
    $releaseNotes += "### Other Changes`n"
    foreach ($o in $others) { $releaseNotes += "$o`n" }
    $releaseNotes += "`n"
}

if ($releaseNotes -eq "") {
    $releaseNotes = "No major changes in this release."
}

$releaseNotes | Out-File -FilePath "RELEASE_NOTES.md" -Encoding utf8
Write-Host "Release notes generated in RELEASE_NOTES.md"
