#!/bin/bash
export HOME=/config # not sure why possibly to due with where saves are placed

/opt/tail-smapi-log.sh &

export XAUTHORITY=~/.Xauthority
TERM=
sed -i -e 's/env TERM=xterm $LAUNCHER "$@"$/env SHELL=\/bin\/bash TERM=xterm xterm  -e "\/bin\/bash -c $LAUNCHER "$@""/' /data/Stardew/Stardew\ Valley/StardewValley

bash -c "/data/Stardew/Stardew\ Valley/StardewValley"

sleep 233333333333333