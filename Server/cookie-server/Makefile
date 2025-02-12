.PHONY: setup
setup:
	go mod tidy
	go vet -v
	go fmt ./...
	go generate ./...

.PHONY: environment-up
environment-up:
	docker-compose -f ./scripts/docker-compose/docker-compose.yml --profile basic up -d

.PHONY: environment-down
environment-down:
	docker-compose -f ./scripts/docker-compose/docker-compose.yml --profile basic down

.PHONY: observability-up
observability-up:
	docker-compose -f ./scripts/docker-compose/docker-compose.yml --profile analysis up -d

.PHONY: observability-down
observability-down:
	docker-compose -f ./scripts/docker-compose/docker-compose.yml --profile analysis down

.PHONY: run-local
run-local: environment-up
	go run ./cmd/cookie-server/main.go

.PHONY: run-observed-local
run-observed-local: environment-up observability-up
	go run ./cmd/cookie-server/main.go

.PHONY: all-down
all-down: environment-down observability-down

