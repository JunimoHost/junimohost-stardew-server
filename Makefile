VERSION=v0.15.0-stress

mod:
	dotnet build -o ./out --configuration Release "/p:EnableModZip=false"

docker/game-daemon: $(shell find daemon -type f)
	cd daemon && GOOS=linux GOARCH=amd64 go build -o ../docker/game-daemon ./cmd/daemon/daemon.go

build: docker/game-daemon $(shell find docker -type f)
	docker build --platform=amd64 -t gcr.io/junimo-host/stardew-base:$(VERSION) ./docker/

push: build
	docker push gcr.io/junimo-host/stardew-base:$(VERSION)

daemon_windows:
	cd daemon && set GOOS=linux && go build -o ../docker/game-daemon ./cmd/daemon/daemon.go
	
	