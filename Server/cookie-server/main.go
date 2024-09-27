package main

import (
	"database/sql"
	"fmt"
	"log"
	"net/http"

	service "cookie-server/internal"
	api "cookie-server/internal/server"

	_ "github.com/lib/pq" // Importiere den PostgreSQL-Treiber
)

func connectToDB() (*sql.DB, error) {
	connStr := "host=localhost port=5432 user=postgres password=1234 dbname=CookieData sslmode=disable"
	db, err := sql.Open("postgres", connStr)
	if err != nil {
		return nil, err
	}

	// Testen der Verbindung
	err = db.Ping()
	if err != nil {
		return nil, err
	}

	return db, nil
}

func main() {
	// Verbindung zur Datenbank aufbauen
	db, err := connectToDB()
	if err != nil {
		log.Fatal("Fehler beim Verbinden zur Datenbank: ", err)
	}
	defer db.Close()

	fmt.Println("Erfolgreich mit der PostgreSQL-Datenbank verbunden!")

	// Erstelle eine Instanz des Service und übergebe die DB-Verbindung
	cookieService := service.New(db)

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
