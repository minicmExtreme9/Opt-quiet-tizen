grep "quicklaunch" /sys/fs/smackfs/load2 2>/dev/null | head -n 20 || echo "no quicklaunch in load2"
echo "--- CinemaGraphApp ---"
grep "CinemaGraphApp" /sys/fs/smackfs/load2 2>/dev/null | head -n 20 || echo "no CinemaGraphApp in load2"
