.PHONY: environment-up
environment-up:
	docker-compose up -d

.PHONY: environment-down
environment-down:
	docker-compose down

.PHONY: run
run: environment-up
	go run main.go