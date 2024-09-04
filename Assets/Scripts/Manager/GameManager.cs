using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


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
    public int updateTime = 10;
    public float timeRemaining = -1;
    public bool timerIsRunning = false;
    private float colorResetTime = 0.5f;

    [Header("PlayerIDField:")]
    public TMP_InputField playerIDField;

    [Header("AmountField:")]
    public TMP_InputField amountCookies_InputField;
    public TMP_InputField totalCostField;
    public TMP_InputField amountCookiesBuyAndSell_InputField;
    public TMP_InputField amountRecBuyAndSell_InputField;
    public int amountToGetGraph = 20;

    [Header("ProduceAmount:")]
    public int sugarIncreaseAmount = 1;
    public int flourIncreaseAmount = 1;
    public int eggsIncreaseAmount = 1;
    public int butterIncreaseAmount = 1;
    public int chocolateIncreaseAmount = 1;
    public int milkIncreaseAmount = 1;

    [Header("Objects:")]
    public GraphManager graphManager;
    private Player currentPlayer;

    [Header("MarketFields:")]
    private string rec;
    private GameObject[] recTag;

    private string pData;
    private int initialAmount = 1;
    private int currentCreateCookiesAmount = 1;
    private int currentBuyAndSellCookiesAmount = 1;
    private int currentBuyAndSellRecAmount = 1;
    private OwnSceneManager ownSceneManager = new OwnSceneManager();
    private int idleScene = 2;
    private int marketScene = 1;
    private int scene;
    private List<Market> marketList;
    int totalCost = 0;

    private void Start()
    {
        scene = SceneManager.GetActiveScene().buildIndex;
        currentPlayer = WebAPI.Instance.GetLoginPlayer();
        UpdateRecources();

        if (scene == idleScene)
        {
            amountCookies_InputField.text = initialAmount.ToString();
        }

        if (scene == marketScene)
        {
            recTag = GameObject.FindGameObjectsWithTag("RecTag");
            GameObject graph = GameObject.Find("Graph");
            graphManager = graph.GetComponent<GraphManager>();
            amountRecBuyAndSell_InputField.text = initialAmount.ToString();
            UpdatePlayerID();
        }
    }

    private void Update()
    {
        if (scene == marketScene)
        {
            if (timerIsRunning)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);

                if (timeRemaining < 0)
                {
                    timeRemaining = updateTime;
                    StartCoroutine(WebAPI.Instance.GetPrices(amountToGetGraph));
                    marketList = WebAPI.Instance.GetMarket();
                    graphManager.UpdateGraph();
                    totalCostField.text = calcTotalCost().ToString();
                    UpdatePlayerData(); //Todo GetPlayer da nur dem Spieler die aktuellen Rec benötigt werden
                }
            }
            if (timeRemaining >= -1.0 && timerIsRunning == false)
            {
                SyncTimer();
                graphManager.UpdateGraph();
            }
        }
    }

    async void SyncTimer()
    {
        StartCoroutine(WebAPI.Instance.GetPrices(amountToGetGraph));
        while (WebAPI.Instance.GetMarket() == null) await Task.Delay(10);
        TimeSpan timeSpan = DateTime.UtcNow - WebAPI.Instance.GetMarket()[0].date;
        timeRemaining = timeSpan.Minutes / 60;
        timerIsRunning = true;
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
        cookie_Text.text = currentPlayer.cookies.ToString();
        sugar_Text.text = currentPlayer.sugar.ToString();
        flour_Text.text = currentPlayer.flour.ToString();
        eggs_Text.text = currentPlayer.eggs.ToString();
        butter_Text.text = currentPlayer.butter.ToString();
        chocolate_Text.text = currentPlayer.chocolate.ToString();
        milk_Text.text = currentPlayer.milk.ToString();
    }

    public void UpdatePlayerID()
    {
        playerIDField.text = currentPlayer.id.ToString();
    }

    public void UpdatePlayerData()
    {
        pData = JsonUtility.ToJson(currentPlayer);
        StartCoroutine(WebAPI.Instance.UpdatePlayer(pData));
    }

    public void ProduceCookies() //Todo anpassen das es nur das Rezept senden was verwendet werden soll
    {
        int requiredAmount = 10 * currentCreateCookiesAmount;
        if (HasSufficientResources(requiredAmount))
        {
            currentPlayer.cookies += 100 * currentCreateCookiesAmount;
            subtracRec(-requiredAmount);
            UpdatePlayerData();// Todo siehe oben
            UpdateRecources();
        }
        else
        {
            Debug.Log("Not enough Resources");
        }
    }

    private bool HasSufficientResources(int requiredAmount)
    {
        return currentPlayer.sugar >= requiredAmount
            && currentPlayer.flour >= requiredAmount
            && currentPlayer.eggs >= requiredAmount
            && currentPlayer.butter >= requiredAmount
            && currentPlayer.chocolate >= requiredAmount
            && currentPlayer.milk >= requiredAmount;
    }

    private void subtracRec(int amountChange)
    {
        currentPlayer.sugar += amountChange;
        currentPlayer.flour += amountChange;
        currentPlayer.eggs += amountChange;
        currentPlayer.butter += amountChange;
        currentPlayer.chocolate += amountChange;
        currentPlayer.milk += amountChange;
    }

    public void MakeSugar()
    {
        currentPlayer.sugar = currentPlayer.sugar + sugarIncreaseAmount;
        UpdateRecources();
    }

    public void MakeFlour()
    {
        currentPlayer.flour = currentPlayer.flour + flourIncreaseAmount;
        UpdateRecources();
    }

    public void MakeEggs()
    {
        currentPlayer.eggs = currentPlayer.eggs + eggsIncreaseAmount;
        UpdateRecources();
    }

    public void MakeButter()
    {
        currentPlayer.butter = currentPlayer.butter + butterIncreaseAmount;
        UpdateRecources();
    }

    public void MakeChocolate()
    {
        currentPlayer.chocolate = currentPlayer.chocolate + chocolateIncreaseAmount;
        UpdateRecources();
    }

    public void MakeMilk()
    {
        currentPlayer.milk = currentPlayer.milk + milkIncreaseAmount;
        UpdateRecources();
    }

    public void SwitchScreen(int sceneIndex)
    {
        if (scene != sceneIndex)
        {
            ownSceneManager.SwitchScene(sceneIndex);
        }
    }

    public void Logout()
    {
        UpdatePlayerData(); //Todo überprüfen ob Server und Client gleich sind wenn nein dann ist etwas falsch und Spieler übernehemen
        ownSceneManager.SwitchScene(0);
    }

    public void AddAmountCreateCookies(int amount)
    {
        if (currentCreateCookiesAmount >= 0 && currentCreateCookiesAmount + amount >= 0)
        {
            currentCreateCookiesAmount += amount;
            amountCookies_InputField.text = currentCreateCookiesAmount.ToString();
        }
        else
        {
            currentCreateCookiesAmount = 0;
            amountCookies_InputField.text = currentCreateCookiesAmount.ToString();
        }
    }

    public void AddAmountBuyAndSellCookies(int amount)
    {
        if (currentBuyAndSellCookiesAmount >= 0 && currentBuyAndSellCookiesAmount + amount >= 0)
        {
            currentBuyAndSellCookiesAmount += amount;
            amountCookiesBuyAndSell_InputField.text = currentBuyAndSellCookiesAmount.ToString();

        }
        else
        {
            currentBuyAndSellCookiesAmount = 0;
            amountCookiesBuyAndSell_InputField.text = currentBuyAndSellCookiesAmount.ToString();
        }

    }

    public void AddAmountBuyAndSellRec(int amount)
    {
        if (currentBuyAndSellRecAmount + amount < 0)
        {
            totalCost = 0;
            currentBuyAndSellRecAmount = 0;
            amountRecBuyAndSell_InputField.text = currentBuyAndSellRecAmount.ToString();
            totalCostField.text = totalCost.ToString();
        }
        else
        {
            currentBuyAndSellRecAmount += amount;
            amountRecBuyAndSell_InputField.text = currentBuyAndSellRecAmount.ToString();
            totalCostField.text = calcTotalCost().ToString();
        }
    }
    private int calcTotalCost()
    {
        totalCost = 0;
        switch (rec)
        {
            case "sugar":
                totalCost = (int)marketList.First().sugarPrice * currentBuyAndSellRecAmount;
                break;
            case "milk":
                totalCost = (int)marketList.First().milkPrice * currentBuyAndSellRecAmount;
                break;
            case "flour":
                totalCost = (int)marketList.First().flourPrice * currentBuyAndSellRecAmount;
                break;
            case "butter":
                totalCost = (int)marketList.First().butterPrice * currentBuyAndSellRecAmount;
                break;
            case "chocolate":
                totalCost = (int)marketList.First().chocolatePrice * currentBuyAndSellRecAmount;
                break;
            case "eggs":
                totalCost = (int)marketList.First().eggsPrice * currentBuyAndSellRecAmount;
                break;
            default:
                break;
        }
        return totalCost;
    }

    public void Buy()
    {
        if (currentPlayer != null)
        {
            if (rec != null)
            {
                if (currentPlayer.cookies >= totalCost)
                {
                    StartCoroutine(WebAPI.Instance.PostBuy(currentPlayer.id, rec, int.Parse(amountRecBuyAndSell_InputField.text)));
                }
                else
                {
                    Debug.Log("Nicht genug Cookies!");
                }
            }
            else
            {
                for (int i = 0; i < this.recTag.Length; i++)
                {
                    recTag[i].GetComponent<Image>().color = Color.red;
                }
                StartCoroutine(ResetTagsAfterDelay(colorResetTime));
            }
        }
        else
        {
            Debug.Log("Player: " + currentPlayer + "Recources: " + rec);
        }
    }

    public void Sell()
    {
        if (currentPlayer != null)
        {
            if (rec != null)
            {
                StartCoroutine(WebAPI.Instance.PostSell(currentPlayer.id, rec, int.Parse(amountRecBuyAndSell_InputField.text)));
                UpdatePlayerData(); // TODO Get Player reicht
            }
            else
            {
                for (int i = 0; i < this.recTag.Length; i++)
                {
                    recTag[i].GetComponent<Image>().color = Color.red;
                }
                StartCoroutine(ResetTagsAfterDelay(2f));
            }
        }
        else
        {
            Debug.Log("Player: " + currentPlayer + "Recources: " + rec);
        }
    }

    public void setRec(string rec)
    {
        GameObject gameObject = GameObject.Find(rec);
        if (rec != this.rec)
        {
            for (int i = 0; i < this.recTag.Length; i++)
            {
                recTag[i].GetComponent<Image>().color = Color.gray;
            }
            gameObject.GetComponent<Image>().color = Color.white;
            this.rec = rec;
        }
        else
        {
            this.rec = rec;
        }
    }

    private IEnumerator ResetTagsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        for (int i = 0; i < this.recTag.Length; i++)
        {
            recTag[i].GetComponent<Image>().color = Color.gray;
        }
    }

    public void CopyToClip()
    {
        GUIUtility.systemCopyBuffer = playerIDField.text;
    }
}
