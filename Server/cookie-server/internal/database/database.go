package database

import (
	api "cookie-server/internal/server"
)

type Database interface {
	NewTransaction() (Transaction, error)
	Close() error
	// todo: migrations?
}

type Transaction interface {
	Commit() error
	Rollback() error
	GetMarketsByAmount(amount int) ([]api.Market, error)
	GetMarkets() ([]api.Market, error)
	CreateUser(user api.User) (*api.User, error)
	GetUser(uuid string) (*api.User, error)
	GetUsers() ([]api.User, error)
}
