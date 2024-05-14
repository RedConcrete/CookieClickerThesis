using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Text;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.Networking;

public class WebAPI : MonoBehaviour
{

    public WebAPI()
    {
       
    }

    string baseUrl = "https://localhost:44392/api/server";
    public IEnumerator PostPlayer(string player)
    {
        string url = baseUrl + "/createPlayer";
        byte[] playerData = Encoding.UTF8.GetBytes(player);
        UnityWebRequest www = new UnityWebRequest(url, "Post");
        www.uploadHandler = new UploadHandlerRaw(playerData);
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();
        

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error + " while creating Player");
        }
        else
        {
            Debug.Log(www.result + " while creating Player");
            Debug.Log(player);
        }
        www.Dispose();
    }

    public IEnumerator GetPlayer(string id)
    {
        Debug.Log("GetPlayer");
        string url = baseUrl + "/getPlayer?id=" + id;
        UnityWebRequest www = new UnityWebRequest(url, "Get");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error + " while getting Player by ID");
        }
        else
        {
            Player player = JsonUtility.FromJson<Player>(www.downloadHandler.text);
            Debug.Log(player);
            Debug.Log(www.result + " while getting Player by ID");
        }
    }
}
