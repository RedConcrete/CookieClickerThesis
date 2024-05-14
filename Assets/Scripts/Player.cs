using System;
using System.Xml.Linq;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public string Id = Guid.NewGuid().ToString();
    public int Cookies = 100;
    public int Sugar;
    public int Flour;
    public int Eggs;
    public int Butter;
    public int Chocolate;
    public int Milk;

    public static Player _instance { get; private set; }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }

    public void ProduceCookies()
    {
        Cookies++;
    }
}
