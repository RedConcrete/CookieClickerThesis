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

// TODO muss aus kvn gemacht werden
func (mg *MarketGenerator) StartGenerator() error {
	for {
		// generate market
		newMarket := api.Market{
			SugarPrice:     float64(200 + rand.Intn(150)),
			FlourPrice:     float64(300 + rand.Intn(150)),
			EggsPrice:      float64(100 + rand.Intn(150)),
			ButterPrice:    float64(400 + rand.Intn(150)),
			ChocolatePrice: float64(500 + rand.Intn(50)),
			MilkPrice:      float64(200 + rand.Intn(150)),
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
