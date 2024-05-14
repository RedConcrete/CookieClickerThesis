using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

public class LoginManager : MonoBehaviour
{
    private Player p;
    private string pData;
    private WebAPI webAPI = new WebAPI();
    private OwnSceneManager ownSceneManager;
    public TMP_InputField playerTextField;

    private void Start()
    {
        ownSceneManager = GetComponent<OwnSceneManager>();
    }

   public void Login()
    {
        Debug.Log(playerTextField.text);
        if(playerTextField.text != "")
        {
            StartCoroutine(webAPI.GetPlayer(playerTextField.text));
            ownSceneManager.SwitchScene(1);
        }
        else
        {
            Debug.Log("Player ID is empty");
        }
    }
    public void Register()
    {
        p = new Player();
        pData = JsonUtility.ToJson(p);
        StartCoroutine(webAPI.PostPlayer(pData));
        ownSceneManager.SwitchScene(1);
    }
}
