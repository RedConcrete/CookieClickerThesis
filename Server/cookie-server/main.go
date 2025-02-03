package main

import (
	service "cookie-server/internal"
	"log"
	"net/http"
	"strconv"

	"cookie-server/internal/database"
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

	dir, err := os.Getwd()
	if err != nil {
		log.Fatal(err)
	}
	log.Println("Aktuelles Arbeitsverzeichnis:", dir)

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

	// Erstelle den API-Server mit dem Service
	srv, err := api.NewServer(cookieService)
	if err != nil {
		log.Fatal(err)
		return
	}

	log.Println("starting server")

	// Starte den HTTPS-Server auf Port 3000
	certFile := "/etc/letsencrypt/live/r3dconcrete.de-0001/fullchain.pem"
	keyFile := "/etc/letsencrypt/live/r3dconcrete.de-0001/privkey.pem"

	// Starte den HTTPS-Server
	if err := http.ListenAndServeTLS(":3000", certFile, keyFile, srv); err != nil {
		log.Fatal(err)
		return
	}
}
