for app in org.tizen.netflix-app org.tizen.tv-viewer org.tizen.menu com.samsung.tv.quicklaunch; do
    echo "=== $app ==="
    p=$(find /opt/usr/apps /usr/apps -maxdepth 2 -name "$app" 2>/dev/null | head -n 1)
    if [ -n "$p" ] && [ -f "$p/tizen-manifest.xml" ]; then
        grep -A 5 "<app-control" "$p/tizen-manifest.xml" || echo "no app-control tag"
    else
        echo "path not found"
    fi
done
