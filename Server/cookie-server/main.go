package main

import (
	service "cookie-server/internal"
	api "cookie-server/internal/server"
	"log"
	"net/http"
)

func main() {
	service := service.New()
	srv, err := api.NewServer(service)

	if err != nil {
		log.Fatal(err)
	}

	log.Println("starting server")

	if err := http.ListenAndServe(":3000", srv); err != nil {
        log.Fatal(err)
    }
}