VERSION=v0.5.0

build:
	docker build -t gcr.io/junimo-host/stardew-base:$(VERSION) ./docker/
push: build
	docker push gcr.io/junimo-host/stardew-base:$(VERSION)