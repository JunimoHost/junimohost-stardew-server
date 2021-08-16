FROM jlesage/baseimage-gui:debian-10

ENV APP_NAME="StardewValley"

RUN apt-get update \
    && apt-get install -y wget unzip tar strace mono-complete xterm \
    && apt-get install -y gettext-base jq netcat procps

RUN mkdir -p /data/Stardew && \
    mkdir -p /data/nexus && \
    wget https://eris.cc/Stardew_latest.tar.gz -qO /data/latest.tar.gz && \
    tar xf /data/latest.tar.gz -C /data/Stardew && \
    rm /data/latest.tar.gz 

RUN wget https://github.com/Pathoschild/SMAPI/releases/download/3.12.2/SMAPI-3.12.2-installer.zip -qO /data/nexus.zip && \
    unzip /data/nexus.zip -d /data/nexus/ && \
    /bin/bash -c "echo -e \"2\n/data/Stardew/Stardew\ Valley\n1\n\" | /usr/bin/mono /data/nexus/SMAPI\ 3.12.2\ installer/internal/unix-install.exe" 

RUN \
    APP_ICON_URL=https://stardewcommunitywiki.com/mediawiki/images/4/48/Fiddlehead_Fern.png && \
    install_app_icon.sh "$APP_ICON_URL"

COPY scripts/ /opt/
COPY ["mods", "/data/Stardew/Stardew Valley/Mods/"]

RUN chmod +x /data/Stardew/Stardew\ Valley/StardewValley && \
    chmod -R 777 /data/Stardew/ && \
    chown -R 1000:1000 /data/Stardew && \
    chmod +x /opt/*.sh

COPY docker-entrypoint.sh /startapp.sh