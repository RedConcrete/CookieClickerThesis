package main

import (
	"log"
	"net/http"

	service "cookie-server/internal"
	"cookie-server/internal/database"
	api "cookie-server/internal/server"
)

func main() {
	database, err := database.NewPostgresDatabase("localhost", 5423, "postgres", "1234", "CookieData")
	if err != nil {
		log.Fatal(err.Error())
	}
	cookieService := service.NewCookieService(database)

	// Erstelle den API-Server mit dem Service
	srv, err := api.NewServer(cookieService)
	if err != nil {
		log.Fatal(err)
	}

	log.Println("starting server")

	// Starte den HTTP-Server
	if err := http.ListenAndServe(":3000", srv); err != nil {
		log.Fatal(err)
	}
}
