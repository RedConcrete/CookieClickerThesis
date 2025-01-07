package main

import (
	service "cookie-server/internal"
	"log"
	"net/http"
	"time"

	"cookie-server/internal/database"
	internal "cookie-server/internal/pipeline"
	api "cookie-server/internal/server"
)

func main() {
	database, err := database.NewPostgresDatabase("localhost", 5460, "postgres", "1234", "CookieData")
	if err != nil {
		log.Fatal(err.Error())
		return
	}
	if err := database.RunMigrations(); err != nil {
		log.Fatal(err.Error())
		return
	}
	cookieService := service.NewCookieService(database)

	// Erstelle einen neuen Kontext mit Abbruch
	// ctx, cancel := context.WithCancel(context.Background())
	// defer cancel() // Stelle sicher, dass der Kontext bei Beendigung abgebrochen wird

	// Starte die Pipeline für die Marktobjekte
	// go internal.StartPipeline(ctx, database, 10*time.Second)

	mg := internal.NewMarketGenerator(database, 10*time.Second)
	go mg.StartGenerator()

	// Erstelle den API-Server mit dem Service
	srv, err := api.NewServer(cookieService)
	if err != nil {
		log.Fatal(err)
		return
	}

	log.Println("starting server")

	// Starte den HTTP-Server
	if err := http.ListenAndServe(":3000", srv); err != nil {
		log.Fatal(err)
		return
	}
}
