FROM jlesage/baseimage-gui:debian-11

ENV APP_NAME="StardewValley"
# set user to root
ENV USER_ID=0
ENV GROUP_ID=0
ENV TZ=America/New_York
ENV HOME=/root

RUN apt-get update \
    && apt-get install -y \
    wget unzip tar curl pulseaudio

ARG STARDEW_DOWNLOAD_URL=https://storage.googleapis.com/junimo-public/SV156.tar.gz
RUN mkdir -p /data/Stardew && \
    mkdir -p /data/nexus && \
    wget $STARDEW_DOWNLOAD_URL -qO /data/latest.tar.gz && \
    tar xf /data/latest.tar.gz -C /data/Stardew && \
    rm /data/latest.tar.gz 

RUN wget https://github.com/Pathoschild/SMAPI/releases/download/3.16.2/SMAPI-3.16.2-installer.zip -qO /data/nexus.zip && \
    unzip /data/nexus.zip -d /data/nexus/ && \
    /bin/bash -c "echo -e \"2\n\n\" | /data/nexus/SMAPI\ 3.16.2\ installer/internal/linux/SMAPI.Installer --install --game-path \"/data/Stardew/Stardew Valley\"" || : && \
    rm -rf "/data/Stardew/Stardew Valley/Mods/SaveBackup"

RUN rm "/data/Stardew/Stardew Valley/steam_api64.dll" && \
    wget https://storage.googleapis.com/junimo-public/steam_api64.dll -qO /data/Stardew/Stardew Valley/steam_api64.dll

RUN chmod +x /data/Stardew/Stardew\ Valley/StardewValley && \
    chmod -R 777 /data/Stardew/ && \
    chown -R 1000:1000 /data/Stardew

RUN rm "/data/Stardew/Stardew Valley/Content/XACT/Wave Bank.xwb" "/data/Stardew/Stardew Valley/Content/XACT/Wave Bank(1.4).xwb"

COPY ["mods", "/data/Stardew/Stardew Valley/Mods/"]
RUN chmod -R 777 /data/Stardew/Stardew\ Valley/Mods

COPY game-daemon /opt
RUN chmod +x /opt/game-daemon

COPY docker-entrypoint.sh /startapp.sh