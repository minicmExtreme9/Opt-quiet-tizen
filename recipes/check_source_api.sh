objdump -T /usr/lib/libsource-api.so 2>/dev/null || nm -D /usr/lib/libsource-api.so 2>/dev/null || strings /usr/lib/libsource-api.so | grep -E "source|hdmi|HDMI" | head -n 30
