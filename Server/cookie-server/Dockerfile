FROM golang:1.23 as build
WORKDIR /go/src/app
COPY . .
RUN make setup
RUN CGO_ENABLED=0 go build -o /go/bin/app

FROM gcr.io/distroless/static-debian12
COPY --from=build /go/bin/app /
EXPOSE 3000
CMD ["/app"]