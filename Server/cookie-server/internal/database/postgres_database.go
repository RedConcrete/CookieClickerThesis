package database

import (
	"database/sql"
	"embed"
	"errors"
	"fmt"
	"log"

	"github.com/golang-migrate/migrate/v4"
	"github.com/golang-migrate/migrate/v4/database/postgres"
	"github.com/golang-migrate/migrate/v4/source/iofs"
	_ "github.com/lib/pq"
)

type PostgresDatabase struct {
	database *sql.DB
}

//go:embed migrations/*
var embeddedDatabaseMigrations embed.FS

// RunMigrations implements Database.
func (p *PostgresDatabase) RunMigrations() error {
	log.Println("running migrations")
	migrationFiles, err := iofs.New(embeddedDatabaseMigrations, "migrations")
	if err != nil {
		return err
	}
	driver, err := postgres.WithInstance(p.database, &postgres.Config{})
	if err != nil {
		return err
	}
	migrations, err := migrate.NewWithInstance(
		"iofs",
		migrationFiles,
		"postgres",
		driver,
	)
	if err != nil {
		return err
	}
	if err := migrations.Up(); err != nil && !errors.Is(err, migrate.ErrNoChange) {
		return err
	}
	log.Println("migrations applied successfully")
	return nil
}

// NewTransaction implements Database.
func (p *PostgresDatabase) NewTransaction() (Transaction, error) {
	transaction, err := p.database.Begin()
	if err != nil {
		return nil, err
	}
	return &PostgresTransaction{
		transaction:         transaction,
		isTransactionActive: true,
	}, nil
}

// Close implements Database.
func (p *PostgresDatabase) Close() error {
	return p.database.Close()
}

func NewPostgresDatabase(host string, port int, user string, password string, databaseName string) (*PostgresDatabase, error) {
	log.Println("connecting to database")
	connectionString := fmt.Sprintf("host=%s port=%v user=%s password=%s dbname=%s sslmode=disable", host, port, user, password, databaseName)
	database, err := sql.Open("postgres", connectionString)
	if err != nil {
		return nil, err
	}
	err = database.Ping()
	if err != nil {
		return nil, err
	}
	log.Println("connected to database")
	return &PostgresDatabase{
		database: database,
	}, nil
}

var _ Database = (*PostgresDatabase)(nil)
