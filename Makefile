VERSION=v0.17.26

build: docker/mods/JunimoServer $(shell find docker -type f)
	docker build --platform=amd64 -t gcr.io/junimo-host/stardew-base:$(VERSION) -f docker/Dockerfile .

clean:
	rm -rf ./docker/mods/JunimoServer
	rm -rf ./mod/build

docker/mods/JunimoServer: $(shell find mod/JunimoServer/**/*.cs -type f) ./mod/JunimoServer/JunimoServer.csproj
ifeq ($(CI), true)
	cd mod && dotnet build -o ./build --configuration Release "/p:EnableModZip=false;EnableModDeploy=false;GamePath=/home/runner/actions-runner/_work/junimohost-stardew-server/junimohost-stardew-server/Stardew Valley"
else
	cd mod && dotnet build -o ./build --configuration Release
endif
	mkdir -p ./docker/mods/JunimoServer
	cp ./mod/build/JunimoServer.dll ./mod/build/JunimoServer.pdb ./mod/build/Microsoft.Extensions.Logging.Abstractions.dll ./mod/build/Google.Protobuf.dll ./mod/build/Grpc.Core.Api.dll ./mod/build/Grpc.Net.Client.dll ./mod/build/Grpc.Net.Common.dll ./mod/JunimoServer/manifest.json ./docker/mods/JunimoServer

game-daemon: $(shell find daemon -type f)
	GOOS=linux GOARCH=amd64 go build -o game-daemon ./cmd/daemon/daemon.go

push: build
	docker push gcr.io/junimo-host/stardew-base:$(VERSION)

daemon_windows:
	cd daemon && set GOOS=linux && go build -o game-daemon ./cmd/daemon/daemon.go
	
	