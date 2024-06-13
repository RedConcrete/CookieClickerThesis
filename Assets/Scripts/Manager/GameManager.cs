using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Collections;

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


    [Header("AmountField:")]
    public TMP_InputField amountCookies_InputField;
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
    private Scene scene;
    private int idleScene = 2;
    private int marketScene = 1;


    private void Start()
    {
        recTag = GameObject.FindGameObjectsWithTag("RecTag");
        scene = SceneManager.GetActiveScene();
        if (scene.buildIndex == idleScene)
        {
            amountCookies_InputField.text = initialAmount.ToString();
            amountCookiesBuyAndSell_InputField.text = initialAmount.ToString();
            amountRecBuyAndSell_InputField.text = initialAmount.ToString();
        }
        if (scene.buildIndex == marketScene)
        {
            amountRecBuyAndSell_InputField.text = initialAmount.ToString();
        }
        currentPlayer = WebAPI.Instance.GetLoginPlayer();
        UpdateRecources();
    }

    private void Update()
    {
        if (scene.buildIndex == marketScene)
        {
            if (timerIsRunning)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);

                if (timeRemaining < 0)
                {
                    timeRemaining = updateTime;
                    StartCoroutine(WebAPI.Instance.GetPrices(amountToGetGraph));
                    graphManager.UpdateGraph();
                    UpdatePlayerData();
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
        StartCoroutine(WebAPI.Instance.GetPrices(1));
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

    public void UpdatePlayerData()
    {
        pData = JsonUtility.ToJson(currentPlayer);
        StartCoroutine(WebAPI.Instance.UpdatePlayer(pData));
    }

    public void ProduceCookies() // Cookies preis anpassen!!
    {
        if (currentPlayer.sugar >= 10 * currentCreateCookiesAmount && currentPlayer.flour >= 10 * currentCreateCookiesAmount && currentPlayer.eggs >= 10 * currentCreateCookiesAmount && currentPlayer.butter >= 10 * currentCreateCookiesAmount && currentPlayer.chocolate >= 10 * currentCreateCookiesAmount && currentPlayer.milk >= 10 * currentCreateCookiesAmount)
        {
            currentPlayer.cookies = currentPlayer.cookies + (100 * currentCreateCookiesAmount);
            currentPlayer.sugar = currentPlayer.sugar - 10 * currentCreateCookiesAmount;
            currentPlayer.flour = currentPlayer.flour - 10 * currentCreateCookiesAmount;
            currentPlayer.eggs = currentPlayer.eggs - 10 * currentCreateCookiesAmount;
            currentPlayer.butter = currentPlayer.butter - 10 * currentCreateCookiesAmount;
            currentPlayer.chocolate = currentPlayer.chocolate - 10 * currentCreateCookiesAmount;
            currentPlayer.milk = currentPlayer.milk - 10 * currentCreateCookiesAmount;
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
        if (scene.buildIndex != sceneIndex)
        {
            ownSceneManager.SwitchScene(sceneIndex);
        }
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
        if (currentBuyAndSellRecAmount >= 0 && currentBuyAndSellRecAmount + amount >= 0)
        {
            currentBuyAndSellRecAmount += amount;
            amountRecBuyAndSell_InputField.text = currentBuyAndSellRecAmount.ToString();
        }
        else
        {
            currentBuyAndSellRecAmount = 0;
            amountRecBuyAndSell_InputField.text = currentBuyAndSellRecAmount.ToString();
        }
    }

    public void Buy()
    {
        if (currentPlayer != null)
        {
            if (rec != null)
            {
                StartCoroutine(WebAPI.Instance.PostBuy(currentPlayer.id, rec, int.Parse(amountRecBuyAndSell_InputField.text)));
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
                UpdatePlayerData();
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

}
