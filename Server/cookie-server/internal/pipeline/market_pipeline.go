package internal

import (
	"cookie-server/internal/database"
	api "cookie-server/internal/server"
	"math/rand"
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
			SugarPrice:     rand.ExpFloat64(), // Beispielwert
			FlourPrice:     rand.ExpFloat64(), // Beispielwert
			EggsPrice:      rand.ExpFloat64(), // Beispielwert
			ButterPrice:    rand.ExpFloat64(), // Beispielwert
			ChocolatePrice: rand.ExpFloat64(), // Beispielwert
			MilkPrice:      rand.ExpFloat64(), // Beispielwert
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
