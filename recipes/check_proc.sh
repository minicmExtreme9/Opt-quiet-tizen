pid=$(pgrep -f CinemaGraphApp.dll | head -n 1)
if [ -n "$pid" ]; then
    echo "PID: $pid"
    echo -n "SMACK: "; cat /proc/$pid/attr/current
    echo ""
    echo -n "UID/GID: "; grep -E "Uid|Gid|Groups" /proc/$pid/status
else
    echo "CinemaGraphApp not running"
fi
