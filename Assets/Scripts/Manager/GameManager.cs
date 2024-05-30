using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

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

    [Header("Timer:")]
    public TMP_Text updateTime_Text;
    public float updateTime = 60;
    public float timeRemaining = 60;
    public bool timerIsRunning = false;


    [Header("AmountField:")]
    public TMP_InputField amountCookies_InputField;
    public TMP_InputField amountCookiesBuyAndSell_InputField;
    public TMP_InputField amountRecBuyAndSell_InputField;

    [Header("ProduceAmount:")]
    public int sugarIncreaseAmount = 1;
    public int flourIncreaseAmount = 1;
    public int eggsIncreaseAmount = 1;
    public int butterIncreaseAmount = 1;
    public int chocolateIncreaseAmount = 1;
    public int milkIncreaseAmount = 1;

    private Player p;
    private string pData;
    private int initialAmount = 1;
    private int currentCreateCookiesAmount = 1;
    private int currentBuyAndSellCookiesAmount = 1;
    private int currentBuyAndSellRecAmount = 1;
    private OwnSceneManager ownSceneManager = new OwnSceneManager();

    private void Start()
    {
        amountCookies_InputField.text = initialAmount.ToString();
        amountCookiesBuyAndSell_InputField.text = initialAmount.ToString();
        amountRecBuyAndSell_InputField.text = initialAmount.ToString();
        timerIsRunning = true;
        p = WebAPI.Instance.GetLoginPlayer();
        UpdateRecources();
    }

    private void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                timeRemaining = updateTime;
                StartCoroutine(WebAPI.Instance.GetPrices());
                UpdatePlayerData();
            }
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        updateTime_Text.text = string.Format("{0:00}:{1:00}", minutes, seconds);
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
        if (p.sugar >= 10 * currentCreateCookiesAmount && p.flour >= 10 * currentCreateCookiesAmount && p.eggs >= 10 * currentCreateCookiesAmount && p.butter >= 10 * currentCreateCookiesAmount && p.chocolate >= 10 * currentCreateCookiesAmount && p.milk >= 10 * currentCreateCookiesAmount)
        {
            p.cookies = p.cookies + (100 * currentCreateCookiesAmount);
            p.sugar = p.sugar - 10 * currentCreateCookiesAmount;
            p.flour = p.flour - 10 * currentCreateCookiesAmount;
            p.eggs = p.eggs - 10 * currentCreateCookiesAmount;
            p.butter = p.butter - 10 * currentCreateCookiesAmount;
            p.chocolate = p.chocolate - 10 * currentCreateCookiesAmount;
            p.milk = p.milk - 10 * currentCreateCookiesAmount;
            UpdateRecources();
            UpdatePlayerData();
        }
        else
        {
            Debug.Log("Not enough Resources");
        }
    }

    public void MakeSugar()
    {
        p.sugar = p.sugar + sugarIncreaseAmount;
        UpdateRecources();
    }

    public void MakeFlour()
    {
        p.flour = p.flour + flourIncreaseAmount;
        UpdateRecources();
    }

    public void MakeEggs()
    {
        p.eggs = p.eggs + eggsIncreaseAmount;
        UpdateRecources();
    }

    public void MakeButter()
    {
        p.butter = p.butter + butterIncreaseAmount;
        UpdateRecources();
    }

    public void MakeChocolate()
    {
        p.chocolate = p.chocolate + chocolateIncreaseAmount;
        UpdateRecources();
    }

    public void MakeMilk()
    {
        p.milk = p.milk + milkIncreaseAmount;
        UpdateRecources();
    }

    public void Logout()
    {
        UpdatePlayerData();
        ownSceneManager.SwitchScene(0);
    }

    public void AddAmountCreateCookies(int amount)
    {
        if (currentCreateCookiesAmount >= 0 && currentCreateCookiesAmount + amount >= 0)
        {
            currentCreateCookiesAmount += amount;
            amountCookies_InputField.text = currentCreateCookiesAmount.ToString();
        }
        else { Debug.Log("GEHT NET SONST UNTER 0"); }
    }

    public void AddAmountBuyAndSellCookies(int amount)
    {
        if (currentBuyAndSellCookiesAmount >= 0 && currentBuyAndSellCookiesAmount + amount >= 0)
        {
            currentBuyAndSellCookiesAmount += amount;
            amountCookiesBuyAndSell_InputField.text = currentBuyAndSellCookiesAmount.ToString();
        }
        else { Debug.Log("GEHT NET SONST UNTER 0"); }

    }

    public void AddAmountBuyAndSellRec(int amount)
    {
        if (currentBuyAndSellRecAmount >= 0 && currentBuyAndSellRecAmount + amount >= 0)
        {
            currentBuyAndSellRecAmount += amount;
            amountRecBuyAndSell_InputField.text = currentBuyAndSellRecAmount.ToString();
        }
        else { Debug.Log("GEHT NET SONST UNTER 0"); }
    }

    public void Buy()
    {

    }

    public void Sell()
    {

    }

}
