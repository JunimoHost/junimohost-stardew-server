#!/bin/bash
/opt/game-daemon &

while [[ "$(curl -s -o /dev/null -w ''%{http_code}'' localhost:8080/startup)" != "200" ]]; do sleep 5; done

tail -F /config/xdg/config/StardewValley/ErrorLogs/SMAPI-latest.txt &

# wait until game-daemon says its ok to start stardew

export XAUTHORITY=~/.Xauthority
HOME=/root pulseaudio -vvv --daemonize

bash -c "/data/Stardew/Stardew\ Valley/StardewValley"
