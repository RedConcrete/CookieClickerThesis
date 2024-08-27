using System;


[System.Serializable]
public class Player
{
    public string id = Guid.NewGuid().ToString();
    public int cookies = 5000;
    public int sugar = 10;
    public int flour = 10;
    public int eggs = 10;
    public int butter = 10;
    public int chocolate = 10;
    public int milk = 10;
}
