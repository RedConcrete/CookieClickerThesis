using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Server.Data;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;


public class WebAPI : MonoBehaviour
{
    public static WebAPI Instance { get; private set; }
    public string Id = Guid.NewGuid().ToString();

    private Player player;
    List<Market> marketList;
    private OwnSceneManager ownSceneManager = new OwnSceneManager();
    private string baseUrl = "https://localhost:44392/api/server";
    private GameManager gameManager;
    private Scene scene;
    private int loginScene = 0;

    private void Awake()
    {
        scene = SceneManager.GetActiveScene();
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
        if (gameManager == null && scene.buildIndex != loginScene)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
    }

    public IEnumerator PostPlayer()
    {
        string url = baseUrl + "/createPlayer";
        byte[] playerData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(player = new Player()));

        UnityWebRequest webRequest = new UnityWebRequest(url, "Post");
        webRequest.uploadHandler = new UploadHandlerRaw(playerData);
        webRequest.SetRequestHeader("Content-Type", "application/json");
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(webRequest.error + " while creating Player");
        }
        else
        {
            //Debug.Log(webRequest.result + " while creating Player");
            ownSceneManager.SwitchScene(1);
        }
        webRequest.Dispose();
    }

    public IEnumerator GetPlayer(string id)
    {
        string url = baseUrl + "/getPlayer?id=" + id;

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
                    Debug.Log(playerJsonData);
                    JsonUtility.FromJsonOverwrite(playerJsonData, player = new Player());
                    ownSceneManager.SwitchScene(1);
                    break;
            }
        }
    }

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
        yield return DoMarketAction("buy", marketRequest);
    }

    public IEnumerator PostSell(string playerId, string rec, int amount)
    {
        MarketRequest marketRequest = new MarketRequest(playerId, rec, amount);
        yield return DoMarketAction("sell", marketRequest);
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
