package service

import (
	"context"
	api "cookie-server/internal/server"
	"fmt"
	"log"
	"net/http"
	"sort"
	"sync"
	"time"

	"github.com/google/uuid"
)

type CookieServer struct {
	users   []api.User
	markets []api.Market
	mux     sync.Mutex
}

// BuyPost implements api.Handler.
/*
func (c *CookieServer) BuyPost(ctx context.Context, req *api.MarketRequest) (*api.BuyPostOK, error) {
	// Suchen des Benutzers anhand der ID
	user, err := c.getUserByID(req.User.ID)
	if err != nil {
		return nil, fmt.Errorf("user not found: %v", err)
	}

	// Preis der Ressource vom Markt holen
	market, err := c.getLatestMarket()
	if err != nil {
		return nil, fmt.Errorf("could not get market prices: %v", err)
	}

	// Überprüfen, ob der Benutzer genug Cookies hat, um den Kauf zu tätigen
	price := c.getResourcePrice(market, req.Rec)
	totalCost := float64(req.Amount) * price

	if user.Cookies < totalCost {
		return nil, fmt.Errorf("not enough cookies")
	}

	// Bestimmen, welches Gut gekauft wird und die entsprechenden Ressourcen zu reduzieren
	switch req.Rec {
	case "sugar":
		user.Sugar += float64(req.Amount)
	case "flour":
		user.Flour += float64(req.Amount)
	case "eggs":
		user.Eggs += float64(req.Amount)
	case "butter":
		user.Butter += float64(req.Amount)
	case "chocolate":
		user.Chocolate += float64(req.Amount)
	case "milk":
		user.Milk += float64(req.Amount)
	default:
		return nil, fmt.Errorf("unknown resource: %s", req.Rec)
	}

	// Ziehe die Kosten von den Cookies des Benutzers ab
	user.Cookies -= totalCost

	// Benutzer in der Datenbank oder dem Speicher aktualisieren

	// Rückgabe der Transaktionsdetails
	response := &api.BuyPostOK{
		Ingredient: req.Rec,
		Amount:     req.Amount,
		TotalPrice: totalCost,
	}

	return response, nil
}
*/

// Gibt die gewünschte Anzahl der neuesten Märkte zurück
func (c *CookieServer) MarketsAmountGet(ctx context.Context, params api.MarketsAmountGetParams) ([]api.Market, error) {
	// Eine Kopie der Märkte erstellen, um die Originalreihenfolge beizubehalten
	marketsCopy := make([]api.Market, len(c.markets))
	copy(marketsCopy, c.markets)

	// Märkte chronologisch sortieren (neueste zuerst)
	sort.Slice(marketsCopy, func(i, j int) bool {
		return marketsCopy[i].Date.After(marketsCopy[j].Date) // Sortieren neu -> alt
	})

	var result []api.Market
	// Märkte basierend auf der geforderten Anzahl zurückgeben
	for _, market := range marketsCopy {
		if len(result) >= params.Amount {
			break
		}
		result = append(result, market)
	}

	return result, nil
}

// Gibt alle verfügbaren Märkte zurück
func (c *CookieServer) MarketsGet(ctx context.Context) ([]api.Market, error) {
	c.mux.Lock()
	defer c.mux.Unlock()
	log.Println("Sende alle Märkte")
	return c.markets, nil
}

// Erstellt einen neuen Nutzer mit einer generierten UUID und Standardwerten für Zutaten
func (c *CookieServer) UsersPost(ctx context.Context) (*api.User, error) {
	userID := uuid.New().String()

	user := api.User{
		ID:        userID,
		Cookies:   10,
		Sugar:     10,
		Flour:     10,
		Eggs:      10,
		Butter:    10,
		Chocolate: 10,
		Milk:      10,
	}

	fmt.Printf("Neuer User erstellt: %+v\n", user)

	return &user, nil
}

// Gibt Informationen zu einem bestimmten Nutzer basierend auf der User-ID zurück
func (c *CookieServer) UsersUserIdGet(ctx context.Context, params api.UsersUserIdGetParams) (*api.User, error) {
	var user *api.User
	var err error

	// Umwandeln der User-ID in einen String
	userIdStr := params.UserId.String()

	// Suchen nach dem Nutzer mit der passenden ID
	for i := 0; i < len(c.users) && user == nil; i++ {
		if c.users[i].ID == userIdStr {
			user = &c.users[i]
		}
	}

	// Wenn kein User gefunden wurde, wird ein Fehler zurückgegeben
	if user == nil {
		err = &api.ErrRespStatusCode{
			StatusCode: http.StatusNotFound,
			Response:   fmt.Sprintf("User mit ID: %v nicht gefunden", userIdStr),
		}
	}
	return user, err
}

