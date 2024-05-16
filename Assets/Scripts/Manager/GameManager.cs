using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TMP_Text cookie_Text;
    public TMP_Text sugar_Text;
    public TMP_Text flour_Text;
    public TMP_Text eggs_Text;
    public TMP_Text butter_Text;
    public TMP_Text chocolate_Text;
    public TMP_Text milk_Text;
    private Player p = Player.GetInstance();
    private OwnSceneManager o = new OwnSceneManager();

    private void Start()
    {
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

    public void ProduceCookies()
    {
        if (p.Sugar >= 10 && p.Flour >= 10 && p.Eggs >= 10 && p.Butter >= 10 && p.Chocolate >= 10 && p.Milk >= 10)
        {
            p.Cookies = p.Cookies + 100;
            UpdateRecources();
        }
        else
        {
            Debug.Log("Not enoth Rec");
        }
    }

}
