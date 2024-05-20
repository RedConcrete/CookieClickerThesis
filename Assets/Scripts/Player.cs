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
    public int cookies = 100;
    public int sugar = 10;
    public int flour = 10;
    public int eggs = 10;
    public int butter = 10;
    public int chocolate = 10;
    public int milk = 10;
}
