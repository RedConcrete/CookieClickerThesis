using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;


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
    public int updateTime = 11;
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
    private User currentUser;
    public RectTransform createRecList;
    public RectTransform graph;

    [Header("MarketFields:")]
    private string rec;
    private GameObject[] recTag;


    public float moveSpeed; // Geschwindigkeit der Bewegung (Pixel pro Sekunde)
    private bool isOffScreen = true; // Status, ob die Liste aus dem Screen ist
    private Coroutine horizontalMoveCoroutineNegeativ;
    private Coroutine horizontalMoveCoroutinePositiv; // Speichert die aktuelle Coroutine
    private Coroutine verticalMoveCoroutineNegativ;
    private Coroutine verticalMoveCoroutinePositiv;
    private string pData;
    private int initialAmount = 1;
    private int currentCreateCookiesAmount = 1;
    private int currentBuyAndSellCookiesAmount = 1;
    private int currentBuyAndSellRecAmount = 1;
    private int idleScene = 2;
    private int marketScene = 1;
    private int scene;
    private List<Market> marketList;
    int totalCost = 0;

    float screenWidth = Screen.width;
    float screenHeight = Screen.height;

    private void Start()
    {
        scene = SceneManager.GetActiveScene().buildIndex;
        currentUser = WebAPI.user;

        MusicManager.Instance.PlayRandomMusic();
        UpdateRecources();
        Debug.Log("Market loaded");

        recTag = GameObject.FindGameObjectsWithTag("RecTag");
        GameObject graph = GameObject.Find("Graph");
        graphManager = graph.GetComponent<GraphManager>();
        amountRecBuyAndSell_InputField.text = initialAmount.ToString();
        UpdatePlayerID();
    }

    private void Update()
    {
        if (scene == marketScene)
        {
            // Pr�fen, ob der Timer l�uft
            if (timerIsRunning)
            {
                // Zeit verringern
                timeRemaining -= Time.deltaTime;

                // Zeit anzeigen
                DisplayTime(timeRemaining);

                // �berpr�fen, ob der Timer abgelaufen ist
                if (timeRemaining <= 1)
                {
                    // Timer zur�cksetzen und Markt aktualisieren
                    timeRemaining = updateTime;
                    UpdateMarketDataAndUserData();
                }
            }
            else if (timeRemaining >= -1.0f && !timerIsRunning)
            {
                // Timer synchronisieren, wenn er nicht l�uft
                SyncTimer();
            }
        }
    }

    public void UpdatePlayer()
    {
        StartCoroutine(WebAPI.Instance.GetPlayer(currentUser.steamid, false));
    }
    public void UpdateMarketDataAndUserData()
    {
        // Hole die Preise vom Server und den User
        StartCoroutine(WebAPI.Instance.UpdatePlayerAndMarket(currentUser.steamid, amountToGetGraph));

        // Lade die Marktdaten in die Liste
        List<Market> newMarketList = WebAPI.Instance.GetMarket();

        // �berpr�fe, ob sich die Marktdaten ge�ndert haben, bevor du die UI aktualisierst
        if (!AreMarketsEqual(marketList, newMarketList))
        {
            //Debug.Log("The new Marketlist is new");
            marketList = newMarketList;

            // Aktualisiere den Graphen, wenn sich die Marktdaten ge�ndert haben
            graphManager.UpdateGraph();

            // Aktualisiere andere UI-Elemente nur bei �nderungen
            totalCostField.text = calcTotalCost().ToString();
        }
        else
        {
            Debug.Log("The new Marketlist isn´t new");
        }
    }
    private bool AreMarketsEqual(List<Market> oldList, List<Market> newList)
    {
        if (oldList == null || newList == null || oldList.Count != newList.Count)
        {
            return false;
        }

        for (int i = 0; i < oldList.Count; i++)
        {
            if (!oldList[i].Equals(newList[i]))
            {
                return false;
            }
        }
        return true;
    }
    async void SyncTimer()
    {
        if (!timerIsRunning)  // Verhindern, dass der Timer erneut gestartet wird
        {
            StartCoroutine(WebAPI.Instance.GetPrices(amountToGetGraph));
            while (WebAPI.Instance.GetMarket() == null)
            {
                await Task.Delay(10);
            }

            // Timer basierend auf der Differenz zur aktuellen Marktzeit synchronisieren
            TimeSpan timeSpan = DateTime.UtcNow - WebAPI.Instance.GetMarket()[0].date;
            timeRemaining = (float)timeSpan.TotalSeconds % updateTime;  // Berechne verbleibende Zeit
            timerIsRunning = true;
        }
    }
    void DisplayTime(float timeToDisplay)
    {
        if (timeToDisplay < 0)
        {
            timeToDisplay = 0;  // Zeit nicht negativ anzeigen
        }

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);  // Minuten berechnen
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);  // Sekunden berechnen

        // Zeit im Format mm:ss anzeigen
        updateTime_Text.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public void UpdateRecources()
    {
        cookie_Text.text = currentUser.cookies.ToString();
        sugar_Text.text = currentUser.sugar.ToString();
        flour_Text.text = currentUser.flour.ToString();
        eggs_Text.text = currentUser.eggs.ToString();
        butter_Text.text = currentUser.butter.ToString();
        chocolate_Text.text = currentUser.chocolate.ToString();
        milk_Text.text = currentUser.milk.ToString();
    }

    public void UpdatePlayerID()
    {
        playerIDField.text = currentUser.steamid;
    }

    public void ProduceCookies() //Todo anpassen das es nur das Rezept senden was verwendet werden soll
    {
        int requiredAmount = 10 * currentCreateCookiesAmount;
        if (HasSufficientResources(requiredAmount))
        {
            currentUser.cookies += 100 * currentCreateCookiesAmount; //TODO
            subtracRec(-requiredAmount);
            UpdateMarketDataAndUserData();// Todo siehe oben
            UpdateRecources();
        }
        else
        {
            Debug.Log("Not enough Resources");
        }
    }

    private bool HasSufficientResources(int requiredAmount)
    {
        return currentUser.sugar >= requiredAmount
            && currentUser.flour >= requiredAmount
            && currentUser.eggs >= requiredAmount
            && currentUser.butter >= requiredAmount
            && currentUser.chocolate >= requiredAmount
            && currentUser.milk >= requiredAmount;
    }

    private void subtracRec(int amountChange)
    {
        currentUser.sugar += amountChange;
        currentUser.flour += amountChange;
        currentUser.eggs += amountChange;
        currentUser.butter += amountChange;
        currentUser.chocolate += amountChange;
        currentUser.milk += amountChange;
    }

    private void ToggleListPosition(RectTransform rect, bool horizontal, bool positiv)
    {

        if (horizontal)
        {
            // Berechnung der Zielposition für horizontale Bewegung
            Vector2 targetPosition = isOffScreen
                ? new Vector2(0, rect.anchoredPosition.y)
                : new Vector2(screenWidth - (screenWidth - rect.sizeDelta.x * 1.25f), rect.anchoredPosition.y);

            // Stoppe laufende Coroutine, wenn eine existiert
            if (horizontalMoveCoroutinePositiv != null)
            {
                StopCoroutine(horizontalMoveCoroutinePositiv);
            }

            // Starte die Bewegung zur Zielposition
            horizontalMoveCoroutinePositiv = StartCoroutine(MoveList(targetPosition, rect));
            isOffScreen = !isOffScreen; // Status umschalten
        }
        else
        {

            if (positiv)
            {

                // Berechnung der Zielposition für vertikale Bewegung
                Vector2 targetPosition = isOffScreen
                    ? new Vector2(rect.anchoredPosition.x, 0) // Position über dem Bildschirm
                    : new Vector2(rect.anchoredPosition.x, rect.sizeDelta.y * 1.25f); // Position unter dem Bildschirm

                // Stoppe laufende Coroutine, wenn eine existiert
                if (verticalMoveCoroutinePositiv != null)
                {
                    StopCoroutine(verticalMoveCoroutinePositiv);
                }

                // Starte die Bewegung zur Zielposition
                verticalMoveCoroutinePositiv = StartCoroutine(MoveList(targetPosition, rect));
                isOffScreen = !isOffScreen; // Status umschalten
            }
            else
            {
                // Berechnung der Zielposition für vertikale Bewegung
                Vector2 targetPosition = isOffScreen
                    ? new Vector2(rect.anchoredPosition.x, 0) // Position über dem Bildschirm
                    : new Vector2(rect.anchoredPosition.x, -rect.sizeDelta.y * 1.25f); // Position unter dem Bildschirm

                // Stoppe laufende Coroutine, wenn eine existiert
                if (verticalMoveCoroutineNegativ != null)
                {
                    StopCoroutine(verticalMoveCoroutineNegativ);
                }

                // Starte die Bewegung zur Zielposition
                verticalMoveCoroutineNegativ = StartCoroutine(MoveList(targetPosition, rect));
                isOffScreen = !isOffScreen; // Status umschalten
            }

        }

    }

    private IEnumerator MoveList(Vector2 targetPosition, RectTransform rect)
    {
        // Solange die aktuelle Position nicht das Ziel erreicht hat
        while (Vector2.Distance(rect.anchoredPosition, targetPosition) > 0.1f)
        {
            // Bewege die Position näher zum Ziel
            rect.anchoredPosition = Vector2.MoveTowards(
                rect.anchoredPosition,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null; // Warte bis zum nächsten Frame
        }
        // Setze die exakte Zielposition am Ende (um Rundungsfehler zu vermeiden)
        rect.anchoredPosition = targetPosition;
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

    private IEnumerator ResetTagsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        for (int i = 0; i < this.recTag.Length; i++)
        {
            recTag[i].GetComponent<Image>().color = Color.gray;
        }
    }

    //---------------------------- Button Methodes ----------------------------

    // Brocken muss gefixed werden !!
    public void setRec(string rec)
    {
        GameObject gameObject = GameObject.Find("MarketRecSetter");
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
    public void Buy()
    {
        if (currentUser != null)
        {
            if (rec != null)
            {
                if (currentUser.cookies >= totalCost)
                {
                    StartCoroutine(WebAPI.Instance.PostBuy(currentUser.steamid, rec, int.Parse(amountRecBuyAndSell_InputField.text)));
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
            Debug.Log("Player: " + currentUser + "Recources: " + rec);
        }
    }
    public void Sell()
    {
        if (currentUser != null)
        {
            if (rec != null)
            {
                StartCoroutine(WebAPI.Instance.PostSell(currentUser.steamid, rec, int.Parse(amountRecBuyAndSell_InputField.text)));
                UpdateMarketDataAndUserData();
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
            Debug.Log("Player: " + currentUser + "Recources: " + rec);
        }
    }
    public void MakeSelectedRec(string rec)
    {
        switch (rec)
        {
            case "sugar":

                currentUser.sugar += sugarIncreaseAmount;
                UpdateRecources();
                break;
            case "flour":
                // 
                currentUser.flour += flourIncreaseAmount;
                UpdateRecources();
                break;
            case "eggs":
                // 
                currentUser.eggs += eggsIncreaseAmount;
                UpdateRecources();
                break;
            case "butter":
                // 
                currentUser.butter += butterIncreaseAmount;
                UpdateRecources();
                break;
            case "chocolate":
                // 
                currentUser.chocolate += chocolateIncreaseAmount;
                UpdateRecources();
                break;
            case "milk":
                // 
                currentUser.milk += milkIncreaseAmount;
                UpdateRecources();
                break;
            default:
                Debug.LogError("Iwas ist falsch!");
                break;
        }
    }

    public void UpgradeIncreaseAmountOfRec(string rec)
    {
        switch (rec)
        {
            case "sugar":
                // 
                sugarIncreaseAmount += 1;
                break;
            case "flour":
                // 
                flourIncreaseAmount += 1;
                break;
            case "eggs":
                // 
                eggsIncreaseAmount += 1;
                break;
            case "butter":
                // 
                butterIncreaseAmount += 1;
                break;
            case "chocolate":
                // 
                chocolateIncreaseAmount += 1;
                break;
            case "milk":
                // 
                milkIncreaseAmount += 1;
                break;
            default:
                Debug.LogError("Iwas ist falsch!");
                break;
        }
    }

    public void Logout()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
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
    public void SwitchToOtherGamemode(int mode)
    {
        switch (mode)
        {
            case 1:
                // Idlemode
                ToggleListPosition(createRecList, false, true);
                ToggleListPosition(graph, false, false);
                break;
            default:
                // Marketmode
                ToggleListPosition(graph, false, false);
                ToggleListPosition(createRecList, false, true);
                break;
        }
    }
    //---------------------------- Not in use ----------------------------
    public void SwitchScreen(int sceneIndex)
    {
        if (scene != sceneIndex)
        {

            if (scene == marketScene)
            {
                graphManager.UpdateGraph();
            }
            SceneManager.LoadScene(sceneIndex);
        }
    }
    public void CopyToClip()
    {
        GUIUtility.systemCopyBuffer = playerIDField.text;
    }

}
