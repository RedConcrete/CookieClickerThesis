CREATE TABLE IF NOT EXISTS "kvn" (
    "id" TEXT PRIMARY KEY,
    "date" DATE NOT NULL,
    "sugar_kv" INTEGER NOT NULL DEFAULT 0,
    "sugar_n" INTEGER NOT NULL DEFAULT 0,
    "flour_kv" INTEGER NOT NULL DEFAULT 0,
    "flour_n" INTEGER NOT NULL DEFAULT 0,
    "eggs_kv" INTEGER NOT NULL DEFAULT 0,
    "eggs_n" INTEGER NOT NULL DEFAULT 0,
    "butter_kv" INTEGER NOT NULL DEFAULT 0,
    "butter_n" INTEGER NOT NULL DEFAULT 0,
    "chocolate_kv" INTEGER NOT NULL DEFAULT 0,
    "chocolate_n" INTEGER NOT NULL DEFAULT 0,
    "milk_kv" INTEGER NOT NULL DEFAULT 0,
    "milk_n" INTEGER NOT NULL DEFAULT 0
);

-- CREATE TABLE IF NOT EXISTS "transaction" (
--     "id" TEXT PRIMARY KEY,
--     "date" DATE NOT NULL,
--     "resource" TEXT NOT NULL,
--     "price" FLOAT NOT NULL DEFAULT 0.0,
--     "amount" INTEGER NOT NULL DEFAULT 0,
--     "total" FLOAT NOT NULL DEFAULT 0.0,
--     "buy" BOOLEAN NOT NULL DEFAULT 1
-- );

CREATE TABLE IF NOT EXISTS "markets" (
    "id" TEXT PRIMARY KEY,
    "date" DATE NOT NULL,
    "sugar_price" FLOAT NOT NULL DEFAULT 0.0,
    "flour_price" FLOAT NOT NULL DEFAULT 0.0,
    "eggs_price" FLOAT NOT NULL DEFAULT 0.0,
    "butter_price" FLOAT NOT NULL DEFAULT 0.0,
    "chocolate_price" FLOAT NOT NULL DEFAULT 0.0,
    "milk_price" FLOAT NOT NULL DEFAULT 0.0
);

CREATE TABLE IF NOT EXISTS "players" (
    "steamid" TEXT PRIMARY KEY,
    "cookies" FLOAT NOT NULL DEFAULT 0.0,
    "sugar" FLOAT NOT NULL DEFAULT 0.0,
    "flour" FLOAT NOT NULL DEFAULT 0.0,
    "eggs" FLOAT NOT NULL DEFAULT 0.0,
    "butter" FLOAT NOT NULL DEFAULT 0.0,
    "chocolate" FLOAT NOT NULL DEFAULT 0.0,
    "milk" FLOAT NOT NULL DEFAULT 0.0
);