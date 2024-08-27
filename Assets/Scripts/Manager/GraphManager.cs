using CodeMonkey.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Linq;

public class GraphManager : MonoBehaviour
{
    [Header("RectTransform:")]
    public RectTransform graphContainer;
    public RectTransform labelTempX;
    public RectTransform labelTempY;
    public RectTransform dashTempX;
    public RectTransform dashTempY;

    [Header("Sprites:")]
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private Sprite sugarSprite;
    [SerializeField] private Sprite flourSprite;
    [SerializeField] private Sprite eggsSprite;
    [SerializeField] private Sprite butterSprite;
    [SerializeField] private Sprite chocolateSprite;
    [SerializeField] private Sprite milkSprite;

    [Header("MarketColors:")]
    public Color sugarColor = Color.red;
    public Color flourColor = Color.blue;
    public Color eggsColor = Color.green;
    public Color butterColor = Color.yellow;
    public Color chocolateColor = Color.magenta;
    public Color milkColor = Color.cyan;

    [Header("ResourcePrices:")]
    public TMP_Text sugar_PriceText;
    public TMP_Text flour_PriceText;
    public TMP_Text eggs_PriceText;
    public TMP_Text butter_PriceText;
    public TMP_Text chocolate_PriceText;
    public TMP_Text milk_PriceText;

    float yMaximum = 1000f;
    float xSize = 50f;
    int graphSize = 1080 / 2;
    int pricesCount = 10;

    private void Awake()
    {
    }

    private GameObject CreateIcon(Vector2 anchoredPos, int price, string rec, Sprite s)
    {
        GameObject icon = new GameObject("icon", typeof(Image));
        icon.transform.SetParent(graphContainer, false);
        Image iconImage = icon.GetComponent<Image>();
        iconImage.sprite = s;

        BoxCollider2D collider = icon.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(10, 10);

        RectTransform rectTransform = icon.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPos;
        rectTransform.sizeDelta = new Vector2(40, 40);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.anchorMin = new Vector2(0, 0);

        GameObject textObject = new GameObject("text", typeof(TextMeshProUGUI));
        textObject.transform.SetParent(icon.transform, false);
        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.text = price.ToString();
        textComponent.fontSize = 12;
        textComponent.color = Color.black;
        textComponent.alignment = TextAlignmentOptions.Center;
        RectTransform textTransform = textObject.GetComponent<RectTransform>();
        textTransform.anchoredPosition = new Vector2(0, -15);

        return icon;
    }
    private GameObject CreateCircle(Vector2 anchoredPos, Color c, int price, string rec)
    {
        GameObject circle = new GameObject("circle", typeof(Image));
        circle.transform.SetParent(graphContainer, false);
        Image circleImage = circle.GetComponent<Image>();
        circleImage.sprite = circleSprite;
        circleImage.color = c;

        BoxCollider2D collider = circle.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(10, 10);

        RectTransform rectTransform = circle.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPos;
        rectTransform.sizeDelta = new Vector2(10, 10);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.anchorMin = new Vector2(0, 0);

        GameObject textObject = new GameObject("text", typeof(TextMeshProUGUI));
        textObject.transform.SetParent(circle.transform, false);
        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.text = price.ToString();
        textComponent.fontSize = 12;
        textComponent.color = Color.black;
        textComponent.alignment = TextAlignmentOptions.Center;
        RectTransform textTransform = textObject.GetComponent<RectTransform>();
        textTransform.anchoredPosition = new Vector2(0, -15);

        return circle;
    }

