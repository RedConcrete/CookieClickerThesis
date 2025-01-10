package service

import (
	"context"
	"cookie-server/internal/database"
	api "cookie-server/internal/server"
	"log"
	"net/http"
	"strconv"
	"sync"
)

type CookieService struct {
	database database.Database
	mux      sync.Mutex
}

// UsersUserIdPost implements api.Handler.
func (c *CookieService) UsersUserIdPost(ctx context.Context, params api.UsersUserIdPostParams) (*api.User, error) {
	log.Println("POST /users/{" + params.UserId + "} called")
	c.mux.Lock()
	defer c.mux.Unlock()
	transaction, err := c.database.NewTransaction()
	if err != nil {
		return nil, err
	}
	defer transaction.Rollback()

	user, err := transaction.CreateUserWithID(api.User{})
	if err != nil {
		return nil, err
	}
	if err := transaction.Commit(); err != nil {
		return nil, err
	}
	return user, nil
}

func NewCookieService(database database.Database) *CookieService {
	return &CookieService{
		database: database,
	}
}

// SellPost implements api.Handler.
func (c *CookieService) SellPost(ctx context.Context, params *api.MarketRequest) (*api.User, error) {
	log.Println("POST /sell/ called")
	c.mux.Lock()
	defer c.mux.Unlock()
	// Erstelle eine neue Datenbanktransaktion
	transaction, err := c.database.NewTransaction()
	if err != nil {
		return nil, err
	}
	defer transaction.Rollback() // Rollback im Fehlerfall
	// Führe die Kauftransaktion durch
	user, err := transaction.DoSellTransaction(params.Steamid.Value, params.Recourse, params.Amount)
	if err != nil {
		return nil, err
	}
	// Transaktion abschließen
	if err := transaction.Commit(); err != nil {
		return nil, err
	}
	// Rückgabe des users
	return user, nil
}

// BuyPost implements api.Handler.
func (c *CookieService) BuyPost(ctx context.Context, params *api.MarketRequest) (*api.User, error) {
	log.Println("POST /buy/ called")
	c.mux.Lock()
	defer c.mux.Unlock()
	// Erstelle eine neue Datenbanktransaktion
	transaction, err := c.database.NewTransaction()
	if err != nil {
		return nil, err
	}
	defer transaction.Rollback() // Rollback im Fehlerfall
	// Führe die Kauftransaktion durch
	user, err := transaction.DoBuyTransaction(params.Steamid.Value, params.Recourse, params.Amount)
	if err != nil {
		return nil, err
	}
	// Transaktion abschließen
	if err := transaction.Commit(); err != nil {
		return nil, err
	}
	// Rückgabe des users
	return user, nil
}

// MarketsAmountGet implements api.Handler.
func (c *CookieService) MarketsAmountGet(ctx context.Context, params api.MarketsAmountGetParams) ([]api.Market, error) {
	log.Println("GET /markets/{" + strconv.Itoa(params.Amount) + "} called")
	c.mux.Lock()
	defer c.mux.Unlock()
	transaction, err := c.database.NewTransaction()
	if err != nil {
		return nil, err
	}
	marketsByAmount, err := transaction.GetMarketsByAmount(params.Amount)
	if err != nil {
		return nil, err
	}
	if err := transaction.Commit(); err != nil {
		return nil, err
	}
	return marketsByAmount, nil
}

// MarketsGet implements api.Handler.
func (c *CookieService) MarketsGet(ctx context.Context) ([]api.Market, error) {
	log.Println("GET /markets called")
	c.mux.Lock()
	defer c.mux.Unlock()
	transaction, err := c.database.NewTransaction()
	if err != nil {
		return nil, err
	}
	markets, err := transaction.GetMarkets()
	if err != nil {
		return nil, err
	}
	if err := transaction.Commit(); err != nil {
		return nil, err
	}
	return markets, nil
}

// NewError implements api.Handler.
func (c *CookieService) NewError(ctx context.Context, err error) *api.ErrRespStatusCode {
	// Beispiel für eine mögliche Implementierung
	println(err.Error())

	return &api.ErrRespStatusCode{
		StatusCode: http.StatusInternalServerError, // Beispiel: Statuscode 500 für interne Serverfehler
		Response:   err.Error(),
	}
}

// UsersGet implements api.Handler.
func (c *CookieService) UsersGet(ctx context.Context) ([]api.User, error) {
	log.Println("GET /users called")
	c.mux.Lock()
	defer c.mux.Unlock()
	transaction, err := c.database.NewTransaction()
	if err != nil {
		return nil, err
	}
	users, err := transaction.GetUsers()
	if err != nil {
		return nil, err
	}
	if err := transaction.Commit(); err != nil {
		return nil, err
	}
	return users, nil
}

// UsersPost implements api.Handler.
func (c *CookieService) UsersPost(ctx context.Context) (*api.User, error) {
	log.Println("POST /users called")
	c.mux.Lock()
	defer c.mux.Unlock()
	transaction, err := c.database.NewTransaction()
	if err != nil {
		return nil, err
	}
	defer transaction.Rollback()

	user, err := transaction.CreateUser(api.User{})
	if err != nil {
		return nil, err
	}
	if err := transaction.Commit(); err != nil {
		return nil, err
	}
	return user, nil
}

// UsersUserIdGet implements api.Handler.
func (c *CookieService) UsersUserIdGet(ctx context.Context, params api.UsersUserIdGetParams) (*api.User, error) {
	c.mux.Lock()
	defer c.mux.Unlock()
	transaction, err := c.database.NewTransaction()
	if err != nil {
		return nil, err
	}
	user, err := transaction.GetUser(api.User{})
	if err != nil {
		return nil, err
	}
	if err := transaction.Commit(); err != nil {
		return nil, err
	}
	return user, nil
}

var _ api.Handler = (*CookieService)(nil)
