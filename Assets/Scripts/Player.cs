using System;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[System.Serializable]
public class Player
{
    public string Id = Guid.NewGuid().ToString();
    public int Cookies = 100;
    public int Sugar = 10;
    public int Flour = 10;
    public int Eggs = 10;
    public int Butter = 10;
    public int Chocolate = 10;
    public int Milk = 10;
}
