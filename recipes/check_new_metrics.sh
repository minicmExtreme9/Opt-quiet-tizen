cat /tmp/quiet-home-state.txt
echo "--- ACTIVE PROCESS STATS ---"
ps -C DN_nemaGraphApp -o pid,user,%cpu,%mem,vsz,rss,comm 2>/dev/null || ps -ef | grep CinemaGraphApp | grep -v grep
echo "--- MEMINFO ---"
cat /proc/meminfo | grep -E '^(MemTotal|MemFree|MemAvailable|SwapTotal|SwapFree)'