// Gibt eine Standard-Fehlermeldung zurück
func (c *CookieServer) NewError(ctx context.Context, err error) *api.ErrRespStatusCode {
	c.mux.Lock()
	defer c.mux.Unlock()
	return &api.ErrRespStatusCode{
		StatusCode: http.StatusInternalServerError,
		Response:   "Ein Fehler ist aufgetreten",
	}
}

// Gibt eine Liste aller Nutzer zurück
func (c *CookieServer) UsersGet(ctx context.Context) ([]api.User, error) {
	c.mux.Lock()
	defer c.mux.Unlock()
	log.Println("Sende alle Nutzer")
	return c.users, nil
}

// Erstellt einen neuen Server mit voreingestellten Nutzern und Märkten
func New() *CookieServer {
	return &CookieServer{
		users: []api.User{
			{
				ID:        "06d04635-ebee-4914-86cb-03cf4a6d06b1",
				Cookies:   10,
				Sugar:     10,
				Flour:     10,
				Eggs:      10,
				Butter:    10,
				Chocolate: 10,
				Milk:      10,
			},
			{
				ID:        "aa711da4-e783-4bd4-9260-38afab760b87",
				Cookies:   10,
				Sugar:     10,
				Flour:     10,
				Eggs:      10,
				Butter:    10,
				Chocolate: 10,
				Milk:      10,
			},
		},

		markets: []api.Market{
			{
				ID:             "06d04635-ebee-4914-86cb-03cf4a6d06b2",
				Date:           time.Date(2024, time.August, 30, 14, 0, 0, 0, time.UTC),
				SugarPrice:     500,
				FlourPrice:     800,
				EggsPrice:      200,
				ButterPrice:    100,
				ChocolatePrice: 950,
				MilkPrice:      300,
			},
			{
				ID:             "aa711da4-e783-4bd4-9260-38afab760b86",
				Date:           time.Date(2024, time.September, 1, 10, 0, 0, 0, time.UTC),
				SugarPrice:     550,
				FlourPrice:     780,
				EggsPrice:      250,
				ButterPrice:    120,
				ChocolatePrice: 900,
				MilkPrice:      280,
			},
			{
				ID:             "f8c23c2e-1c3e-4f9f-b345-19d23b46b0d8",
				Date:           time.Date(2024, time.September, 2, 9, 30, 0, 0, time.UTC),
				SugarPrice:     600,
				FlourPrice:     790,
				EggsPrice:      230,
				ButterPrice:    150,
				ChocolatePrice: 940,
				MilkPrice:      310,
			},
			{
				ID:             "e63c77e6-32e2-4f18-bb1a-fd1d0073b1a1",
				Date:           time.Date(2024, time.September, 3, 11, 0, 0, 0, time.UTC),
				SugarPrice:     620,
				FlourPrice:     815,
				EggsPrice:      240,
				ButterPrice:    130,
				ChocolatePrice: 920,
				MilkPrice:      330,
			},
			{
				ID:             "b3eecb3b-1f02-4944-b062-b0a6392e2ae5",
				Date:           time.Date(2024, time.September, 4, 15, 0, 0, 0, time.UTC),
				SugarPrice:     640,
				FlourPrice:     810,
				EggsPrice:      220,
				ButterPrice:    110,
				ChocolatePrice: 910,
				MilkPrice:      340,
			},
			{
				ID:             "1a2e75e8-c619-4851-8f2d-b6f22ae10f84",
				Date:           time.Date(2024, time.September, 5, 8, 0, 0, 0, time.UTC),
				SugarPrice:     650,
				FlourPrice:     830,
				EggsPrice:      200,
				ButterPrice:    140,
				ChocolatePrice: 880,
				MilkPrice:      350,
			},
			{
				ID:             "b8d92c52-9c0b-4617-b1f0-57dff92e43ed",
				Date:           time.Date(2024, time.September, 6, 13, 0, 0, 0, time.UTC),
				SugarPrice:     670,
				FlourPrice:     820,
				EggsPrice:      210,
				ButterPrice:    120,
				ChocolatePrice: 870,
				MilkPrice:      360,
			},
			{
				ID:             "b3eecb3b-1f02-4944-b062-b0a6392e2ae6",
				Date:           time.Date(2024, time.September, 7, 11, 0, 0, 0, time.UTC),
				SugarPrice:     690,
				FlourPrice:     840,
				EggsPrice:      240,
				ButterPrice:    130,
				ChocolatePrice: 860,
				MilkPrice:      370,
			},
			{
				ID:             "99edb2f6-2e3e-4e39-895c-79c751bdf41f",
				Date:           time.Date(2024, time.September, 8, 10, 30, 0, 0, time.UTC),
				SugarPrice:     710,
				FlourPrice:     850,
				EggsPrice:      230,
				ButterPrice:    140,
				ChocolatePrice: 840,
				MilkPrice:      380,
			},
			{
				ID:             "d706b1da-1b4a-4c79-b3ba-1f315b10a227",
				Date:           time.Date(2024, time.September, 9, 12, 0, 0, 0, time.UTC),
				SugarPrice:     720,
				FlourPrice:     860,
				EggsPrice:      260,
				ButterPrice:    150,
				ChocolatePrice: 830,
				MilkPrice:      390,
			},
			{
				ID:             "ff30d9c4-9495-4670-bcb4-1cf41ee6f222",
				Date:           time.Date(2024, time.September, 10, 11, 15, 0, 0, time.UTC),
				SugarPrice:     730,
				FlourPrice:     870,
				EggsPrice:      250,
				ButterPrice:    160,
				ChocolatePrice: 810,
				MilkPrice:      400,
			},
			{
				ID:             "e1cbd50b-46ec-4938-9eb9-47cbf7ec7d68",
				Date:           time.Date(2024, time.September, 11, 10, 0, 0, 0, time.UTC),
				SugarPrice:     740,
				FlourPrice:     880,
				EggsPrice:      270,
				ButterPrice:    170,
				ChocolatePrice: 800,
				MilkPrice:      410,
			},
			{
				ID:             "5f4936aa-e26b-47c2-8b5c-bc6a30e78c81",
				Date:           time.Date(2024, time.September, 12, 8, 30, 0, 0, time.UTC),
				SugarPrice:     750,
				FlourPrice:     890,
				EggsPrice:      280,
				ButterPrice:    180,
				ChocolatePrice: 790,
				MilkPrice:      420,
			},
			{
				ID:             "c37b53c0-b8e5-476b-bef1-7c037fb0199e",
				Date:           time.Date(2024, time.September, 13, 9, 0, 0, 0, time.UTC),
				SugarPrice:     760,
				FlourPrice:     900,
				EggsPrice:      290,
				ButterPrice:    190,
				ChocolatePrice: 780,
				MilkPrice:      430,
			},
			{
				ID:             "c47f1e89-4cba-48f7-8b2d-52b8e9434b8c",
				Date:           time.Date(2024, time.September, 14, 14, 0, 0, 0, time.UTC),
				SugarPrice:     770,
				FlourPrice:     910,
				EggsPrice:      300,
				ButterPrice:    200,
				ChocolatePrice: 770,
				MilkPrice:      440,
			}, {
				ID:             "b8d92c52-9c0b-4617-b1f0-57dff92e43ed",
				Date:           time.Date(2024, time.September, 6, 13, 0, 0, 0, time.UTC),
				SugarPrice:     670,
				FlourPrice:     820,
				EggsPrice:      210,
				ButterPrice:    120,
				ChocolatePrice: 870,
				MilkPrice:      360,
			},
			{
				ID:             "b3eecb3b-1f02-4944-b062-b0a6392e2ae6",
				Date:           time.Date(2024, time.September, 7, 11, 0, 0, 0, time.UTC),
				SugarPrice:     690,
				FlourPrice:     840,
				EggsPrice:      240,
				ButterPrice:    130,
				ChocolatePrice: 860,
				MilkPrice:      370,
			},
			{
				ID:             "99edb2f6-2e3e-4e39-895c-79c751bdf41f",
				Date:           time.Date(2024, time.September, 8, 10, 30, 0, 0, time.UTC),
				SugarPrice:     710,
				FlourPrice:     850,
				EggsPrice:      230,
				ButterPrice:    140,
				ChocolatePrice: 840,
				MilkPrice:      380,
			},
			{
				ID:             "d706b1da-1b4a-4c79-b3ba-1f315b10a227",
				Date:           time.Date(2024, time.September, 9, 12, 0, 0, 0, time.UTC),
				SugarPrice:     720,
				FlourPrice:     860,
				EggsPrice:      260,
				ButterPrice:    150,
				ChocolatePrice: 830,
				MilkPrice:      390,
			},
			{
				ID:             "ff30d9c4-9495-4670-bcb4-1cf41ee6f222",
				Date:           time.Date(2024, time.September, 10, 11, 15, 0, 0, time.UTC),
				SugarPrice:     730,
				FlourPrice:     870,
				EggsPrice:      250,
				ButterPrice:    160,
				ChocolatePrice: 810,
				MilkPrice:      400,
			},
			{
				ID:             "e1cbd50b-46ec-4938-9eb9-47cbf7ec7d68",
				Date:           time.Date(2024, time.September, 11, 10, 0, 0, 0, time.UTC),
				SugarPrice:     740,
				FlourPrice:     880,
				EggsPrice:      270,
				ButterPrice:    170,
				ChocolatePrice: 800,
				MilkPrice:      410,
			},
			{
				ID:             "5f4936aa-e26b-47c2-8b5c-bc6a30e78c81",
				Date:           time.Date(2024, time.September, 12, 8, 30, 0, 0, time.UTC),
				SugarPrice:     750,
				FlourPrice:     890,
				EggsPrice:      280,
				ButterPrice:    180,
				ChocolatePrice: 790,
				MilkPrice:      420,
			},
			{
				ID:             "c37b53c0-b8e5-476b-bef1-7c037fb0199e",
				Date:           time.Date(2024, time.September, 13, 9, 0, 0, 0, time.UTC),
				SugarPrice:     760,
				FlourPrice:     900,
				EggsPrice:      290,
				ButterPrice:    190,
				ChocolatePrice: 780,
				MilkPrice:      430,
			},
			{
				ID:             "c47f1e89-4cba-48f7-8b2d-52b8e9434b8c",
				Date:           time.Date(2024, time.September, 14, 14, 0, 0, 0, time.UTC),
				SugarPrice:     770,
				FlourPrice:     910,
				EggsPrice:      300,
				ButterPrice:    200,
				ChocolatePrice: 770,
				MilkPrice:      440,
			}, {
				ID:             "b8d92c52-9c0b-4617-b1f0-57dff92e43ed",
				Date:           time.Date(2024, time.September, 6, 13, 0, 0, 0, time.UTC),
				SugarPrice:     670,
				FlourPrice:     820,
				EggsPrice:      210,
				ButterPrice:    120,
				ChocolatePrice: 870,
				MilkPrice:      360,
			},
			{
				ID:             "b3eecb3b-1f02-4944-b062-b0a6392e2ae6",
				Date:           time.Date(2024, time.September, 7, 11, 0, 0, 0, time.UTC),
				SugarPrice:     690,
				FlourPrice:     840,
				EggsPrice:      240,
				ButterPrice:    130,
				ChocolatePrice: 860,
				MilkPrice:      370,
			},
			{
				ID:             "99edb2f6-2e3e-4e39-895c-79c751bdf41f",
				Date:           time.Date(2024, time.September, 8, 10, 30, 0, 0, time.UTC),
				SugarPrice:     710,
				FlourPrice:     850,
				EggsPrice:      230,
				ButterPrice:    140,
				ChocolatePrice: 840,
				MilkPrice:      380,
			},
			{
				ID:             "d706b1da-1b4a-4c79-b3ba-1f315b10a227",
				Date:           time.Date(2024, time.September, 9, 12, 0, 0, 0, time.UTC),
				SugarPrice:     720,
				FlourPrice:     860,
				EggsPrice:      260,
				ButterPrice:    150,
				ChocolatePrice: 830,
				MilkPrice:      390,
			},
			{
				ID:             "ff30d9c4-9495-4670-bcb4-1cf41ee6f222",
				Date:           time.Date(2024, time.September, 10, 11, 15, 0, 0, time.UTC),
				SugarPrice:     730,
				FlourPrice:     870,
				EggsPrice:      250,
				ButterPrice:    160,
				ChocolatePrice: 810,
				MilkPrice:      400,
			},
			{
				ID:             "e1cbd50b-46ec-4938-9eb9-47cbf7ec7d68",
				Date:           time.Date(2024, time.September, 11, 10, 0, 0, 0, time.UTC),
				SugarPrice:     740,
				FlourPrice:     880,
				EggsPrice:      270,
				ButterPrice:    170,
				ChocolatePrice: 800,
				MilkPrice:      410,
			},
			{
				ID:             "5f4936aa-e26b-47c2-8b5c-bc6a30e78c81",
				Date:           time.Date(2024, time.September, 12, 8, 30, 0, 0, time.UTC),
				SugarPrice:     750,
				FlourPrice:     890,
				EggsPrice:      280,
				ButterPrice:    180,
				ChocolatePrice: 790,
				MilkPrice:      420,
			},
			{
				ID:             "c37b53c0-b8e5-476b-bef1-7c037fb0199e",
				Date:           time.Date(2024, time.September, 13, 9, 0, 0, 0, time.UTC),
				SugarPrice:     760,
				FlourPrice:     900,
				EggsPrice:      290,
				ButterPrice:    190,
				ChocolatePrice: 780,
				MilkPrice:      430,
			},
			{
				ID:             "c47f1e89-4cba-48f7-8b2d-52b8e9434b8c",
				Date:           time.Date(2024, time.September, 14, 14, 0, 0, 0, time.UTC),
				SugarPrice:     770,
				FlourPrice:     910,
				EggsPrice:      300,
				ButterPrice:    200,
				ChocolatePrice: 770,
				MilkPrice:      440,
			}, {
				ID:             "b8d92c52-9c0b-4617-b1f0-57dff92e43ed",
				Date:           time.Date(2024, time.September, 6, 13, 0, 0, 0, time.UTC),
				SugarPrice:     670,
				FlourPrice:     820,
				EggsPrice:      210,
				ButterPrice:    120,
				ChocolatePrice: 870,
				MilkPrice:      360,
			},
			{
				ID:             "b3eecb3b-1f02-4944-b062-b0a6392e2ae6",
				Date:           time.Date(2024, time.September, 7, 11, 0, 0, 0, time.UTC),
				SugarPrice:     690,
				FlourPrice:     840,
				EggsPrice:      240,
				ButterPrice:    130,
				ChocolatePrice: 860,
				MilkPrice:      370,
			},
			{
				ID:             "99edb2f6-2e3e-4e39-895c-79c751bdf41f",
				Date:           time.Date(2024, time.September, 8, 10, 30, 0, 0, time.UTC),
				SugarPrice:     710,
				FlourPrice:     850,
				EggsPrice:      230,
				ButterPrice:    140,
				ChocolatePrice: 840,
				MilkPrice:      380,
			},
			{
				ID:             "d706b1da-1b4a-4c79-b3ba-1f315b10a227",
				Date:           time.Date(2024, time.September, 9, 12, 0, 0, 0, time.UTC),
				SugarPrice:     720,
				FlourPrice:     860,
				EggsPrice:      260,
				ButterPrice:    150,
				ChocolatePrice: 830,
				MilkPrice:      390,
			},
			{
				ID:             "ff30d9c4-9495-4670-bcb4-1cf41ee6f222",
				Date:           time.Date(2024, time.September, 10, 11, 15, 0, 0, time.UTC),
				SugarPrice:     730,
				FlourPrice:     870,
				EggsPrice:      250,
				ButterPrice:    160,
				ChocolatePrice: 810,
				MilkPrice:      400,
			},
			{
				ID:             "e1cbd50b-46ec-4938-9eb9-47cbf7ec7d68",
				Date:           time.Date(2024, time.September, 11, 10, 0, 0, 0, time.UTC),
				SugarPrice:     740,
				FlourPrice:     880,
				EggsPrice:      270,
				ButterPrice:    170,
				ChocolatePrice: 800,
				MilkPrice:      410,
			},
			{
				ID:             "5f4936aa-e26b-47c2-8b5c-bc6a30e78c81",
				Date:           time.Date(2024, time.September, 12, 8, 30, 0, 0, time.UTC),
				SugarPrice:     750,
				FlourPrice:     890,
				EggsPrice:      280,
				ButterPrice:    180,
				ChocolatePrice: 790,
				MilkPrice:      420,
			},
			{
				ID:             "c37b53c0-b8e5-476b-bef1-7c037fb0199e",
				Date:           time.Date(2024, time.September, 13, 9, 0, 0, 0, time.UTC),
				SugarPrice:     760,
				FlourPrice:     900,
				EggsPrice:      290,
				ButterPrice:    190,
				ChocolatePrice: 780,
				MilkPrice:      430,
			},
			{
				ID:             "c47f1e89-4cba-48f7-8b2d-52b8e9434b8c",
				Date:           time.Date(2024, time.September, 14, 14, 0, 0, 0, time.UTC),
				SugarPrice:     770,
				FlourPrice:     910,
				EggsPrice:      300,
				ButterPrice:    200,
				ChocolatePrice: 770,
				MilkPrice:      440,
			}, {
				ID:             "b8d92c52-9c0b-4617-b1f0-57dff92e43ed",
				Date:           time.Date(2024, time.September, 6, 13, 0, 0, 0, time.UTC),
				SugarPrice:     670,
				FlourPrice:     820,
				EggsPrice:      210,
				ButterPrice:    120,
				ChocolatePrice: 870,
				MilkPrice:      360,
			},
			{
				ID:             "b3eecb3b-1f02-4944-b062-b0a6392e2ae6",
				Date:           time.Date(2024, time.September, 7, 11, 0, 0, 0, time.UTC),
				SugarPrice:     690,
				FlourPrice:     840,
				EggsPrice:      240,
				ButterPrice:    130,
				ChocolatePrice: 860,
				MilkPrice:      370,
			},
			{
				ID:             "99edb2f6-2e3e-4e39-895c-79c751bdf41f",
				Date:           time.Date(2024, time.September, 8, 10, 30, 0, 0, time.UTC),
				SugarPrice:     710,
				FlourPrice:     850,
				EggsPrice:      230,
				ButterPrice:    140,
				ChocolatePrice: 840,
				MilkPrice:      380,
			},
			{
				ID:             "d706b1da-1b4a-4c79-b3ba-1f315b10a227",
				Date:           time.Date(2024, time.September, 9, 12, 0, 0, 0, time.UTC),
				SugarPrice:     720,
				FlourPrice:     860,
				EggsPrice:      260,
				ButterPrice:    150,
				ChocolatePrice: 830,
				MilkPrice:      390,
			},
			{
				ID:             "ff30d9c4-9495-4670-bcb4-1cf41ee6f222",
				Date:           time.Date(2024, time.September, 10, 11, 15, 0, 0, time.UTC),
				SugarPrice:     730,
				FlourPrice:     870,
				EggsPrice:      250,
				ButterPrice:    160,
				ChocolatePrice: 810,
				MilkPrice:      400,
			},
			{
				ID:             "e1cbd50b-46ec-4938-9eb9-47cbf7ec7d68",
				Date:           time.Date(2024, time.September, 11, 10, 0, 0, 0, time.UTC),
				SugarPrice:     740,
				FlourPrice:     880,
				EggsPrice:      270,
				ButterPrice:    170,
				ChocolatePrice: 800,
				MilkPrice:      410,
			},
			{
				ID:             "5f4936aa-e26b-47c2-8b5c-bc6a30e78c81",
				Date:           time.Date(2024, time.September, 12, 8, 30, 0, 0, time.UTC),
				SugarPrice:     750,
				FlourPrice:     890,
				EggsPrice:      280,
				ButterPrice:    180,
				ChocolatePrice: 790,
				MilkPrice:      420,
			},
			{
				ID:             "c37b53c0-b8e5-476b-bef1-7c037fb0199e",
				Date:           time.Date(2024, time.September, 13, 9, 0, 0, 0, time.UTC),
				SugarPrice:     760,
				FlourPrice:     900,
				EggsPrice:      290,
				ButterPrice:    190,
				ChocolatePrice: 780,
				MilkPrice:      430,
			},
			{
				ID:             "c47f1e89-4cba-48f7-8b2d-52b8e9434b8c",
				Date:           time.Date(2024, time.September, 14, 14, 0, 0, 0, time.UTC),
				SugarPrice:     770,
				FlourPrice:     910,
				EggsPrice:      300,
				ButterPrice:    200,
				ChocolatePrice: 770,
				MilkPrice:      440,
			},
		},
	}
}

var _ api.Handler = (*CookieServer)(nil)
