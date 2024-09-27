package database

import (
	api "cookie-server/internal/server"
	"database/sql"
	"fmt"

	"github.com/google/uuid"
)

type PostgresTransaction struct {
	transaction         *sql.Tx
	isTransactionActive bool
}

// Rollback implements Transaction.
func (p *PostgresTransaction) Rollback() error {
	if p.isTransactionActive {
		p.isTransactionActive = false
		return p.transaction.Rollback()
	}
	return nil
}

// Commit implements Transaction.
func (p *PostgresTransaction) Commit() error {
	if p.isTransactionActive {
		p.isTransactionActive = false
		return p.transaction.Commit()
	}
	return nil
}

// CreateUser implements Transaction.
func (p *PostgresTransaction) CreateUser(user api.User) (*api.User, error) {
	// SQL-Abfrage zum Einfügen eines neuen Benutzers in die "Players"-Tabelle
	query := `INSERT INTO public."Players" ("Id", "Cookies", "Sugar", "Flour", "Eggs", "Butter", "Chocolate", "Milk")
			  VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
			  RETURNING "Id", "Cookies", "Sugar", "Flour", "Eggs", "Butter", "Chocolate", "Milk"`

	if user.ID == "" {
		user.ID = uuid.New().String()
	}

	// Führt die Abfrage aus und scannt die zurückgegebenen Werte in das Benutzerobjekt
	err := p.transaction.QueryRow(query, user.ID, user.Cookies, user.Sugar, user.Flour, user.Eggs, user.Butter, user.Chocolate, user.Milk).
		Scan(
			&user.ID,
			&user.Cookies,
			&user.Sugar,
			&user.Flour,
			&user.Eggs,
			&user.Butter,
			&user.Chocolate,
			&user.Milk)
	if err != nil {
		return nil, err
	}

	return &user, nil
}

// GetMarkets implements Transaction.
func (p *PostgresTransaction) GetMarkets() ([]api.Market, error) {
	var marketsByAmount []api.Market
	query := `SELECT "Id", "Date", "SugarPrice", "FlourPrice", "EggsPrice", "ButterPrice", "ChocolatePrice", "MilkPrice"
	          FROM public."Markets"
	          ORDER BY "Date" DESC`

	rows, err := p.transaction.Query(query)
	if err != nil {
		return nil, err
	}
	defer rows.Close()
	for rows.Next() {
		var market api.Market
		err := rows.Scan(
			&market.ID,
			&market.Date,
			&market.SugarPrice,
			&market.FlourPrice,
			&market.EggsPrice,
			&market.ButterPrice,
			&market.ChocolatePrice,
			&market.MilkPrice,
		)
		if err != nil {
			return nil, err
		}
		marketsByAmount = append(marketsByAmount, market)
	}
	return marketsByAmount, nil
}

// GetMarketsByAmount implements Transaction.
func (p *PostgresTransaction) GetMarketsByAmount(amount int) ([]api.Market, error) {
	var marketsByAmount []api.Market
	query := `SELECT "Id", "Date", "SugarPrice", "FlourPrice", "EggsPrice", "ButterPrice", "ChocolatePrice", "MilkPrice"
	          FROM public."Markets"
	          ORDER BY "Date" DESC
	          LIMIT $1`
	rows, err := p.transaction.Query(query, amount)
	if err != nil {
		return nil, err
	}
	defer rows.Close()
	for rows.Next() {
		var market api.Market
		err := rows.Scan(
			&market.ID,
			&market.Date,
			&market.SugarPrice,
			&market.FlourPrice,
			&market.EggsPrice,
			&market.ButterPrice,
			&market.ChocolatePrice,
			&market.MilkPrice,
		)
		if err != nil {
			return nil, err
		}
		marketsByAmount = append(marketsByAmount, market)
	}
	return marketsByAmount, nil
}

// GetUser implements Transaction.
func (p *PostgresTransaction) GetUser(uuid string) (*api.User, error) {
	var user api.User

	// Datenbankabfrage zum Abrufen des Benutzers anhand der ID
	query := `SELECT "Id", "Cookies", "Sugar", "Flour", "Eggs", "Butter", "Chocolate", "Milk"
	          FROM public."Players"
	          WHERE "Id" = $1`

	// Führt die Abfrage aus
	rows, err := p.transaction.Query(query, uuid)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	// Wechsle zur ersten Zeile des Ergebnisses
	if rows.Next() {
		// Scannt die Werte in die entsprechende Benutzerstruktur
		err = rows.Scan(
			&user.ID,
			&user.Cookies,
			&user.Sugar,
			&user.Flour,
			&user.Eggs,
			&user.Butter,
			&user.Chocolate,
			&user.Milk)
		if err != nil {
			return nil, err
		}
	} else {
		// Falls keine Zeile gefunden wurde
		return nil, fmt.Errorf("user with id %s not found", uuid)
	}

	return &user, nil
}

// GetUsers implements Transaction.
func (p *PostgresTransaction) GetUsers() ([]api.User, error) {
	var users []api.User
	query := `SELECT "Id", "Cookies", "Sugar", "Flour", "Eggs", "Butter", "Chocolate", "Milk"
	          FROM public."Players"`
	rows, err := p.transaction.Query(query)
	if err != nil {
		return nil, err
	}
	defer rows.Close()
	for rows.Next() {
		var user api.User
		err := rows.Scan(
			&user.ID,
			&user.Cookies,
			&user.Sugar,
			&user.Flour,
			&user.Eggs,
			&user.Butter,
			&user.Chocolate,
			&user.Milk,
		)
		if err != nil {
			return nil, err
		}
		users = append(users, user)
	}
	return users, nil
}

var _ Transaction = (*PostgresTransaction)(nil)
