build:
	docker build -t gcr.io/junimo-host/stardew-base:dev ./docker/
push: build
	docker push gcr.io/junimo-host/stardew-base:dev