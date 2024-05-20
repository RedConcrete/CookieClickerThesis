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
        cookie_Text.text = p.cookies.ToString();
        sugar_Text.text = p.sugar.ToString();
        flour_Text.text = p.flour.ToString();
        eggs_Text.text = p.eggs.ToString();
        butter_Text.text = p.butter.ToString();
        chocolate_Text.text = p.chocolate.ToString();
        milk_Text.text = p.milk.ToString();
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
            if (p.sugar >= 10 * amount && p.flour >= 10 * amount && p.eggs >= 10 * amount && p.butter >= 10 * amount && p.chocolate >= 10 * amount && p.milk >= 10 * amount)
            {
                p.cookies = p.cookies + (100 * amount);
                p.sugar = p.sugar - 10 * amount;
                p.flour = p.flour - 10 * amount;
                p.eggs = p.eggs - 10 * amount;
                p.butter = p.butter - 10 * amount;
                p.chocolate = p.chocolate - 10 * amount;
                p.milk = p.milk - 10 * amount;
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
