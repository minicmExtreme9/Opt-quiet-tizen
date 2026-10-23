#!/bin/sh
set -x
/usr/bin/paplay /opt/usr/apps/com.samsung.tv.CinemaGraphApp/res/quiet-home/click.wav
echo "paplay root exit: $?"
su - 5001 -c '/usr/bin/paplay /opt/usr/apps/com.samsung.tv.CinemaGraphApp/res/quiet-home/click.wav'
echo "paplay 5001 exit: $?"
