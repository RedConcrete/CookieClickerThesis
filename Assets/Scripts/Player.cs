using System;
using System.Xml.Linq;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[Serializable]
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

    private void Awake()
    {
    }

    public void ProduceCookies()
    {
        Cookies++;
    }
}
