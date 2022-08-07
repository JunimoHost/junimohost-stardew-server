VERSION=v0.15.1

build: docker/game-daemon docker/mods/JunimoServer $(shell find docker -type f)
	docker build --platform=amd64 -t gcr.io/junimo-host/stardew-base:$(VERSION) ./docker/

clean:
	rm -r ./docker/mods/JunimoServer
	rm -r ./mod/build
	rm ./docker/game-daemon

docker/mods/JunimoServer: $(shell find mod -type f)
ifeq ($(CI), true)
	cd mod && dotnet build -o ./build --configuration Release "/p:EnableModZip=false;EnableModDeploy=false;GamePath=/home/runner/work/junimohost-stardew-server/junimohost-stardew-server/Stardew Valley"
else
	cd mod && dotnet build -o ./build --configuration Release "/p:EnableModZip=false;EnableModDeploy=false"
endif
	mkdir -p ./docker/mods/JunimoServer
	cp ./mod/build/JunimoServer.dll ./mod/build/JunimoServer.pdb ./mod/build/MimeTypesMap.dll ./mod/JunimoServer/manifest.json ./docker/mods/JunimoServer

docker/game-daemon: $(shell find daemon -type f)
	cd daemon && GOOS=linux GOARCH=amd64 go build -o ../docker/game-daemon ./cmd/daemon/daemon.go

push: build
	docker push gcr.io/junimo-host/stardew-base:$(VERSION)

daemon_windows:
	cd daemon && set GOOS=linux && go build -o ../docker/game-daemon ./cmd/daemon/daemon.go
	
	