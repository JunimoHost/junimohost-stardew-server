VERSION=v0.14.2

daemon:
	GOOS=linux GOARCH=amd64 go build -o ./docker/game-daemon ./cmd/daemon/daemon.go

build:
	docker build --platform=amd64 -t gcr.io/junimo-host/stardew-base:$(VERSION) ./docker/

push: build
	docker push gcr.io/junimo-host/stardew-base:$(VERSION)