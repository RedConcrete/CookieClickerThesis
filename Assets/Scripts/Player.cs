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
    public string id = Guid.NewGuid().ToString();
    public int cookies = 1000;
    public int sugar = 100;
    public int flour = 100;
    public int eggs = 100;
    public int butter = 100;
    public int chocolate = 100;
    public int milk = 100;
}
