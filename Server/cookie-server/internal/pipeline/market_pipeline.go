package internal

import (
	"context"
	"cookie-server/internal/database"
	"database/sql"
	"log"
	"time"

	_ "github.com/lib/pq" // PostgreSQL Driver
)

type PostgresTransaction struct {
	transaction         *sql.Tx
	isTransactionActive bool
}

// Market repräsentiert ein Marktobjekt
type Market struct {
	Id             string
	Date           time.Time
	SugarPrice     float64
	FlourPrice     float64
	EggsPrice      float64
	ButterPrice    float64
	ChocolatePrice float64
	MilkPrice      float64
}

func StartPipeline(ctx context.Context, db database.Database, interval time.Duration) {
	marketChan := make(chan *Market)

	tx := &PostgresTransaction{} // Erstelle ein PostgresTransaction-Objekt

	go MarketGenerator(ctx, marketChan, db, interval)

	for market := range marketChan {
		log.Println("Neues Marktobjekt:", market)
		if _, err := tx.saveMarketToDB(db, market); err != nil {
			log.Printf("Fehler beim Speichern des Marktes: %v", err)
		}
	}
}

// MarketGenerator erzeugt Marktobjekte in einem bestimmten Intervall
func MarketGenerator(ctx context.Context, out chan<- *Market, db database.Database, interval time.Duration) {
	for {
		select {
		case <-ctx.Done():
			close(out)
			return
		default:
			newMarket := &Market{
				SugarPrice:     1.0, // Beispielwert
				FlourPrice:     2.0, // Beispielwert
				EggsPrice:      3.0, // Beispielwert
				ButterPrice:    4.0, // Beispielwert
				ChocolatePrice: 5.0, // Beispielwert
				MilkPrice:      6.0, // Beispielwert
			}

			out <- newMarket

			time.Sleep(interval)
		}
	}
}

// saveMarketToDB speichert das Marktobjekt in der Datenbank
func (p *PostgresTransaction) saveMarketToDB(db database.Database, market *Market) (*Market, error) {
	query := `INSERT INTO public."Markets" ("Id", "Date", "SugarPrice", "FlourPrice", "EggsPrice", "ButterPrice", "ChocolatePrice", "MilkPrice") 
			  VALUES (gen_random_uuid(), NOW(), $1, $2, $3, $4, $5, $6)
			  RETURNING "Id", "Date", "SugarPrice", "FlourPrice", "EggsPrice", "ButterPrice", "ChocolatePrice", "MilkPrice"`

	err := p.transaction.QueryRow(query,
		market.SugarPrice,
		market.FlourPrice,
		market.EggsPrice,
		market.ButterPrice,
		market.ChocolatePrice,
		market.MilkPrice).
		Scan(
			&market.Id,
			&market.Date,
			&market.SugarPrice,
			&market.FlourPrice,
			&market.EggsPrice,
			&market.ButterPrice,
			&market.ChocolatePrice,
			&market.MilkPrice)

	if err != nil {
		return nil, err
	}

	return market, nil
}
