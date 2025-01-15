using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserMarketData
{
    public UserMarketDataUser user;
    public List<UserMarketDataMarket> markets;
}

[System.Serializable]
public class UserMarketDataUser
{
    public string steamid;
    public float cookies;
    public float sugar;
    public float flour;
    public float eggs;
    public float butter;
    public float chocolate;
    public float milk;
}

[System.Serializable]
public class UserMarketDataMarket
{
    public string id;
    public DateTime date;
    public float sugarPrice;
    public float flourPrice;
    public float eggsPrice;
    public float butterPrice;
    public float chocolatePrice;
    public float milkPrice;
}
