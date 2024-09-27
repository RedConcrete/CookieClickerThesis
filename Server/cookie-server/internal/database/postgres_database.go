package database

import (
	_"github.com/lib/pq"
	"database/sql"
	"fmt"
)

type PostgresDatabase struct {
	database *sql.DB
}

// NewTransaction implements Database.
func (p *PostgresDatabase) NewTransaction() (Transaction, error) {
	transaction, err := p.database.Begin()
	if err != nil {
		return nil, err
	}
	return &PostgresTransaction{
		transaction: transaction,
		isTransactionActive: true,
	}, nil
}

// Close implements Database.
func (p *PostgresDatabase) Close() error {
	return p.database.Close()
}

func NewPostgresDatabase(host string, port int, user string, password string, databaseName string) (*PostgresDatabase, error) {
	connectionString := fmt.Sprintf("host=%s port=%v user=%s password=%s dbname=%s sslmode=disable", host, port, user, password, databaseName)
	database, err := sql.Open("postgres", connectionString)
	if err != nil {
		return nil, err
	}
	err = database.Ping()
	if err != nil {
		return nil, err
	}
	defer database.Close()
	return &PostgresDatabase{
		database: database,
	}, nil
}

var _ Database = (*PostgresDatabase)(nil)
