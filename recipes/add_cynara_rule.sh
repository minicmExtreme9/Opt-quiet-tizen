#!/bin/sh
set -eu

DB="/var/cynara/db"
if [ ! -f "$DB/_MANIFESTS_GLOBAL.bak" ]; then
    cp -a "$DB/_MANIFESTS_GLOBAL" "$DB/_MANIFESTS_GLOBAL.bak"
    cp -a "$DB/checksum" "$DB/checksum.bak"
fi

RULE="User::Pkg::com.samsung.tv.CinemaGraphApp;*;http://tizen.org/privilege/appmanager.launch;0xFFFF;"
if ! grep -Fxq "$RULE" "$DB/_MANIFESTS_GLOBAL"; then
    echo "$RULE" >> "$DB/_MANIFESTS_GLOBAL"
fi

NEW_MD5=$(md5sum "$DB/_MANIFESTS_GLOBAL" | cut -d' ' -f1)
sed -i "s/^_MANIFESTS_GLOBAL;.*/_MANIFESTS_GLOBAL;$NEW_MD5/" "$DB/checksum"

# Restart cynara
CYNARA_PID=$(pgrep -f "/usr/bin/cynara" || true)
if [ -n "$CYNARA_PID" ]; then
    kill "$CYNARA_PID" || true
    sleep 1
fi

echo "Cynara updated with new MD5: $NEW_MD5, new PID: $(pgrep -f /usr/bin/cynara || echo 'none')"
