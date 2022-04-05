#!/bin/bash
/opt/tail-smapi-log.sh &
/opt/game-daemon &

# wait until game-daemon says its ok to start stardew

export XAUTHORITY=~/.Xauthority
TERM=
sed -i -e 's/env TERM=xterm $LAUNCHER "$@"$/env SHELL=\/bin\/bash TERM=xterm xterm  -e "\/bin\/bash -c $LAUNCHER "$@""/' /data/Stardew/Stardew\ Valley/StardewValley

bash -c "/data/Stardew/Stardew\ Valley/StardewValley"

sleep 233333333333333