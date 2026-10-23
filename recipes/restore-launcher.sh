#!/bin/sh
# SNAPSHOT_NAME ORIGINAL_MAIN_OPERATION_APP ORIGINAL_HOME_APP ORIGINAL_STARTUP_APP [TARGET_APP] [TARGET_DLL]
# Read all three original IDs from that TV's saved records; do not guess them.
set -eu
test "$(id -u)" = 0
name=${1:?}; operation=${2:?}; home=${3:?}; startup=${4:?}
target_app=${5:-com.samsung.tv.bixby-promotion}
target_dll=${6:-BixbyPromotion.dll}
case "$name" in ''|*[!A-Za-z0-9_-]*) exit 1;; esac
for app in "$operation" "$home" "$startup" "$target_app"; do
    case "$app" in ''|*[!A-Za-z0-9_.-]*) exit 1;; esac
done
D=/opt/usr/share/quiet-tizen-backups/$name
A=/opt/usr/apps/$target_app
test -s "$D/Probe.dll"
test -s "$D/$target_app-original/bin/$target_dll"
test -s "$D/.pkgmgr_parser.db"
sha256sum -c "$D/installed-home.sha256"
/usr/bin/dotnet "$D/Probe.dll" set-home "$operation"
vconftool set -f -t string db/appfw/home_appid "$home"
vconftool set -f -t string db/appfw/startup_appid "$startup"
app_launcher -u 5001 -k "$target_app" || true
cp -a "$D/$target_app-original/bin/$target_dll" "$A/bin/$target_dll"
chsmack -a "User::Pkg::$target_app::RO" "$A/bin/$target_dll"
if [ -d "$D/home-native-image" ]; then
    test ! -e "$A/bin/.native_image"
    cp -a "$D/home-native-image" "$A/bin/.native_image"
fi
sqlite3 -bail /opt/dbspace/.pkgmgr_parser.db <<SQL
ATTACH '$D/.pkgmgr_parser.db' AS before;
BEGIN IMMEDIATE;
CREATE TEMP TABLE guard(ok INTEGER CHECK(ok=1));
INSERT INTO guard SELECT count(*)=1 FROM before.package_app_info WHERE app_id='$target_app';
INSERT INTO guard SELECT count(*)=1 FROM package_app_info WHERE app_id='$target_app';
UPDATE package_app_info SET
app_disable=(SELECT app_disable FROM before.package_app_info WHERE app_id='$target_app'),
app_onboot=(SELECT app_onboot FROM before.package_app_info WHERE app_id='$target_app'),
app_autorestart=(SELECT app_autorestart FROM before.package_app_info WHERE app_id='$target_app')
WHERE app_id='$target_app';
DROP TABLE guard;
COMMIT;
SQL
app_launcher -u 5001 -s "$home"
echo 'Original launcher routing and assembly restored. Extra Quiet Home resources remain inert for recovery.'
