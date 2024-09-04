package service

import (
	"context"
	api "cookie-server/internal/server"
	"fmt"
	"log"
	"net/http"
	"sync"
)

type CookieServer struct {
	users []api.User
	mux   sync.Mutex
}

// UsersUserIdGet implements api.Handler.
func (c *CookieServer) UsersUserIdGet(ctx context.Context, params api.UsersUserIdGetParams) (*api.User, error) {
	var user *api.User
	var err error
	for i:=0; i<len(c.users) && user == nil; i++ {
		if c.users[i].ID == int(params.UserId) {
			user = &c.users[i]
		}
	}
	if user == nil {
		err = &api.ErrRespStatusCode{
			StatusCode: http.StatusNotFound,
			Response: fmt.Sprintf("player with id: %v not found", params.UserId),
		}
	}
	return user, err
}

// NewError implements api.Handler.
func (c *CookieServer) NewError(ctx context.Context, err error) *api.ErrRespStatusCode {
	c.mux.Lock()
	defer c.mux.Unlock()
	return &api.ErrRespStatusCode{
		StatusCode: http.StatusInternalServerError,
		Response:   "Mimimimi",
	}
}

// UsersGet implements api.Handler.
func (c *CookieServer) UsersGet(ctx context.Context) ([]api.User, error) {
	c.mux.Lock()
	defer c.mux.Unlock()
	log.Println("yolo get users")
	return c.users, nil
}


func New() *CookieServer {
	return &CookieServer{
		users: []api.User{
			api.User{
				ID:   1,
				Name: "Adolf",
			},
			api.User{
				ID:   2,
				Name: "Adolfine",
			},
		},
	}
}

var _ api.Handler = (*CookieServer)(nil)
