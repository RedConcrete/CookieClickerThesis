.PHONY: setup
setup:
	go mod tidy
	go vet -v

.PHONY: environment-up
environment-up:
	docker-compose up -d

.PHONY: environment-down
environment-down:
	docker-compose down

.PHONY: run
run: setup environment-up
	go run main.go

# .PHONY: docker-build
# docker-build