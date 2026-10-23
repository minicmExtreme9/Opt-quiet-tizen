for app in /opt/usr/apps/*; do
  if [ -f "$app/bin/.native_image/"*.ni.dll ] 2>/dev/null || [ -f "$app/bin/"*.dll ] 2>/dev/null; then
    stat -c "%u:%g %n" "$app"
  fi
done