    public async void UpdateGraph()
    {
        while (WebAPI.Instance.GetMarket() == null) await Task.Delay(10);
        List<Market> marketList = WebAPI.Instance.GetMarket();

        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        GameObject lastCircleGameObjectSugar = null;
        GameObject lastCircleGameObjectFlour = null;
        GameObject lastCircleGameObjectEggs = null;
        GameObject lastCircleGameObjectButter = null;
        GameObject lastCircleGameObjectChocolate = null;
        GameObject lastCircleGameObjectMilk = null;

        DeleteAllChilds();

        for (int i = 0; marketList.Count > i; i++)
        {
            float xPos = xSize + graphSize + (pricesCount - 1 - i) * xSize;
            GameObject circleGameObjectSugar;
            GameObject circleGameObjectFlour;
            GameObject circleGameObjectEggs;
            GameObject circleGameObjectButter;
            GameObject circleGameObjectChocolate;
            GameObject circleGameObjectMilk;

            float yPosSugar = (marketList[i].sugarPrice / yMaximum) * graphHeight;
            if (i == 0)
            {
                circleGameObjectSugar = CreateIcon(new Vector2(xPos, yPosSugar), (int)marketList[i].sugarPrice, "Sugar", sugarSprite);
            }
            else
            {
                circleGameObjectSugar = CreateCircle(new Vector2(xPos, yPosSugar), sugarColor, (int)marketList[i].sugarPrice, "Sugar");
            }
            if (lastCircleGameObjectSugar != null)
            {
                CreateDotConnection(lastCircleGameObjectSugar.GetComponent<RectTransform>().anchoredPosition,
                    circleGameObjectSugar.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObjectSugar = circleGameObjectSugar;


            float yPosFlour = (marketList[i].flourPrice / yMaximum) * graphHeight;
            if (i == 0)
            {
                circleGameObjectFlour = CreateIcon(new Vector2(xPos, yPosFlour), (int)marketList[i].flourPrice, "Flour", flourSprite);
            }
            else
            {
                circleGameObjectFlour = CreateCircle(new Vector2(xPos, yPosFlour), flourColor, (int)marketList[i].flourPrice, "Flour");
            }
            if (lastCircleGameObjectFlour != null)
            {
                CreateDotConnection(lastCircleGameObjectFlour.GetComponent<RectTransform>().anchoredPosition,
                    circleGameObjectFlour.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObjectFlour = circleGameObjectFlour;


            float yPosEggs = (marketList[i].eggsPrice / yMaximum) * graphHeight;
            if (i == 0)
            {
                circleGameObjectEggs = CreateIcon(new Vector2(xPos, yPosEggs), (int)marketList[i].eggsPrice, "Eggs", eggsSprite);
            }
            else
            {
                circleGameObjectEggs = CreateCircle(new Vector2(xPos, yPosEggs), eggsColor, (int)marketList[i].eggsPrice, "Eggs");
            }
            if (lastCircleGameObjectEggs != null)
            {
                CreateDotConnection(lastCircleGameObjectEggs.GetComponent<RectTransform>().anchoredPosition,
                    circleGameObjectEggs.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObjectEggs = circleGameObjectEggs;

            float yPosButter = (marketList[i].butterPrice / yMaximum) * graphHeight;
            if (i == 0)
            {
                circleGameObjectButter = CreateIcon(new Vector2(xPos, yPosButter), (int)marketList[i].butterPrice, "Butter", butterSprite);
            }
            else
            {
                circleGameObjectButter = CreateCircle(new Vector2(xPos, yPosButter), butterColor, (int)marketList[i].butterPrice, "Butter");
            }
            if (lastCircleGameObjectButter != null)
            {
                CreateDotConnection(lastCircleGameObjectButter.GetComponent<RectTransform>().anchoredPosition,
                    circleGameObjectButter.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObjectButter = circleGameObjectButter;

            float yPosChocolate = (marketList[i].chocolatePrice / yMaximum) * graphHeight;
            if (i == 0)
            {
                circleGameObjectChocolate = CreateIcon(new Vector2(xPos, yPosChocolate), (int)marketList[i].chocolatePrice, "Chocolate", chocolateSprite);
            }
            else
            {
                circleGameObjectChocolate = CreateCircle(new Vector2(xPos, yPosChocolate), chocolateColor, (int)marketList[i].chocolatePrice, "Chocolate");
            }
            if (lastCircleGameObjectChocolate != null)
            {
                CreateDotConnection(lastCircleGameObjectChocolate.GetComponent<RectTransform>().anchoredPosition,
                    circleGameObjectChocolate.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObjectChocolate = circleGameObjectChocolate;

            float yPosMilk = (marketList[i].milkPrice / yMaximum) * graphHeight;
            if (i == 0)
            {
                circleGameObjectMilk = CreateIcon(new Vector2(xPos, yPosMilk), (int)marketList[i].milkPrice, "Milk", milkSprite);
            }
            else
            {
                circleGameObjectMilk = CreateCircle(new Vector2(xPos, yPosMilk), milkColor, (int)marketList[i].milkPrice, "Milk");
            }
            if (lastCircleGameObjectMilk != null)
            {
                CreateDotConnection(lastCircleGameObjectMilk.GetComponent<RectTransform>().anchoredPosition,
                    circleGameObjectMilk.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObjectMilk = circleGameObjectMilk;

            RectTransform labelX = Instantiate(labelTempX);
            labelX.SetParent(graphContainer, false);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(xPos, -7f);
            labelX.GetComponent<TextMeshProUGUI>().text = marketList[i].date.ToString("HH:mm");

            RectTransform dashX = Instantiate(dashTempX);
            dashX.SetParent(graphContainer, false);
            dashX.gameObject.SetActive(true);
            dashX.sizeDelta = new Vector2(1.5f, graphHeight);
            dashX.anchoredPosition = new Vector2(xPos, -7f);
        }

        sugar_PriceText.text = (int)marketList.First().sugarPrice + " Cokkies";
        flour_PriceText.text = (int)marketList.First().flourPrice + " Cokkies";
        eggs_PriceText.text = (int)marketList.First().eggsPrice + " Cokkies";
        butter_PriceText.text = (int)marketList.First().butterPrice + " Cokkies";
        chocolate_PriceText.text = (int)marketList.First().chocolatePrice + " Cokkies";
        milk_PriceText.text = (int)marketList.First().milkPrice + " Cokkies";

        int separatorCount = 10;
        for (int i = 0; i <= separatorCount; i++)
        {
            RectTransform labelY = Instantiate(labelTempY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            float norm = i * 1f / separatorCount;
            labelY.anchoredPosition = new Vector2(graphWidth + 25f, norm * graphHeight);
            labelY.GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(norm * yMaximum).ToString();

            RectTransform dashY = Instantiate(dashTempY);
            dashY.SetParent(graphContainer, false);
            dashY.gameObject.SetActive(true);
            dashY.sizeDelta = new Vector2(graphWidth, 1.5f);
            dashY.anchoredPosition = new Vector2(-4f, norm * graphHeight);
        }
    }

    public void DeleteAllChilds()
    {
        for (int i = graphContainer.transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(graphContainer.transform.GetChild(i).gameObject);
        }
    }

    private void CreateDotConnection(Vector2 dotPosA, Vector2 dotPosB)
    {
        GameObject line = new GameObject("dotConnection", typeof(Image));
        line.transform.SetParent(graphContainer, false);
        RectTransform rectTransform = line.GetComponent<RectTransform>();
        Vector2 dir = (dotPosB - dotPosA).normalized;
        float distance = Vector2.Distance(dotPosA, dotPosB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = dotPosA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));

        if (rectTransform.localEulerAngles.z <= 180)
        {
            line.GetComponent<Image>().color = Color.red;
        }
        else
        {
            line.GetComponent<Image>().color = Color.green;
        }
    }
}

