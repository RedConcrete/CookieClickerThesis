using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using TMPro;
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
    public TMP_InputField amount_InputField;

    [Header("ProduceAmount:")]
    public int sugarIncreaseAmount = 1;
    public int flourIncreaseAmount = 1;
    public int eggsIncreaseAmount = 1;
    public int butterIncreaseAmount = 1;
    public int chocolateIncreaseAmount = 1;
    public int milkIncreaseAmount = 1;

    private Player p;
    private string pData;
    private OwnSceneManager ownSceneManager = new OwnSceneManager();

    private void Start()
    {
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
}
