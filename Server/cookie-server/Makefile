IMG=cookie-server:latest

.PHONY: setup
setup:
	go mod tidy
	go vet -v
	go fmt ./...
	go generate ./...
	mkdir certs
	openssl genrsa -out certs/server.key 2048
	openssl req -new -key certs/server.key -out certs/server.csr -subj "/CN=localhost"
	openssl x509 -req -days 365 -in certs/server.csr -signkey certs/server.key -out certs/server.crt

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
	docker-compose -f ./scripts/docker-compose/docker-compose.yml --profile core down

.PHONY: run-all-docker
run-all-docker: environment-up observability-up
	docker-compose -f ./scripts/docker-compose/docker-compose.yml --profile core up


.PHONY: docker-build
docker-build:
	docker build -t ${IMG} .
