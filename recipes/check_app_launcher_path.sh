which app_launcher
ls -la $(which app_launcher)
ls -la /usr/bin/app_launcher
smackgetattr /usr/bin/app_launcher 2>/dev/null || true
