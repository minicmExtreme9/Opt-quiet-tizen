ps -p 14858 -o pid,user,%cpu,%mem,vsz,rss,comm
ps -ef | grep -E "csfs|CinemaGraphApp|smart-hub"
