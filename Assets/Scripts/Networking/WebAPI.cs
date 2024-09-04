using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Server.Data;
using UnityEngine.SceneManagement;


public class WebAPI : MonoBehaviour
{
    public static WebAPI Instance { get; private set; }
    public string Id = Guid.NewGuid().ToString();

    private Player player;
    List<Market> marketList;
    private OwnSceneManager ownSceneManager = new OwnSceneManager();
    private string oldBaseUrl = "https://localhost:5000/api/server";
    private string baseUrl = "http://localhost:5000";
    //private string baseUrl = "https://66ad-5-146-99-178.ngrok-free.app/api/server";
    private GameManager gameManager;
    private int loginScene = 0;
    private int scene;

    private void Awake()
    {
        scene = SceneManager.GetActiveScene().buildIndex;
        Debug.Log(scene);
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
        if (gameManager == null && scene != loginScene)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        scene = SceneManager.GetActiveScene().buildIndex;
    }

    public IEnumerator PostPlayer()
    {
        string url = baseUrl + "/CreatePlayer";

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
                player = JsonConvert.DeserializeObject<Player>(playerJsonData);
                ownSceneManager.SwitchScene(1);
            }
            else
            {
                Debug.LogError("Received empty response from the server");
            }
        }
        webRequest.Dispose();
    }

    public IEnumerator GetPlayer(string id)
    {
        string url = $"{baseUrl}/GetPlayer/{id}";

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
                    player = JsonConvert.DeserializeObject<Player>(playerJsonData);
                    ownSceneManager.SwitchScene(1);
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
        string url = baseUrl + "/getMarket?amountToGet=" + amount;

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

    public Player GetLoginPlayer()
    {
        return player;
    }

    public List<Market> GetMarket()
    {
        return marketList;
    }
}
