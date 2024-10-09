package internal

import (
	"cookie-server/internal/database"
	api "cookie-server/internal/server"
	"time"
)

type MarketGenerator struct {
	database database.Database
	interval time.Duration
}

func NewMarketGenerator(database database.Database, interval time.Duration) *MarketGenerator {
	return &MarketGenerator{
		database: database,
		interval: interval,
	}
}

func (mg *MarketGenerator) StartGenerator() error {
	for {
		// generate market
		newMarket := api.Market{
			SugarPrice:     1.0, // Beispielwert
			FlourPrice:     2.0, // Beispielwert
			EggsPrice:      3.0, // Beispielwert
			ButterPrice:    4.0, // Beispielwert
			ChocolatePrice: 5.0, // Beispielwert
			MilkPrice:      6.0, // Beispielwert
		}

		// save to database
		transaction, err := mg.database.NewTransaction()
		if err != nil {
			return err
		}
		if err := transaction.CreateMarket(&newMarket); err != nil {
			transaction.Rollback()
			return err
		}
		if err := transaction.Commit(); err != nil {
			return err
		}
		time.Sleep(mg.interval)
	}
}
