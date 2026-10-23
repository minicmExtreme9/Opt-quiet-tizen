dlogutil -c
dotnet /home/owner/share/tmp/sdk_tools/quiet-tizen/Probe.dll test-source 13
dlogutil -d | grep -iE "source|hdmi" | tail -n 35
