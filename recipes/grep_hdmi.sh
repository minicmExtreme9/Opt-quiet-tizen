strings /usr/lib/libtvs-av.so 2>/dev/null | grep -iE "hdmi|source.*type" | head -n 30
strings /usr/lib/libsource-api.so | grep -iE "hdmi" | head -n 30
strings /usr/lib/libtvs-util.so 2>/dev/null | grep -iE "hdmi" | head -n 30
