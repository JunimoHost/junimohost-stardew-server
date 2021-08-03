push:
	docker build -t gcr.io/junimo-host/stardew-base:dev ./docker
	docker push gcr.io/junimo-host/stardew-base:dev