package internal

import (
	"cookie-server/internal/database"
	api "cookie-server/internal/server"
	"log"
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
		log.Println("generate new market")
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

// type PostgresTransaction struct {
// 	transaction         *sql.Tx
// 	isTransactionActive bool
// }

// func StartPipeline(ctx context.Context, db database.Database, interval time.Duration) {
// 	marketChan := make(chan *api.Market)

// 	tx := &PostgresTransaction{} // Erstelle ein PostgresTransaction-Objekt

// 	go MarketGenerator(ctx, marketChan, db, interval)

// 	for market := range marketChan {
// 		log.Println("Neues Marktobjekt:", market)
// 		if _, err := tx.saveMarketToDB(db, market); err != nil {
// 			log.Printf("Fehler beim Speichern des Marktes: %v", err)
// 		}
// 	}
// }

// // MarketGenerator erzeugt Marktobjekte in einem bestimmten Intervall
// func MarketGenerator(ctx context.Context, out chan<- *api.Market, db database.Database, interval time.Duration) {
// 	for {
// 		select {
// 		case <-ctx.Done():
// 			close(out)
// 			return
// 		default:
// 			newMarket := &api.Market{
// 				SugarPrice:     1.0, // Beispielwert
// 				FlourPrice:     2.0, // Beispielwert
// 				EggsPrice:      3.0, // Beispielwert
// 				ButterPrice:    4.0, // Beispielwert
// 				ChocolatePrice: 5.0, // Beispielwert
// 				MilkPrice:      6.0, // Beispielwert
// 			}

// 			out <- newMarket

// 			time.Sleep(interval)
// 		}
// 	}
// }

// // saveMarketToDB speichert das Marktobjekt in der Datenbank
// func (p *PostgresTransaction) saveMarketToDB(db database.Database, market *api.Market) (*api.Market, error) {
// 	query := `INSERT INTO public."markets" ("id", "date", "sugar_price", "flour_price", "eggs_price", "butter_price", "chocolate_price", "milk_price")
// 			  VALUES (gen_random_uuid(), NOW(), $1, $2, $3, $4, $5, $6)
// 			  RETURNING "id", "date", "sugar_price", "flour_price", "eggs_price", "butter_price", "chocolate_price", "milk_price"`

// 	err := p.transaction.QueryRow(query,
// 		market.SugarPrice,
// 		market.FlourPrice,
// 		market.EggsPrice,
// 		market.ButterPrice,
// 		market.ChocolatePrice,
// 		market.MilkPrice).
// 		Scan(
// 			&market.ID,
// 			&market.Date,
// 			&market.SugarPrice,
// 			&market.FlourPrice,
// 			&market.EggsPrice,
// 			&market.ButterPrice,
// 			&market.ChocolatePrice,
// 			&market.MilkPrice)

// 	if err != nil {
// 		return nil, err
// 	}

// 	return market, nil
// }
