#!/bin/sh
# Install only. Home/startup routing is changed AFTER manual launch is verified.
# Input: snapshot name. Stage .local/package.tar.gz as quiet-tizen/package.tar.gz first.
set -eu
test "$(id -u)" = 0
name=${1:?Snapshot name required}
case "$name" in ''|*[!A-Za-z0-9_-]*) exit 1;; esac
target_app=${2:-com.samsung.tv.bixby-promotion}
target_dll=${3:-BixbyPromotion.dll}
D=/opt/usr/share/quiet-tizen-backups/$name
A=/opt/usr/apps/$target_app
S=/home/owner/share/tmp/sdk_tools/quiet-tizen
test -s "$D/.pkgmgr_parser.db"
test -s "$S/package.tar.gz"
test -s "$A/bin/$target_dll"
test "$(stat -c %u:%g "$A/bin/$target_dll")" = 201:0
test ! -e "$D/$target_app-original"
test ! -e "$A/res/quiet-home" # First install only; do not silently overwrite an existing custom launcher.
test ! -e "$S/unpacked"
# The archive is generated from our local build, not downloaded from an unknown party.
mkdir "$S/unpacked"
tar -xzf "$S/package.tar.gz" -C "$S/unpacked"
test -s "$S/unpacked/$target_dll"
test -s "$S/unpacked/Probe.dll"
test -s "$S/unpacked/quiet-home/apps.json"
cp -a "$A" "$D/$target_app-original"
cp "$S/unpacked/Probe.dll" "$D/Probe.dll"
/usr/bin/dotnet "$D/Probe.dll" > "$D/operation-and-sources-before.txt"
app_launcher -u 5001 -k "$target_app" || true
if [ -d "$A/bin/.native_image" ]; then mv "$A/bin/.native_image" "$D/home-native-image"; fi
cp "$S/unpacked/$target_dll" "$A/bin/$target_dll"
cp -R "$S/unpacked/quiet-home" "$A/res/"
chown 201:0 "$A/bin/$target_dll"
chmod 644 "$A/bin/$target_dll"
chsmack -a "User::Pkg::$target_app::RO" "$A/bin/$target_dll"
sha256sum "$A/bin/$target_dll" > "$D/installed-home.sha256"
find "$A/res/quiet-home" -type d -exec chmod 755 {} \;
find "$A/res/quiet-home" -type f -exec chmod 644 {} \;
find "$A/res/quiet-home" -exec chown 201:0 {} \;
find "$A/res/quiet-home" -exec chsmack -a "User::Pkg::$target_app::RO" {} \;
sqlite3 -bail /opt/dbspace/.pkgmgr_parser.db <<SQL
BEGIN IMMEDIATE;
CREATE TEMP TABLE guard(ok INTEGER CHECK(ok=1));
INSERT INTO guard SELECT count(*)=1 FROM package_app_info WHERE app_id='$target_app';
UPDATE package_app_info SET app_disable='false',app_onboot='false',app_autorestart='false' WHERE app_id='$target_app';
DROP TABLE guard;
COMMIT;
SQL
app_launcher -u 5001 -s "$target_app"
echo 'Installed and launch requested. Verify the visible UI, remote, sound, apps and HDMI before changing Home routing.'
echo "Original package and operation/source record: $D"
