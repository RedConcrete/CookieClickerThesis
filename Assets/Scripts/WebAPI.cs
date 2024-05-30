using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.Networking;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class WebAPI : MonoBehaviour
{
    public static WebAPI Instance { get; private set; }
    public string Id = Guid.NewGuid().ToString();

    private Player player;
    private Market market;
    private OwnSceneManager ownSceneManager = new OwnSceneManager();
    private string baseUrl = "https://localhost:44392/api/server";

    private void Awake()
    {
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
            Debug.Log(webRequest.result + " while creating Player");
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

    public IEnumerator UpdatePlayer(string player)
    {
        string url = baseUrl + "/updatePlayer";
        byte[] playerData = Encoding.UTF8.GetBytes(player);

        UnityWebRequest webRequest = new UnityWebRequest(url, "Put");
        webRequest.uploadHandler = new UploadHandlerRaw(playerData);
        webRequest.SetRequestHeader("Content-Type", "application/json");
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(webRequest.error + " while updating Player");
        }
        else
        {
            Debug.Log(player);
        }
        webRequest.Dispose();
    }

    public IEnumerator CheckServer()
    {
        Debug.Log("CheckServer");
        string url = baseUrl + "/";
        UnityWebRequest www = new UnityWebRequest(url, "Get");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error + " No Connection to Server found!");
        }
        else
        {
            Debug.Log(www.result + " Server is Online!");
        }
    }

    public IEnumerator GetPrices()
    {
        string url = baseUrl + "/getMarket";

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
                    Debug.Log(marketJsonData);
                    JsonUtility.FromJsonOverwrite(marketJsonData, market = new Market());
                    break;
            }
        }
    }

    public IEnumerator PostBuy()
    {
        string url = baseUrl + "/createPlayer";
        byte[] playerData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(player = new Player()));

        UnityWebRequest webRequest = new UnityWebRequest(url, "Post");
        webRequest.uploadHandler = new UploadHandlerRaw(playerData);
        webRequest.SetRequestHeader("Content-Type", "application/json");
        yield return webRequest.SendWebRequest();

        webRequest.Dispose();
    }
    public IEnumerator PostSell()
    {
        string url = baseUrl + "/createPlayer";
        byte[] playerData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(player = new Player()));

        UnityWebRequest webRequest = new UnityWebRequest(url, "Post");
        webRequest.uploadHandler = new UploadHandlerRaw(playerData);
        webRequest.SetRequestHeader("Content-Type", "application/json");
        yield return webRequest.SendWebRequest();

        webRequest.Dispose();
    }

    public Player GetLoginPlayer()
    {
        return player;
    }
}
