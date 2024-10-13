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
			SugarPrice:     float64(rand.Intn(1000) + 1),
			FlourPrice:     float64(rand.Intn(1000) + 1),
			EggsPrice:      float64(rand.Intn(1000) + 1),
			ButterPrice:    float64(rand.Intn(1000) + 1),
			ChocolatePrice: float64(rand.Intn(1000) + 1),
			MilkPrice:      float64(rand.Intn(1000) + 1),
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
