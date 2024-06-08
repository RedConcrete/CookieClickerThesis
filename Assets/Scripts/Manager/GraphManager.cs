using CodeMonkey.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    private RectTransform graphContainer;

    [Header("MarketColors:")]
    public Color sugarColor = Color.red;
    public Color flourColor = Color.blue;
    public Color eggsColor = Color.green;
    public Color butterColor = Color.yellow;
    public Color chocolateColor = Color.magenta;
    public Color milkColor = Color.cyan;

    private void Awake()
    {
        graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
    }

    private GameObject CreateCircle(Vector2 anchoredPos, Color c)
    {
        GameObject circle = new GameObject("circle", typeof(Image));
        circle.transform.SetParent(graphContainer, false);
        circle.GetComponent<Image>().sprite = circleSprite;
        circle.GetComponent<Image>().color = c;
        RectTransform rectTransform = circle.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPos;
        rectTransform.sizeDelta = new Vector2(11,11);
        rectTransform.anchorMax = new Vector2(0,0);
        rectTransform.anchorMin = new Vector2(0,0);
        return circle;
    }

    public async void UpdateGraph()
    {
        while (WebAPI.Instance.GetMarket() == null) await Task.Delay(10);
        List<Market> marketList = WebAPI.Instance.GetMarket();
        float graphHeight = graphContainer.sizeDelta.y;
        float yMaximum = 1000f;
        float xSize = 50f;
        int graphSize = 1080 / 2;
        int pricesCount = 10;

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

            float yPosSugar = (marketList[i].sugarPrice / yMaximum) * graphHeight;
            GameObject circleGameObjectSugar = CreateCircle(new Vector2(xPos, yPosSugar),sugarColor);
            if (lastCircleGameObjectSugar != null)
            {
                CreateDotConnection(lastCircleGameObjectSugar.GetComponent<RectTransform>().anchoredPosition,
                    circleGameObjectSugar.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObjectSugar = circleGameObjectSugar;


            float yPosFlour = (marketList[i].flourPrice / yMaximum) * graphHeight;
            GameObject circleGameObjectFlour = CreateCircle(new Vector2(xPos, yPosFlour), flourColor);
            if (lastCircleGameObjectFlour != null)
            {
                CreateDotConnection(lastCircleGameObjectFlour.GetComponent<RectTransform>().anchoredPosition,
                    circleGameObjectFlour.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObjectFlour = circleGameObjectFlour;

            float yPosEggs = (marketList[i].eggsPrice / yMaximum) * graphHeight;
            GameObject circleGameObjectEggs = CreateCircle(new Vector2(xPos, yPosEggs), eggsColor);
            if (lastCircleGameObjectEggs != null)
            {
                CreateDotConnection(lastCircleGameObjectEggs.GetComponent<RectTransform>().anchoredPosition,
                    circleGameObjectEggs.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObjectEggs = circleGameObjectEggs;

            float yPosButter = (marketList[i].butterPrice / yMaximum) * graphHeight;
            GameObject circleGameObjectButter = CreateCircle(new Vector2(xPos, yPosButter), butterColor);
            if (lastCircleGameObjectButter != null)
            {
                CreateDotConnection(lastCircleGameObjectButter.GetComponent<RectTransform>().anchoredPosition,
                    circleGameObjectButter.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObjectButter = circleGameObjectButter;

            float yPosChocolate = (marketList[i].chocolatePrice / yMaximum) * graphHeight;
            GameObject circleGameObjectChocolate = CreateCircle(new Vector2(xPos, yPosChocolate), chocolateColor);
            if (lastCircleGameObjectChocolate != null)
            {
                CreateDotConnection(lastCircleGameObjectChocolate.GetComponent<RectTransform>().anchoredPosition,
                    circleGameObjectChocolate.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObjectChocolate = circleGameObjectChocolate;

            float yPosMilk = (marketList[i].milkPrice / yMaximum) * graphHeight;
            GameObject circleGameObjectMilk = CreateCircle(new Vector2(xPos, yPosMilk), milkColor);
            if (lastCircleGameObjectMilk != null)
            {
                CreateDotConnection(lastCircleGameObjectMilk.GetComponent<RectTransform>().anchoredPosition,
                    circleGameObjectMilk.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObjectMilk = circleGameObjectMilk;
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
        
