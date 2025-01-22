using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Server.Data;
using UnityEngine.SceneManagement;
using Steamworks;
using Unity.VisualScripting;
using UnityEngine.SocialPlatforms;
using UnityEditor.PackageManager;
using System.Security.Cryptography;
using Unity.Mathematics;


public class WebAPI : MonoBehaviour
{
    public static WebAPI Instance { get; private set; }
    public static User user;  // Statische Variable f�r den Player
    public static ulong SteamId;  // Statische Steam-ID des Players
    private AuthTicket authTicket;

    List<Market> marketList;
    //private string baseUrl = "http://cookie-server.r3dconcrete.de:3000";
    private string baseUrl = "http://localhost:3000";
    private GameManager gameManager;
    private int loginScene = 0;
    private int loginLoadTime = 5;

    private void Awake()
    {
        try
        {
            Steamworks.SteamClient.Init(2816100);

            StartCoroutine(AuthenticateUser());

        }
        catch (System.Exception e)
        {
            Debug.LogError(e + " Steam connection ERROR! ");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        if (Instance != null)
        {
            Debug.Log("Destroying duplicate WebAPI instance.");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        Steamworks.SteamClient.RunCallbacks();

        if (gameManager == null && SceneManager.GetActiveScene().buildIndex != loginScene)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
    }

    private void OnApplicationQuit()
    {
        if (authTicket != null)
        {
            Steamworks.SteamUser.EndAuthSession(Steamworks.SteamClient.SteamId);
        }
        Steamworks.SteamClient.Shutdown();
    }

    private IEnumerator AuthenticateUser()
    {
        // Holen eines Authentifizierungstickets
        authTicket = Steamworks.SteamUser.GetAuthSessionTicket();

        if (authTicket != null)
        {
            Debug.Log("Successfully created authentication ticket.");
            byte[] ticketData = authTicket.Data;
            string base64Ticket = Convert.ToBase64String(ticketData);

            Debug.Log("SteamID: " + GetSteamID());

            // Warten für 1 Sekunde
            yield return new WaitForSeconds(loginLoadTime);
            StartCoroutine(WebAPI.Instance.GetPlayer(SteamId.ToString(), true));
        }
        else
        {
            Debug.LogError("Failed to create authentication ticket.");
        }

        yield return null;
    }

    public IEnumerator PostPlayer()
    {
        if (authTicket != null)
        {
            string url = $"{baseUrl}/users/{SteamId}";

            UnityWebRequest webRequest = new UnityWebRequest(url, "POST");
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {webRequest.error}");
                Debug.LogError($"Response Code: {webRequest.responseCode}");
                Debug.LogError($"Response: {webRequest.downloadHandler.text}");
            }
            else
            {
                string playerJsonData = webRequest.downloadHandler.text;
                if (!string.IsNullOrEmpty(playerJsonData))
                {
                    user = JsonConvert.DeserializeObject<User>(playerJsonData);  // Zuweisung zur statischen Variable
                    SceneManager.LoadScene(1);
                }
                else
                {
                    Debug.LogError("Received empty response from the server");
                }
            }
            webRequest.Dispose();
        }
        else
        {
            Debug.LogError("No Steam connection");
        }
    }

    public IEnumerator GetPlayer(string steamid, bool isLoggingIn)
    {
        if (authTicket != null)
        {
            string url = $"{baseUrl}/users/{steamid}";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        Debug.LogError(String.Format("ERROR " + webRequest.error));
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#endif
                        Application.Quit();
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(String.Format("ERROR " + webRequest.error));

                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(String.Format("ERROR " + webRequest.error));

                        break;
                    case UnityWebRequest.Result.Success:
                        string playerJsonData = webRequest.downloadHandler.text;
                        user = JsonConvert.DeserializeObject<User>(playerJsonData);  // Zuweisung zur statischen Variable
                        if (isLoggingIn)
                        {
                            SceneManager.LoadScene(1);
                            Debug.Log("Login successful with: " + steamid);
                        }
                        break;
                }
            }
        }
        else
        {
            Debug.LogError("No Steam connection");
        }
    }

    /**
     *
     * 
    **/
    public IEnumerator UpdatePlayerAndMarket(string steamid, int amount)
    {
        string url = baseUrl + "/update/" + steamid + "?amount=" + amount;
        byte[] playerData = Encoding.UTF8.GetBytes(steamid);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("ERROR: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    string marketJsonData = webRequest.downloadHandler.text;

                    // Deserialisiere das UserMarketData-Objekt
                    UserMarketData userMarketData = JsonConvert.DeserializeObject<UserMarketData>(marketJsonData);

                    user = new User
                    {
                        steamid = userMarketData.user.steamid,
                        cookies = (int)userMarketData.user.cookies,
                        sugar = (int)userMarketData.user.sugar,
                        flour = (int)userMarketData.user.flour,
                        eggs = (int)userMarketData.user.eggs,
                        butter = (int)userMarketData.user.butter,
                        chocolate = (int)userMarketData.user.chocolate,
                        milk = (int)userMarketData.user.milk,
                    };

                    // Benutze die neue Markt-Liste
                    List<Market> newMarketList = new List<Market>();

                    // Märkte in die newMarketList konvertieren
                    foreach (var userMarket in userMarketData.markets)
                    {
                        Market market = new Market
                        {
                            Id = userMarket.id,
                            date = userMarket.date,
                            sugarPrice = userMarket.sugarPrice,
                            flourPrice = userMarket.flourPrice,
                            eggsPrice = userMarket.eggsPrice,
                            butterPrice = userMarket.butterPrice,
                            chocolatePrice = userMarket.chocolatePrice,
                            milkPrice = userMarket.milkPrice
                        };

                        newMarketList.Add(market);
                    }

                    // Weise newMarketList zu marketList zu
                    marketList = newMarketList;
                    break;
            }
        }
    }


    public IEnumerator GetPrices(int amount)
    {
        string url = baseUrl + "/markets/" + amount;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(String.Format("ERROR", webRequest.error));
                    break;
                case UnityWebRequest.Result.Success:
                    string marketJsonData = webRequest.downloadHandler.text;
                    marketList = null;
                    marketList = JsonConvert.DeserializeObject<List<Market>>(marketJsonData);
                    break;
            }
        }
    }

    public IEnumerator PostBuy(string steamid, string rec, int amount)
    {
        MarketRequest marketRequest = new MarketRequest(steamid, rec, amount);
        return DoMarketAction("buy", marketRequest);
    }

    public IEnumerator PostSell(string steamid, string rec, int amount)
    {
        MarketRequest marketRequest = new MarketRequest(steamid, rec, amount);
        return DoMarketAction("sell", marketRequest);
    }

    private IEnumerator DoMarketAction(string action, MarketRequest marketRequest)
    {
        {
            string url = baseUrl + "/" + action;
            string json = JsonUtility.ToJson(marketRequest);

            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                yield return webRequest.SendWebRequest();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError($"ERROR: {webRequest.error}");
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError($"HTTP Error: {webRequest.error}");
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log("Received: " + webRequest.downloadHandler.text);
                        string playerJsonData = webRequest.downloadHandler.text;
                        if (string.IsNullOrEmpty(playerJsonData))
                        {
                            Debug.LogError("Received empty response from the server");
                        }
                        else
                        {
                            Debug.Log(playerJsonData);
                            JsonUtility.FromJsonOverwrite(playerJsonData, user);
                            gameManager.UpdateRecources();
                        }
                        break;
                }
                Debug.Log("Player has: " + user.cookies + " Cookies");
                gameManager.UpdateMarketDataAndUserData();
            }
        }
    }

    public List<Market> GetMarket()
    {
        return marketList;
    }

    public ulong GetSteamID()
    {
        SteamId = Steamworks.SteamClient.SteamId;
        return SteamId;
    }

}
