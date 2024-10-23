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


public class WebAPI : MonoBehaviour
{
    public static WebAPI Instance { get; private set; }
    public static Player player;  // Statische Variable für den Player
    public static ulong SteamId;  // Statische Steam-ID des Players
    private AuthTicket authTicket;

    List<Market> marketList;
    private string baseUrl = "http://localhost:3000";
    private GameManager gameManager;
    private int loginScene = 0;

    private void Awake()
    {

        try
        {
            Steamworks.SteamClient.Init(2816100);
            StartCoroutine(AuthenticateUser());
        }
        catch (System.Exception e)
        {
            Debug.LogError(e + " Steam Conection ERROR! ");
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

            GetSteamID();
            StartCoroutine(WebAPI.Instance.GetPlayer(SteamId, true));
            StartCoroutine(WebAPI.Instance.PostPlayer());
        }
        else
        {
            Debug.LogError("Failed to create authentication ticket.");
        }

        yield return null;
    }

    public IEnumerator PostPlayer()
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
                player = JsonConvert.DeserializeObject<Player>(playerJsonData);  // Zuweisung zur statischen Variable
                SceneManager.LoadScene(1);
            }
            else
            {
                Debug.LogError("Received empty response from the server");
            }
        }
        webRequest.Dispose();
    }

    public IEnumerator GetPlayer(string id, bool isLoggingIn)
    {
        string url = $"{baseUrl}/users/{id}";

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
                    string playerJsonData = webRequest.downloadHandler.text;
                    player = JsonConvert.DeserializeObject<Player>(playerJsonData);  // Zuweisung zur statischen Variable
                    if (isLoggingIn)
                    {
                        SceneManager.LoadScene(1);
                        Debug.Log("Login successful" + id);
                    }
                    break;
            }
        }
    }


    public IEnumerator GetPlayer(ulong id, bool isLoggingIn)
    {
        string url = $"{baseUrl}/users/{id}";

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
                    string playerJsonData = webRequest.downloadHandler.text;
                    player = JsonConvert.DeserializeObject<Player>(playerJsonData);  // Zuweisung zur statischen Variable
                    if (isLoggingIn)
                    {
                        SceneManager.LoadScene(1);
                        Debug.Log("Login successful" + id);
                    }
                    break;
            }
        }
    }

    /**
     * 
     *  todo muss angepast werden so, dass der Server alle infos bekommt
     *  die gemacht werden müssen und dann es auf derm Server durchgeführt wird
     * 
    **/
    public IEnumerator UpdatePlayer(string playerId)
    {
        string url = baseUrl + "/updatePlayer";
        byte[] playerData = Encoding.UTF8.GetBytes(playerId);

        UnityWebRequest webRequest = new UnityWebRequest(url, "Put");
        webRequest.uploadHandler = new UploadHandlerRaw(playerData);
        webRequest.SetRequestHeader("Content-Type", "application/json");
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(webRequest.error + " while updating Player");
        }
        webRequest.Dispose();
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

    public IEnumerator PostBuy(string playerId, string rec, int amount)
    {
        MarketRequest marketRequest = new MarketRequest(playerId, rec, amount);
        return DoMarketAction("buy", marketRequest);
    }

    public IEnumerator PostSell(string playerId, string rec, int amount)
    {
        MarketRequest marketRequest = new MarketRequest(playerId, rec, amount);
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
                            JsonUtility.FromJsonOverwrite(playerJsonData, player);
                            gameManager.UpdateRecources();
                        }
                        break;
                }
                Debug.Log(player.cookies);
                gameManager.UpdatePlayerData();
            }
        }
    }

    public List<Market> GetMarket()
    {
        return marketList;
    }
    private void GetSteamID()
    {
        SteamId = Steamworks.SteamClient.SteamId;
    }
}
