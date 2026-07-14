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
#   1. Update <Version> in Kuwantima.csproj
#   2. Commit all changes
#   3. Tag the commit
#   4. Push commit + tag to origin
#   5. GitHub Actions handles NuGet pack + publish (Trusted Publishing)
# ═══════════════════════════════════════════════════════════════════

set -e

VERSION="$1"
REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"
CSPROJ="$REPO_ROOT/Kuwantima/Kuwantima.csproj"

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

# ── Pre-flight checks ────────────────────────────────────────────
echo ""
echo "Pre-flight checklist:"
echo "  [ ] VERSION HISTORY updated in KuwantimaPrimaryTheme.axaml?"
echo "  [ ] Version entry added in DocumentsPage.axaml?"
echo "  [ ] doc-version stamp bumped in all three docs/*.html handouts?"
echo "  [ ] README.md updated if needed?"
echo ""
read -p "Continue? (y/n) " -n 1 -r
echo ""
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Aborted."
    exit 0
fi

# ── Step 1: Update .csproj version ───────────────────────────────
echo ""
echo "Step 1/6: Updating version in .csproj..."
sed -i "s|<Version>[^<]*</Version>|<Version>$VERSION</Version>|" "$CSPROJ"
echo "  → Kuwantima.csproj <Version> set to $VERSION"

# ── Step 2: Commit ───────────────────────────────────────────────
echo ""
echo "Step 2/6: Committing..."
cd "$REPO_ROOT"
git add -A
git commit -m "Release Kuwantima v$VERSION

Co-Authored-By: Claude Opus 4.8 (1M context) <noreply@anthropic.com>"
echo "  → Committed"

# ── Step 3: Tag ──────────────────────────────────────────────────
echo ""
echo "Step 3/6: Tagging v$VERSION..."
git tag "v$VERSION"
echo "  → Tagged v$VERSION"

# ── Step 4: Push ─────────────────────────────────────────────────
echo ""
echo "Step 4/6: Pushing to origin..."
git push origin master
git push origin "v$VERSION"
echo "  → Pushed commit + tag"

# ── Step 5: NuGet publish ────────────────────────────────────────
echo ""
echo "Step 5/5: NuGet publish triggered via GitHub Actions"
echo "  → The v$VERSION tag push triggers .github/workflows/publish.yml"
echo "  → GitHub Actions will build, pack, and publish to nuget.org"
echo "  → Monitor at: https://github.com/Votskwani/Kuwantima/actions"

echo ""
echo "═══════════════════════════════════════════════════════"
echo "  Kuwantima v$VERSION — done!"
echo "═══════════════════════════════════════════════════════"
