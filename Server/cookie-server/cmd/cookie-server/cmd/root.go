package cmd

import (
	"context"
	service "cookie-server/internal"
	"cookie-server/internal/database"
	"cookie-server/internal/middleware"
	internal "cookie-server/internal/pipeline"
	api "cookie-server/internal/server"
	"crypto/tls"
	"embed"
	"fmt"
	"log"
	"net/http"
	"time"

	"github.com/spf13/cobra"
	"github.com/spf13/pflag"
	"github.com/spf13/viper"
)

//go:embed certs/server.crt certs/server.key
var certFS embed.FS

var dbHost string
var dbPort int
var dbUser string
var dbPassword string
var dbName string

var rootCmd = &cobra.Command{
	Use:   "cookie-server",
	Short: "cookie-server", // todo: add description short
	Long:  "cookie-server", // todo: add description long
	Run: func(cmd *cobra.Command, args []string) {
		_, err := middleware.SetupOTelSDK(context.Background())
		if err != nil {
			log.Fatalf("could not initialize otl	p setup %v", err)
		}

		database, err := database.NewPostgresDatabase(dbHost, dbPort, dbUser, dbPassword, dbName)
		if err != nil {
			log.Fatalf("could not connect to the database: %v", err)
			return
		}
		defer database.Close()
		if err := database.RunMigrations(); err != nil {
			log.Fatal(err.Error())
			return
		}
		cookieService := service.NewCookieService(database)
		mg := internal.NewMarketGenerator(database, 10*time.Second)
		go mg.StartGenerator()

		// Erstelle den API-Server mit dem Service
		srv, err := api.NewServer(cookieService)
		if err != nil {
			log.Fatal(err)
			return
		}

		log.Println("starting server on port: 3000")
		// Load the embedded certificates

		certData, err := certFS.ReadFile("certs/server.crt")
		if err != nil {
			log.Fatal(err)
			return
		}
		keyData, err := certFS.ReadFile("certs/server.key")
		if err != nil {
			log.Fatal(err)
			return
		}

		// Create a TLS certificate
		cert, err := tls.X509KeyPair(certData, keyData)
		if err != nil {
			log.Fatal(err)
			return
		}

		// Configure the HTTPS server
		server := &http.Server{
			Addr:    ":3000",
			Handler: srv,
			TLSConfig: &tls.Config{
				Certificates: []tls.Certificate{cert},
			},
		}

		// Starte den HTTPS-Server
		if err := server.ListenAndServeTLS("", ""); err != nil {
			log.Fatal(err)
			return
		}
	},
}

func Execute() {
	if err := rootCmd.Execute(); err != nil {
		log.Fatal(err)
	}
}

func init() {
	cobra.OnInitialize(initConfig)
	rootCmd.PersistentFlags().StringVar(
		&dbHost,
		"db-host",
		"localhost",
		"postgres database hostname for storing data",
	)
	rootCmd.PersistentFlags().IntVar(
		&dbPort,
		"db-port",
		5460,
		"postgres database port for storing data",
	)
	rootCmd.PersistentFlags().StringVar(
		&dbUser,
		"db-user",
		"postgres",
		"postgres user to manage the data to store",
	)
	rootCmd.PersistentFlags().StringVar(
		&dbPassword,
		"db-password",
		"1234",
		"postgres password to access the database",
	)
	rootCmd.PersistentFlags().StringVar(
		&dbName,
		"db-name",
		"CookieData",
		"postgres database location for storing data",
	)
}

func initConfig() {
	rootCmd.PersistentFlags().VisitAll(func(f *pflag.Flag) {
		if !f.Changed && viper.IsSet(f.Name) {
			if err := rootCmd.PersistentFlags().Set(f.Name, fmt.Sprint(viper.Get(f.Name))); err != nil {
				log.Fatalf("unable to set value for command line parameter: %v", err)
			}
		}
	})
}
