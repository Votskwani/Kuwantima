#!/bin/bash
# ═══════════════════════════════════════════════════════════════════
# Kuwantima Publish Script
# ═══════════════════════════════════════════════════════════════════
# Usage:  ./publish.sh <version>
#
# Before running, ensure you have:
#   1. Updated VERSION HISTORY in KuwantimaPrimaryTheme.axaml
#   2. Added version entry in DocumentsPage.axaml
#   3. Updated the doc-version stamp in all three docs/*.html handouts
#   4. Updated CLAUDE.md if checklists changed
#   5. Made all feature/fix changes
#
# This script will:
#   1. Update <Version> in Kuwantima.csproj AND Kuwantima.DataGrid.csproj (lockstep — see
#      CLAUDE.md's Version Management section; both packages ship under one version number)
#   2. Commit all changes
#   3. Tag the commit
#   4. Push commit + tag to origin
#   5. GitHub Actions handles NuGet pack + publish (Trusted Publishing)
# ═══════════════════════════════════════════════════════════════════

set -e

VERSION="$1"
REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"
CSPROJS=(
    "$REPO_ROOT/Kuwantima/Kuwantima.csproj"
    "$REPO_ROOT/Kuwantima.DataGrid/Kuwantima.DataGrid.csproj"
)

# ── Validate ──────────────────────────────────────────────────────
if [ -z "$VERSION" ]; then
    echo "Usage: ./publish.sh <version> [nuget-api-key]"
    echo "  e.g. ./publish.sh 1.1.0"
    echo "  e.g. ./publish.sh 1.1.0 oy2abc..."
    exit 1
fi

if ! echo "$VERSION" | grep -qE '^[0-9]+\.[0-9]+\.[0-9]+$'; then
    echo "Error: Version must be in format X.Y.Z (e.g. 1.1.0)"
    exit 1
fi

echo "═══════════════════════════════════════════════════════"
echo "  Kuwantima Publish — v$VERSION"
echo "═══════════════════════════════════════════════════════"

# ── Step 0: Sync check ───────────────────────────────────────────
# Guards against clobbering a push that landed on origin/master since this checkout last synced —
# relevant now that this repo may have more than one active contributor. Fetch-and-compare rather
# than trusting local state, same "verify against ground truth" discipline as everything else in
# this repo (see CLAUDE.md's verification-trap section).
echo ""
echo "Step 0/6: Checking sync with origin..."
cd "$REPO_ROOT"
git fetch origin
LOCAL="$(git rev-parse master)"
REMOTE="$(git rev-parse origin/master)"
BASE="$(git merge-base master origin/master)"

if [ "$LOCAL" = "$REMOTE" ]; then
    echo "  → Up to date with origin/master"
elif [ "$LOCAL" = "$BASE" ]; then
    echo "Error: local master is behind origin/master — someone else has pushed since this checkout last synced."
    echo "  Run 'git pull --rebase origin master' first, then re-run this script."
    exit 1
elif [ "$REMOTE" = "$BASE" ]; then
    echo "  → Local master is ahead of origin/master (expected — you have unpushed work)"
else
    echo "Error: local and origin/master have DIVERGED — someone else has pushed different commits."
    echo "  Resolve this manually (git fetch, then rebase or merge) before publishing."
    exit 1
fi

# ── Pre-flight checks ────────────────────────────────────────────
echo ""
echo "Pre-flight checklist:"
echo "  [ ] VERSION HISTORY updated in KuwantimaPrimaryTheme.axaml AND KuwantimaDataGridTheme.axaml?"
echo "  [ ] Version entry added in DocumentsPage.axaml?"
echo "  [ ] doc-version stamp bumped in all three docs/*.html handouts?"
echo "  [ ] README.md updated if needed?"
echo "  [ ] PackageReleaseNotes rewritten in BOTH csproj files — state which package(s) actually changed?"
echo ""
read -p "Continue? (y/n) " -n 1 -r
echo ""
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Aborted."
    exit 0
fi

# ── Step 1: Update .csproj version (lockstep across both packages) ──
echo ""
echo "Step 1/6: Updating version in both .csproj files..."
for CSPROJ in "${CSPROJS[@]}"; do
    sed -i "s|<Version>[^<]*</Version>|<Version>$VERSION</Version>|" "$CSPROJ"
    echo "  → $(basename "$CSPROJ") <Version> set to $VERSION"
done

# ── Step 2: Commit ───────────────────────────────────────────────
echo ""
echo "Step 2/6: Committing..."
cd "$REPO_ROOT"
git add -A
git commit -m "Release Kuwantima v$VERSION

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
echo "  → Committed"

# ── Step 3: Tag ──────────────────────────────────────────────────
echo ""
echo "Step 3/6: Tagging v$VERSION..."
git tag "v$VERSION"
echo "  → Tagged v$VERSION"

# ── Step 4: Push ─────────────────────────────────────────────────
# Re-check sync immediately before pushing too — the window between Step 0 and here (the
# pre-flight prompt alone can sit open indefinitely) is exactly when a second contributor's push
# could land, and a stale fetch from Step 0 wouldn't catch it.
echo ""
echo "Step 4/6: Re-checking sync, then pushing to origin..."
git fetch origin
if [ "$(git rev-parse origin/master)" != "$REMOTE" ]; then
    echo "Error: origin/master moved since Step 0's check — someone else just pushed."
    echo "  Run 'git pull --rebase origin master' (or merge) first, then re-run this script."
    exit 1
fi
git push origin master
git push origin "v$VERSION"
echo "  → Pushed commit + tag"

# ── Step 5: NuGet publish ────────────────────────────────────────
echo ""
echo "Step 5/6: NuGet publish triggered via GitHub Actions"
echo "  → The v$VERSION tag push triggers .github/workflows/publish.yml"
echo "  → GitHub Actions will build, pack, and publish to nuget.org"
echo "  → Monitor at: https://github.com/Votskwani/Kuwantima/actions"

echo ""
echo "═══════════════════════════════════════════════════════"
echo "  Kuwantima v$VERSION — done!"
echo "═══════════════════════════════════════════════════════"
