# GitHub cleanup for mtmattei, from docs/github-audit-2026-10.md
#
# Dry run (default):   pwsh -File docs/github-cleanup.ps1
# Archive only:        pwsh -File docs/github-cleanup.ps1 -Execute -Archive
# Delete only:         pwsh -File docs/github-cleanup.ps1 -Execute -Delete
#
# Deleting needs the delete_repo scope once:  gh auth refresh -h github.com -s delete_repo
# Archiving is reversible (gh repo unarchive). Deleting is not.

param(
    [switch]$Execute,
    [switch]$Archive,
    [switch]$Delete
)

$Owner = 'mtmattei'

# Empty or trivial. Download Lumen's LUMEN_Project_Kit.zip first if you want it.
$DeleteRepos = @(
    'ChefsOmakase-test', 'DYT', 'Uno-particle-effects', 'uno.hotdesign',
    'desktop-tutorial', 'Calculator', 'PuckUp', 'Nexus', 'Lumen',
    # Public Firebase key: restrict or delete it in Google Cloud Console first
    'FCM-Push-Notifications-Test'
)

# Stale forks. uno, uno.extensions and Uno.Samples are kept on purpose (open PRs).
$DeleteForks = @(
    'awesome-mcp-servers-1', 'awesome-mcp-servers', 'bootstrap', 'AncestorBindingSample',
    'good-first-issue', 'awesome-uno-platform', 'workshops', 'stargazer', 'uno.resizetizer',
    'Microcharts', 'discoverdotnet', 'figma-docs', 'hacktoberfest-swag', 'ArchitectureWeekly',
    'wasmweekly'
)

# Safe to archive now: duplicated in Uno-Builds, old demos, finished labs.
$ArchiveRepos = @(
    # Duplicates of an Uno-Builds folder
    'Sweather', 'ConfPass', 'FibonacciSphere', 'FriendSonar', 'matrix', 'LiquidMorph',
    'Orbital', 'radial-action-menu', 'parallax-invitation-cards', 'memory-drift',
    'Composer', 'Thermostat-Build', 'SantaTracker',
    # 2025 Hot Design demos
    'Habits', 'HorizontalCalendar', 'EV-ChargingApp', 'NetflixSplash', 'UnoGPT5SearchBar',
    'EnergyDashboard', 'Codemash',
    # Done, parked or superseded
    'Naoto-Light', 'DesignSkillEval', 'Aware', 'DeskBoard',
    'ChefsTest', 'UnoPlatformSkills', 'LiquidGlassProbe'
)

# NOT run by this script. Archive each one by hand after its code has moved:
#   Uno-Builds-net10, Workflow, Build-Samples     -> rescue unique folders into Uno-Builds
#   Meridian, driftline                           -> after Liveline is its own repo
#   Dorval, Puck                                  -> after dorval-youngtimers absorbs call-ups
#   trace, fieldcheck-maui                        -> after Patina absorbs them
#   Tactile, Blur-shader, LiquidGlassLab,
#   Hyperspeed, DigitalFidget                     -> after uno.particles absorbs them

function Invoke-Step([string]$Verb, [string]$Repo) {
    $full = "$Owner/$Repo"
    if (-not $Execute) { Write-Host "[dry run] $Verb $full"; return }
    switch ($Verb) {
        'archive' { gh repo archive $full --yes }
        'delete'  { gh repo delete  $full --yes }
    }
    if ($LASTEXITCODE -ne 0) { Write-Warning "$Verb failed for $full" }
}

if (-not $Execute -or $Archive) { $ArchiveRepos | ForEach-Object { Invoke-Step 'archive' $_ } }
if (-not $Execute -or $Delete)  { ($DeleteRepos + $DeleteForks) | ForEach-Object { Invoke-Step 'delete' $_ } }

if ($Execute -and -not ($Archive -or $Delete)) {
    Write-Host 'Pass -Archive and/or -Delete with -Execute to choose what runs.'
}
