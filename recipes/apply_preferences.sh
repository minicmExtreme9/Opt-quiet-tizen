#!/bin/sh
set -eu
vconftool set -f -t string db/was/preload_by_country false
vconftool set -f -t bool db/menu/autorun_tv_plus 0
vconftool set -f -t bool db/menu/voice/voice_activated 0
vconftool set -f -t int db/menu/voice/wakeupless_status 0
vconftool set -f -t bool db/si_service/voice_recog_enable 0
vconftool set -f -t bool db/multi-assistant/prelaunch_mode 0
sqlite3 -bail /opt/dbspace/.webappservice.db <<'EOF'
BEGIN IMMEDIATE;
CREATE TEMP TABLE guard(ok INTEGER CHECK(ok=1));
INSERT INTO guard SELECT count(*)=1 FROM WASConfigTable
WHERE name='RESERVED_18' AND json_valid(value)
AND json_type(value,'$.rsp.preloadByCountry')='text'
AND json_extract(value,'$.rsp.preloadByCountry')='true';
UPDATE WASConfigTable
SET value=json_set(value,'$.rsp.preloadByCountry','false')
WHERE name='RESERVED_18';
DROP TABLE guard;
COMMIT;
EOF
echo "Preferences updated."
