# docker context is from the root folder, not ./docker
# --- Build Stage ---
FROM jlesage/baseimage-gui:debian-11 as build-stage

WORKDIR /app

# Install Go 1.19
RUN apt-get update && \
    apt-get install -y wget && \
    wget https://golang.org/dl/go1.21.3.linux-amd64.tar.gz && \
    tar -C /usr/local -xzf go1.21.3.linux-amd64.tar.gz && \
    rm go1.21.3.linux-amd64.tar.gz

# Add Go to PATH
ENV PATH="/usr/local/go/bin:${PATH}"

# Verify Go installation
RUN go version

# Copy go mod and sum files 
COPY daemon/go.mod daemon/go.sum ./

# Download all dependencies. 
# Dependencies will be cached if the go.mod and go.sum files are not changed 
RUN go mod download 

COPY daemon .

# Build the Go app
RUN go build -o game-daemon ./cmd/daemon/daemon.go

# --- Final Stage ---
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

RUN wget https://github.com/Pathoschild/SMAPI/releases/download/3.18.6/SMAPI-3.18.6-installer.zip -qO /data/nexus.zip && \
    unzip /data/nexus.zip -d /data/nexus/ && \
    /bin/bash -c "echo -e \"2\n\n\" | /data/nexus/SMAPI\ 3.18.6\ installer/internal/linux/SMAPI.Installer --install --game-path \"/data/Stardew/Stardew Valley\"" || : && \
    rm -rf "/data/Stardew/Stardew Valley/Mods/SaveBackup"

RUN rm "/data/Stardew/Stardew Valley/steam_api64.dll" && \
    wget https://storage.googleapis.com/junimo-public/steam/steam_api64.dll -O /data/Stardew/Stardew\ Valley/steam_api64.dll && \
    rm "/data/Stardew/Stardew Valley/libsteam_api.so" && \
    wget https://storage.googleapis.com/junimo-public/steam/libsteam_api.so -O /data/Stardew/Stardew\ Valley/libsteam_api.so && \
    mkdir -p /root/.steam/sdk64 && \
    wget https://storage.googleapis.com/junimo-public/steam/steamclient.so -O /root/.steam/sdk64/steamclient.so

COPY ["docker/config.user.json", "/data/Stardew/Stardew Valley/smapi-internal/"] 

RUN chmod +x /data/Stardew/Stardew\ Valley/StardewValley && \
    chmod -R 777 /data/Stardew/ && \
    chown -R 1000:1000 /data/Stardew

RUN rm "/data/Stardew/Stardew Valley/Content/XACT/Wave Bank.xwb" "/data/Stardew/Stardew Valley/Content/XACT/Wave Bank(1.4).xwb"

COPY ["docker/mods", "/data/Stardew/Stardew Valley/Mods/"]
RUN chmod -R 777 /data/Stardew/Stardew\ Valley/Mods

COPY --from=build-stage /app/game-daemon /opt/game-daemon
RUN chmod +x /opt/game-daemon

COPY docker/docker-entrypoint.sh /startapp.sh