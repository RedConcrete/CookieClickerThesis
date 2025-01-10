package database

import (
	api "cookie-server/internal/server"
)

type Database interface {
	NewTransaction() (Transaction, error)
	Close() error
	RunMigrations() error
}

type Transaction interface {
	Commit() error
	Rollback() error
	GetMarketsByAmount(amount int) ([]api.Market, error)
	GetMarkets() ([]api.Market, error)
	CreateUser(user api.User) (*api.User, error)
	CreateUserWithID(steamid string) (*api.User, error)
	GetUser(steamid string) (*api.User, error)
	GetUsers() ([]api.User, error)
	DoBuyTransaction(steamid string, recourse string, amount int) (*api.User, error)
	DoSellTransaction(steamid string, recourse string, amount int) (*api.User, error)
	UpdateUser(user *api.User) error
	CreateMarket(market *api.Market) error
}
