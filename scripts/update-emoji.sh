#!/usr/bin/env bash
#
# Fetches the Twemoji artwork into LetterGenerator/Emoji.
#
#   ./scripts/update-emoji.sh
#
# Upstream is jdecked/twemoji. The graphics license is CC-BY 4.0.

set -euo pipefail

readonly REPO="jdecked/twemoji"
readonly ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
readonly EMOJI_DIR="$ROOT/LetterGenerator/Emoji"
readonly TARBALL_FILENAME="twemoji.tar.gz"

work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT

# Get latest tarball
curl -s https://api.github.com/repos/$REPO/releases/latest \
| grep "tarball_url" \
| cut -d : -f 2,3 \
| tr -d \", \
| xargs -L 1 curl -fsSL -o "$work/$TARBALL_FILENAME"

# Extract the assets
tar xzf "$work/$TARBALL_FILENAME" -C "$work" \
    "*/assets/72x72"
rm "$work/$TARBALL_FILENAME"
extracted="$(ls "$work")"

# Copy assets to emoji dir
readonly SOURCE_DIR="$work/$extracted/assets/72x72"
mkdir -p "$EMOJI_DIR"
cp "$SOURCE_DIR"/*.png "$EMOJI_DIR"/

echo "Done"
