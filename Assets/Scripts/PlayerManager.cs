using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviour
{
    private Player p = new Player();
    private WebAPI webAPI = new WebAPI();
    private TMP_InputField playerTextField;


    private void Start()
    {
        playerTextField = GameObject.Find("LoginField").GetComponent<TMP_InputField>();
        string pData = JsonUtility.ToJson(p);
        StartCoroutine(webAPI.PostPlayer(pData));
    }


   public void Login()
    {
        Debug.Log(playerTextField.text);
        StartCoroutine(webAPI.GetPlayer(playerTextField.text));
    }
}
