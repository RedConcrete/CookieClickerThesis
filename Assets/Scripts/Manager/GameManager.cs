using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("ResourceFields:")]
    public TMP_Text cookie_Text;
    public TMP_Text sugar_Text;
    public TMP_Text flour_Text;
    public TMP_Text eggs_Text;
    public TMP_Text butter_Text;
    public TMP_Text chocolate_Text;
    public TMP_Text milk_Text;

    [Header("AmountField:")]
    public TMP_InputField amount_InputField;

    private Player p;
    private string pData;
    private OwnSceneManager ownSceneManager = new OwnSceneManager();

    private void Start()
    {
        p = WebAPI.Instance.GetLoginPlayer();
        UpdateRecources();
    }

    public void UpdateRecources()
    {
        cookie_Text.text = p.Cookies.ToString();
        sugar_Text.text = p.Sugar.ToString();
        flour_Text.text = p.Flour.ToString();
        eggs_Text.text = p.Eggs.ToString();
        butter_Text.text = p.Butter.ToString();
        chocolate_Text.text = p.Chocolate.ToString();
        milk_Text.text = p.Milk.ToString();
    }

    public void UpdatePlayerData()
    {
        pData = JsonUtility.ToJson(p);
        StartCoroutine(WebAPI.Instance.UpdatePlayer(pData));
    }

    public void ProduceCookies()
    {
        if (int.TryParse(amount_InputField.text, out int amount))
        {
            if (p.Sugar >= 10 * amount && p.Flour >= 10 * amount && p.Eggs >= 10 * amount && p.Butter >= 10 * amount && p.Chocolate >= 10 * amount && p.Milk >= 10 * amount)
            {
                p.Cookies = p.Cookies + (100 * amount);
                p.Sugar = p.Sugar - 10 * amount;
                p.Flour = p.Flour - 10 * amount;
                p.Eggs = p.Eggs - 10 * amount;
                p.Butter = p.Butter - 10 * amount;
                p.Chocolate = p.Chocolate - 10 * amount;
                p.Milk = p.Milk - 10 * amount;
                UpdateRecources();
                UpdatePlayerData();
            }
            else
            {
                Debug.Log("Not enough Resources");
            }
        }
        else
        {
            Debug.LogError($"Attempted conversion of {amount_InputField.text} failed.");
        }
    }

    public void Logout()
    {
        ownSceneManager.SwitchScene(0);
    }
}
