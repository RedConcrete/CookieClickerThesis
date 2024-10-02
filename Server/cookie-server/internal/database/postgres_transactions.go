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

// UpdateUser implements Transaction.
func (p *PostgresTransaction) UpdateUser(user *api.User) error {
	query := `UPDATE public."Players"
              SET "Cookies" = $1, "Sugar" = $2, "Flour" = $3, "Eggs" = $4,
                  "Butter" = $5, "Chocolate" = $6, "Milk" = $7
              WHERE "Id" = $8`

	_, err := p.transaction.Exec(query, user.Cookies, user.Sugar, user.Flour, user.Eggs, user.Butter, user.Chocolate, user.Milk, user.ID)
	return err
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

// DoBuyTransaction implements Transaction.
func (p *PostgresTransaction) DoBuyTransaction(uuid, recourse string, amount int) (*api.User, error) {
	var user api.User

	// Datenbankabfrage zum Abrufen des Benutzers anhand der ID
	query := `SELECT "Id", "Cookies", "Sugar", "Flour", "Eggs", "Butter", "Chocolate", "Milk"
	          FROM public."Players"
	          WHERE "Id" = $1`

	// Führt die Abfrage aus
	row := p.transaction.QueryRow(query, uuid)

	// Scannt die Werte in die entsprechende Benutzerstruktur
	err := row.Scan(
		&user.ID,
		&user.Cookies,
		&user.Sugar,
		&user.Flour,
		&user.Eggs,
		&user.Butter,
		&user.Chocolate,
		&user.Milk)
	if err != nil {
		if err == sql.ErrNoRows {
			return nil, fmt.Errorf("user with id %s not found", uuid)
		}
		return nil, err
	}

	// Datenbankabfrage zum Abrufen des letzten Marktpreises
	marketQuery := `SELECT "SugarPrice", "FlourPrice", "EggsPrice", "ButterPrice", "ChocolatePrice", "MilkPrice"
                    FROM public."Markets"
                    ORDER BY "Date" DESC
                    LIMIT 1`

	var sugarPrice, flourPrice, eggsPrice, butterPrice, chocolatePrice, milkPrice float64

	if err := p.transaction.QueryRow(marketQuery).Scan(&sugarPrice, &flourPrice, &eggsPrice, &butterPrice, &chocolatePrice, &milkPrice); err != nil {
		return nil, err
	}

	// Preis basierend auf der Ressource auswählen
	var totalPrice float64
	switch recourse {
	case "sugar":
		totalPrice = sugarPrice * float64(amount)
		user.Sugar += float64(amount)
	case "flour":
		totalPrice = flourPrice * float64(amount)
		user.Flour += float64(amount)
	case "eggs":
		totalPrice = eggsPrice * float64(amount)
		user.Eggs += float64(amount)
	case "butter":
		totalPrice = butterPrice * float64(amount)
		user.Butter += float64(amount)
	case "chocolate":
		totalPrice = chocolatePrice * float64(amount)
		user.Chocolate += float64(amount)
	case "milk":
		totalPrice = milkPrice * float64(amount)
		user.Milk += float64(amount)
	default:
		return nil, fmt.Errorf("recourse %s not found", recourse)
	}

	// Überprüfe, ob der Benutzer genügend Cookies hat
	if user.Cookies < totalPrice {
		return nil, fmt.Errorf("not enough cookies for purchase")
	}

	// Ziehe den Preis von den Cookies ab
	user.Cookies -= totalPrice

	// Aktualisiere den Benutzer in der Datenbank
	if err := p.UpdateUser(&user); err != nil {
		return nil, err
	}

	return &user, nil
}

// DoSellTransaction implements Transaction.
func (p *PostgresTransaction) DoSellTransaction(uuid string, recourse string, amount int) (*api.User, error) {
	var user api.User

	// Datenbankabfrage zum Abrufen des Benutzers anhand der ID
	query := `SELECT "Id", "Cookies", "Sugar", "Flour", "Eggs", "Butter", "Chocolate", "Milk"
	          FROM public."Players"
	          WHERE "Id" = $1`

	// Führt die Abfrage aus
	row := p.transaction.QueryRow(query, uuid)

	// Scannt die Werte in die entsprechende Benutzerstruktur
	err := row.Scan(
		&user.ID,
		&user.Cookies,
		&user.Sugar,
		&user.Flour,
		&user.Eggs,
		&user.Butter,
		&user.Chocolate,
		&user.Milk)
	if err != nil {
		if err == sql.ErrNoRows {
			return nil, fmt.Errorf("user with id %s not found", uuid)
		}
		return nil, err
	}

	// Datenbankabfrage zum Abrufen des letzten Marktpreises
	marketQuery := `SELECT "SugarPrice", "FlourPrice", "EggsPrice", "ButterPrice", "ChocolatePrice", "MilkPrice"
                    FROM public."Markets"
                    ORDER BY "Date" DESC
                    LIMIT 1`

	var sugarPrice, flourPrice, eggsPrice, butterPrice, chocolatePrice, milkPrice float64

	if err := p.transaction.QueryRow(marketQuery).Scan(&sugarPrice, &flourPrice, &eggsPrice, &butterPrice, &chocolatePrice, &milkPrice); err != nil {
		return nil, err
	}

	// Preis basierend auf der Ressource auswählen
	var totalPrice float64
	switch recourse {
	case "sugar":
		if user.Sugar < float64(amount) {
			return nil, fmt.Errorf("not enough sugar to sell")
		}
		totalPrice = sugarPrice * float64(amount)
		user.Sugar -= float64(amount)
	case "flour":
		if user.Flour < float64(amount) {
			return nil, fmt.Errorf("not enough flour to sell")
		}
		totalPrice = flourPrice * float64(amount)
		user.Flour -= float64(amount)
	case "eggs":
		if user.Eggs < float64(amount) {
			return nil, fmt.Errorf("not enough eggs to sell")
		}
		totalPrice = eggsPrice * float64(amount)
		user.Eggs -= float64(amount)
	case "butter":
		if user.Butter < float64(amount) {
			return nil, fmt.Errorf("not enough butter to sell")
		}
		totalPrice = butterPrice * float64(amount)
		user.Butter -= float64(amount)
	case "chocolate":
		if user.Chocolate < float64(amount) {
			return nil, fmt.Errorf("not enough chocolate to sell")
		}
		totalPrice = chocolatePrice * float64(amount)
		user.Chocolate -= float64(amount)
	case "milk":
		if user.Milk < float64(amount) {
			return nil, fmt.Errorf("not enough milk to sell")
		}
		totalPrice = milkPrice * float64(amount)
		user.Milk -= float64(amount)
	default:
		return nil, fmt.Errorf("recourse %s not found", recourse)
	}

	// Füge den Erlös in Cookies hinzu
	user.Cookies += totalPrice

	// Aktualisiere den Benutzer in der Datenbank
	if err := p.UpdateUser(&user); err != nil {
		return nil, err
	}

	return &user, nil
}

var _ Transaction = (*PostgresTransaction)(nil)
