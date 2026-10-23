#!/bin/sh
set -eu
app_launcher -u 5001 -k com.samsung.tv.CinemaGraphApp || true
sleep 1
rm -rf /home/owner/share/tmp/sdk_tools/quiet-tizen/unpacked
mkdir -p /home/owner/share/tmp/sdk_tools/quiet-tizen/unpacked
tar -xzf /home/owner/share/tmp/sdk_tools/quiet-tizen/package.tar.gz -C /home/owner/share/tmp/sdk_tools/quiet-tizen/unpacked
cp /home/owner/share/tmp/sdk_tools/quiet-tizen/unpacked/CinemaGraphApp.dll /opt/usr/apps/com.samsung.tv.CinemaGraphApp/bin/CinemaGraphApp.dll
if [ -f /home/owner/share/tmp/sdk_tools/quiet-tizen/unpacked/CinemaGraphApp.runtimeconfig.json ]; then
    cp /home/owner/share/tmp/sdk_tools/quiet-tizen/unpacked/CinemaGraphApp.runtimeconfig.json /opt/usr/apps/com.samsung.tv.CinemaGraphApp/bin/CinemaGraphApp.runtimeconfig.json
    chown 201:0 /opt/usr/apps/com.samsung.tv.CinemaGraphApp/bin/CinemaGraphApp.runtimeconfig.json
    chmod 644 /opt/usr/apps/com.samsung.tv.CinemaGraphApp/bin/CinemaGraphApp.runtimeconfig.json
    chsmack -a User::Pkg::com.samsung.tv.CinemaGraphApp::RO /opt/usr/apps/com.samsung.tv.CinemaGraphApp/bin/CinemaGraphApp.runtimeconfig.json
fi
rm -rf /opt/usr/apps/com.samsung.tv.CinemaGraphApp/res/quiet-home
cp -R /home/owner/share/tmp/sdk_tools/quiet-tizen/unpacked/quiet-home /opt/usr/apps/com.samsung.tv.CinemaGraphApp/res/
chown 201:0 /opt/usr/apps/com.samsung.tv.CinemaGraphApp/bin/CinemaGraphApp.dll
chmod 644 /opt/usr/apps/com.samsung.tv.CinemaGraphApp/bin/CinemaGraphApp.dll
chsmack -a User::Pkg::com.samsung.tv.CinemaGraphApp::RO /opt/usr/apps/com.samsung.tv.CinemaGraphApp/bin/CinemaGraphApp.dll
find /opt/usr/apps/com.samsung.tv.CinemaGraphApp/res/quiet-home -type d -exec chmod 755 {} \;
find /opt/usr/apps/com.samsung.tv.CinemaGraphApp/res/quiet-home -type f -exec chmod 644 {} \;
find /opt/usr/apps/com.samsung.tv.CinemaGraphApp/res/quiet-home -exec chown 201:0 {} \;
find /opt/usr/apps/com.samsung.tv.CinemaGraphApp/res/quiet-home -exec chsmack -a User::Pkg::com.samsung.tv.CinemaGraphApp::RO {} \;
sync
app_launcher -u 5001 -s com.samsung.tv.CinemaGraphApp
echo "Updated and launched."
