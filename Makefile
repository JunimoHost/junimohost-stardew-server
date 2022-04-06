VERSION=v0.11.9

build:
	docker build -t gcr.io/junimo-host/stardew-base:$(VERSION) ./docker/
push: build
	docker push gcr.io/junimo-host/stardew-base:$(VERSION)