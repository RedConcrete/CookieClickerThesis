package service

import (
	"context"
	"cookie-server/internal/database"
	api "cookie-server/internal/server"
	"sync"
)

type CookieService struct {
	database database.Database
	mux      sync.Mutex
}

func NewCookieService(database database.Database) *CookieService {
	return &CookieService{
		database: database,
	}
}

// MarketsAmountGet implements api.Handler.
func (c *CookieService) MarketsAmountGet(ctx context.Context, params api.MarketsAmountGetParams) ([]api.Market, error) {
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
	panic("unimplemented")
}

// NewError implements api.Handler.
func (c *CookieService) NewError(ctx context.Context, err error) *api.ErrRespStatusCode {
	panic("unimplemented")
}

// UsersGet implements api.Handler.
func (c *CookieService) UsersGet(ctx context.Context) ([]api.User, error) {
	panic("unimplemented")
}

// UsersPost implements api.Handler.
func (c *CookieService) UsersPost(ctx context.Context) (*api.User, error) {
	panic("unimplemented")
}

// UsersUserIdGet implements api.Handler.
func (c *CookieService) UsersUserIdGet(ctx context.Context, params api.UsersUserIdGetParams) (*api.User, error) {
	panic("unimplemented")
}

var _ api.Handler = (*CookieService)(nil)
