package database

import (
	api "cookie-server/internal/server"
	"database/sql"
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
	panic("unimplemented")
}

// GetMarkets implements Transaction.
func (p *PostgresTransaction) GetMarkets() ([]api.Market, error) {
	var marketsByAmount []api.Market
	query := `SELECT "Id", "Date", "SugarPrice", "FlourPrice", "EggsPrice", "ButterPrice", "ChocolatePrice", "MilkPrice"
	          FROM public."Markets"
	          ORDER BY "Date" DESC
	          LIMIT $1`
	rows, err := p.transaction.Query(query)
	if err != nil {
		return nil, err
	}
	defer rows.Close()
	for rows.Next() {
		var market api.Market
		err := rows.Scan(&market)
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
		err := rows.Scan(&market)
		if err != nil {
			return nil, err
		}
		marketsByAmount = append(marketsByAmount, market)
	}
	return marketsByAmount, nil
}

// GetUser implements Transaction.
func (p *PostgresTransaction) GetUser(uuid string) (*api.User, error) {
	var user *api.User
	// Datenbankabfrage zum Abrufen des Benutzers anhand der ID
	query := `SELECT "Id", "Cookies", "Sugar", "Flour", "Eggs", "Butter", "Chocolate", "Milk"
	          FROM public."Players"
	          WHERE "Id" = $1`
	rows, err := p.transaction.Query(query)
	if err != nil {
		return nil, err
	}
	defer rows.Close()
	err = rows.Scan(&user)
	return user, nil
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
		err := rows.Scan(&user)
		if err != nil {
			return nil, err
		}
		users = append(users, user)
	}
	return users, nil
}

var _ Transaction = (*PostgresTransaction)(nil)

/*


// BuyPost implements api.Handler.

func (c *CookieServer) BuyPost(ctx context.Context, req *api.MarketRequest) (*api.BuyPostOK, error) {
	// Suchen des Benutzers anhand der ID
	user, err := c.getUserByID(req.User.ID)
	if err != nil {
		return nil, fmt.Errorf("user not found: %v", err)
	}

	// Preis der Ressource vom Markt holen
	market, err := c.getLatestMarket()
	if err != nil {
		return nil, fmt.Errorf("could not get market prices: %v", err)
	}

	// Überprüfen, ob der Benutzer genug Cookies hat, um den Kauf zu tätigen
	price := c.getResourcePrice(market, req.Rec)
	totalCost := float64(req.Amount) * price

	if user.Cookies < totalCost {
		return nil, fmt.Errorf("not enough cookies")
	}

	// Bestimmen, welches Gut gekauft wird und die entsprechenden Ressourcen zu reduzieren
	switch req.Rec {
	case "sugar":
		user.Sugar += float64(req.Amount)
	case "flour":
		user.Flour += float64(req.Amount)
	case "eggs":
		user.Eggs += float64(req.Amount)
	case "butter":
		user.Butter += float64(req.Amount)
	case "chocolate":
		user.Chocolate += float64(req.Amount)
	case "milk":
		user.Milk += float64(req.Amount)
	default:
		return nil, fmt.Errorf("unknown resource: %s", req.Rec)
	}

	// Ziehe die Kosten von den Cookies des Benutzers ab
	user.Cookies -= totalCost

	// Benutzer in der Datenbank oder dem Speicher aktualisieren

	// Rückgabe der Transaktionsdetails
	response := &api.BuyPostOK{
		Ingredient: req.Rec,
		Amount:     req.Amount,
		TotalPrice: totalCost,
	}

	return response, nil
}


// Gibt die gewünschte Anzahl der neuesten Märkte zurück
func (c *CookieServer) MarketsAmountGet(ctx context.Context, params api.MarketsAmountGetParams) ([]api.Market, error) {
	// SQL-Abfrage: Märkte nach Datum sortieren (neueste zuerst) und die gewünschte Anzahl zurückgeben
	query := `SELECT "Id", "Date", "SugarPrice", "FlourPrice", "EggsPrice", "ButterPrice", "ChocolatePrice", "MilkPrice"
	          FROM public."Markets"
	          ORDER BY "Date" DESC
	          LIMIT $1`

	// Abfrage mit Limit ausführen (params.Amount gibt die Anzahl der gewünschten Märkte an)
	rows, err := c.db.QueryContext(ctx, query, params.Amount)
	if err != nil {
		return nil, fmt.Errorf("Fehler beim Abrufen der Märkte: %v", err)
	}
	defer rows.Close()

	var markets []api.Market

	// Ergebniszeilen durchlaufen und die Märkte in die Liste einfügen
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
			return nil, fmt.Errorf("Fehler beim Scannen des Marktes: %v", err)
		}
		markets = append(markets, market)
	}

	// Fehler während der Iteration prüfen
	if err = rows.Err(); err != nil {
		return nil, fmt.Errorf("Fehler während der Zeileniteration: %v", err)
	}

	return markets, nil
}

// Gibt alle verfügbaren Märkte zurück
func (c *CookieServer) MarketsGet(ctx context.Context) ([]api.Market, error) {
	c.mux.Lock()
	defer c.mux.Unlock()

	// SQL-Abfrage zum Abrufen aller Märkte
	query := `SELECT "Id", "Date", "SugarPrice", "FlourPrice", "EggsPrice", "ButterPrice", "ChocolatePrice", "MilkPrice"
	          FROM public."Markets"`

	// Abfrage ausführen
	rows, err := c.db.QueryContext(ctx, query)
	if err != nil {
		return nil, fmt.Errorf("Fehler beim Abrufen der Märkte: %v", err)
	}
	defer rows.Close()

	var markets []api.Market

	// Ergebniszeilen durchlaufen und die Märkte in die Liste einfügen
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
			return nil, fmt.Errorf("Fehler beim Scannen des Marktes: %v", err)
		}
		markets = append(markets, market)
	}

	// Fehler während der Iteration prüfen
	if err = rows.Err(); err != nil {
		return nil, fmt.Errorf("Fehler während der Zeileniteration: %v", err)
	}

	// Wenn keine Märkte gefunden wurden
	if len(markets) == 0 {
		log.Println("Keine Märkte in der Datenbank gefunden")
	}

	return markets, nil
}

*/