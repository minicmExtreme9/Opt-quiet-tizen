#!/bin/sh
set -eu
cd /opt/usr/apps/com.samsung.tv.CinemaGraphApp
if python3 -c "import zipfile" 2>/dev/null; then
    python3 -c "
import zipfile, os
out = '/home/owner/share/tmp/sdk_tools/quiet-tizen/CinemaGraphApp.tpk'
with zipfile.ZipFile(out, 'w', zipfile.ZIP_DEFLATED) as z:
    for root, dirs, files in os.walk('.'):
        for f in files:
            p = os.path.join(root, f)
            arcname = os.path.relpath(p, '.')
            z.write(p, arcname)
print('TPK archive created successfully at ' + out)
"
else
    tar -czf /home/owner/share/tmp/sdk_tools/quiet-tizen/CinemaGraphApp-bundle.tar.gz .
    echo "Bundle tar created."
fi
