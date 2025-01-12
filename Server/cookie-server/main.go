package main

import (
	service "cookie-server/internal"
	"log"
	"net/http"
	"strconv"

	"time"

	"cookie-server/internal/database"
	internal "cookie-server/internal/pipeline"
	api "cookie-server/internal/server"
	"os"
)

func main() {

	// Umgebungsvariablen für die Datenbankverbindung abrufen
	dbHost := os.Getenv("DB_HOST")
	dbPortStr := os.Getenv("DB_PORT")
	dbUser := os.Getenv("DB_USER")
	dbPassword := os.Getenv("DB_PASSWORD")
	dbName := os.Getenv("DB_NAME")

	dbPort, err := strconv.Atoi(dbPortStr)
	if err != nil {
		log.Fatalf("Ungültiger Datenbankport: %v", err)
		return
	}

	// Verbindung zur Datenbank herstellen
	database, err := database.NewPostgresDatabase(dbHost, dbPort, dbUser, dbPassword, dbName)
	if err != nil {
		log.Fatalf("Fehler beim Verbinden mit der Datenbank: %v", err)
		return
	}
	defer database.Close()

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
