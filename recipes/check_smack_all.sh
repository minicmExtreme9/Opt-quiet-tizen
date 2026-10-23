grep -h "App::Manager" /etc/smack/accesses.d/* 2>/dev/null | head -n 30 || echo "no App::Manager in accesses.d"
echo "--- checking active smack rules ---"
grep "com.samsung.tv.quicklaunch" /etc/smack/accesses.d/* 2>/dev/null || echo "none for quicklaunch"
grep "com.samsung.tv.CinemaGraphApp" /etc/smack/accesses.d/* 2>/dev/null || echo "none for CinemaGraphApp"
