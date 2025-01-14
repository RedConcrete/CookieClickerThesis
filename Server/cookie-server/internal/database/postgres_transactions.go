package database

import (
	api "cookie-server/internal/server"
	"database/sql"
	"fmt"
	"log"
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

// CreateUserWithID implements Transaction.
func (p *PostgresTransaction) CreateUserWithID(steamid string) (*api.User, error) {
	log.Println("POST /users/{" + steamid + "} called")
	var user api.User
	// SQL-Abfrage zum Einfügen eines neuen Benutzers in die "Players"-Tabelle
	query := `INSERT INTO "players" ("steamid", "cookies", "sugar", "flour", "eggs", "butter", "chocolate", "milk")
			  VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
			  ON CONFLICT ("steamid") DO NOTHING
			  RETURNING "steamid", "cookies", "sugar", "flour", "eggs", "butter", "chocolate", "milk"`
	// Führt die Abfrage aus und scannt die zurückgegebenen Werte in das Benutzerobjekt
	err := p.transaction.QueryRow(query, steamid, user.Cookies, user.Sugar, user.Flour, user.Eggs, user.Butter, user.Chocolate, user.Milk).
		Scan(
			&user.Steamid,
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

// Nicht in nutzung !!!!!
func (p *PostgresTransaction) CreateUser(user api.User) (*api.User, error) {

	// SQL-Abfrage zum Einfügen eines neuen Benutzers in die "Players"-Tabelle
	query := `INSERT INTO "players" ("steamid", "cookies", "sugar", "flour", "eggs", "butter", "chocolate", "milk")
			  VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
			  ON CONFLICT ("steamid") DO NOTHING
			  RETURNING "steamid", "cookies", "sugar", "flour", "eggs", "butter", "chocolate", "milk"`

	// Führt die Abfrage aus und scannt die zurückgegebenen Werte in das Benutzerobjekt
	err := p.transaction.QueryRow(query, user.Steamid, user.Cookies, user.Sugar, user.Flour, user.Eggs, user.Butter, user.Chocolate, user.Milk).
		Scan(
			&user.Steamid,
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
	log.Println("updating user")
	// SQL-Abfrage zum Aktualisieren des Benutzers in der "Players"-Tabelle
	query := `UPDATE "players"
              SET "cookies" = $1, "sugar" = $2, "flour" = $3, "eggs" = $4,
                  "butter" = $5, "chocolate" = $6, "milk" = $7
              WHERE "id" = $8`
	_, err := p.transaction.Exec(query, user.Cookies, user.Sugar, user.Flour, user.Eggs, user.Butter, user.Chocolate, user.Milk, user.Steamid)
	return err
}

// GetUser implements Transaction.
func (p *PostgresTransaction) GetUser(steamid string) (*api.User, error) {
	log.Println("GET /users/{" + steamid + "} called")

	var user api.User

	// Datenbankabfrage zum Abrufen des Benutzers anhand der ID
	query := `SELECT "steamid", "cookies", "sugar", "flour", "eggs", "butter", "chocolate", "milk"
	          FROM "players"
	          WHERE "steamid" = $1`
	// Führt die Abfrage aus
	rows, err := p.transaction.Query(query, steamid)

	if err != nil {
		return nil, err
	}
	defer rows.Close()

	// Wechsle zur ersten Zeile des Ergebnisses
	if rows.Next() {
		// Scannt die Werte in die entsprechende Benutzerstruktur
		err = rows.Scan(
			&user.Steamid,
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
		// Benutzer existiert nicht, erstelle ihn mit Standardwerten
		query := `INSERT INTO "players" ("steamid", "cookies", "sugar", "flour", "eggs", "butter", "chocolate", "milk")
			  VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
			  RETURNING "steamid", "cookies", "sugar", "flour", "eggs", "butter", "chocolate", "milk"`
		// Führt die Abfrage aus und scannt die zurückgegebenen Werte in das Benutzerobjekt
		err := p.transaction.QueryRow(query, steamid, user.Cookies, user.Sugar, user.Flour, user.Eggs, user.Butter, user.Chocolate, user.Milk).
			Scan(
				&user.Steamid,
				&user.Cookies,
				&user.Sugar,
				&user.Flour,
				&user.Eggs,
				&user.Butter,
				&user.Chocolate,
				&user.Milk)
		if err != nil {
			return nil, fmt.Errorf("failed to create user with id %s: %w", steamid, err)
		}

		// Benutzer erneut abfragen, um die Daten zurückzugeben
		return p.GetUser(steamid)
	}
	return &user, nil
}

// GetUsers implements Transaction.
func (p *PostgresTransaction) GetUsers() ([]api.User, error) {
	log.Println("getting users")
	var users []api.User
	query := `SELECT "steamid", "cookies", "sugar", "flour", "eggs", "butter", "chocolate", "milk"
	          FROM "players"`
	rows, err := p.transaction.Query(query)
	if err != nil {
		return nil, err
	}
	defer rows.Close()
	for rows.Next() {
		var user api.User
		err := rows.Scan(
			&user.Steamid,
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
	log.Println("getting markets")
	var marketsByAmount []api.Market
	query := `SELECT "id", "date", "sugar_price", "flour_price", "eggs_price", "butter_price", "chocolate_price", "milk_price"
	          FROM "markets"
	          ORDER BY "date" DESC`
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
	log.Printf("GET /market/{%d}", amount)

	var marketsByAmount []api.Market

	query := `SELECT "id", "date", "sugar_price", "flour_price", "eggs_price", "butter_price", "chocolate_price", "milk_price"
	          FROM "markets"
	          ORDER BY "date" DESC
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
func (p *PostgresTransaction) DoBuyTransaction(uuid string, recourse string, amount int) (*api.User, error) {
	log.Println("buying resources")
	var user api.User
	// Datenbankabfrage zum Abrufen des Benutzers anhand der ID
	query := `SELECT "steamid", "cookies", "sugar", "flour", "eggs", "butter", "chocolate", "milk"
	          FROM "players"
	          WHERE "steamid" = $1`
	// Führt die Abfrage aus
	row := p.transaction.QueryRow(query, uuid)
	// Scannt die Werte in die entsprechende Benutzerstruktur
	err := row.Scan(
		&user.Steamid,
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
	marketQuery := `SELECT "sugar_price", "flour_price", "eggs_price", "butter_price", "chocolate_price", "milk_price"
                    FROM "markets"
                    ORDER BY "date" DESC
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
	log.Println("selling resources")
	var user api.User
	// Datenbankabfrage zum Abrufen des Benutzers anhand der ID
	query := `SELECT "steamid", "cookies", "sugar", "flour", "eggs", "butter", "chocolate", "milk"
	          FROM "players"
	          WHERE "steamid" = $1`
	// Führt die Abfrage aus
	row := p.transaction.QueryRow(query, uuid)
	// Scannt die Werte in die entsprechende Benutzerstruktur
	err := row.Scan(
		&user.Steamid,
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
	marketQuery := `SELECT "sugar_price", "flour_price", "eggs_price", "butter_price", "chocolate_price", "milk_price"
                    FROM "markets"
                    ORDER BY "date" DESC
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

// CreateMarket implements Transaction.
func (p *PostgresTransaction) CreateMarket(market *api.Market) error {
	//log.Printf("CREATE /market/")

	query := `INSERT INTO "markets" ("id", "date", "sugar_price", "flour_price", "eggs_price", "butter_price", "chocolate_price", "milk_price") 
			  VALUES (gen_random_uuid(), NOW(), $1, $2, $3, $4, $5, $6)
			  RETURNING "id", "date", "sugar_price", "flour_price", "eggs_price", "butter_price", "chocolate_price", "milk_price"`

	p.transaction.QueryRow(query,
		market.SugarPrice,
		market.FlourPrice,
		market.EggsPrice,
		market.ButterPrice,
		market.ChocolatePrice,
		market.MilkPrice)
	return nil
}

func (p *PostgresTransaction) GetUserMarketData(steamid string, marketAmount int) (*api.UserMarketData, error) {
	log.Printf("GET /update/{%s} and his Marketdata (limit: %d) called", steamid, marketAmount)

	var user api.User
	var markets []api.Market

	// SQL-Abfrage, um die Benutzerdaten abzurufen
	userQuery := `
		SELECT "steamid", "cookies", "sugar", "flour", "eggs", "butter", "chocolate", "milk"
		FROM "players"
		WHERE "steamid" = $1`
	err := p.transaction.QueryRow(userQuery, steamid).Scan(
		&user.Steamid,
		&user.Cookies,
		&user.Sugar,
		&user.Flour,
		&user.Eggs,
		&user.Butter,
		&user.Chocolate,
		&user.Milk)
	if err != nil {
		return nil, fmt.Errorf("error fetching user: %w", err)
	}

	// SQL-Abfrage, um die gewünschte Anzahl an Marktdaten abzurufen
	marketQuery := `
		SELECT "id", "date", "sugar_price", "flour_price", "eggs_price", "butter_price", "chocolate_price", "milk_price"
		FROM "markets"
		ORDER BY "date" DESC
		LIMIT $1`
	rows, err := p.transaction.Query(marketQuery, marketAmount)
	if err != nil {
		return nil, fmt.Errorf("error fetching markets: %w", err)
	}
	defer rows.Close()

	// Marktdaten in eine Liste einfügen
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
			&market.MilkPrice)
		if err != nil {
			return nil, fmt.Errorf("error scanning market data: %w", err)
		}
		markets = append(markets, market)
	}

	// Überprüfen, ob es Fehler während der Iteration gab
	if err := rows.Err(); err != nil {
		return nil, fmt.Errorf("error processing market rows: %w", err)
	}
	var marketItems []api.UserMarketDataMarketsItem

	// Umwandlung der Markets in den richtigen Typ
	for _, market := range markets {
		marketItem := api.UserMarketDataMarketsItem{
			ID:             market.ID,
			Date:           market.Date,
			SugarPrice:     market.SugarPrice,
			FlourPrice:     market.FlourPrice,
			EggsPrice:      market.EggsPrice,
			ButterPrice:    market.ButterPrice,
			ChocolatePrice: market.ChocolatePrice,
			MilkPrice:      market.MilkPrice,
		}
		marketItems = append(marketItems, marketItem)
	}

	// Erstelle das UserMarketData-Objekt
	userMarketData := api.UserMarketData{
		User: api.UserMarketDataUser{
			Steamid:   user.Steamid,
			Cookies:   user.Cookies,
			Sugar:     user.Sugar,
			Flour:     user.Flour,
			Eggs:      user.Eggs,
			Butter:    user.Butter,
			Chocolate: user.Chocolate,
			Milk:      user.Milk,
		},
		Markets: marketItems, // Umgewandelte Märkte
	}

	return &userMarketData, nil
}

var _ Transaction = (*PostgresTransaction)(nil)
