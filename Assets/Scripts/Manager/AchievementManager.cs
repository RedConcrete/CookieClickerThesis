using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    // Singleton-Instanz
    public static AchievementManager Instance { get; private set; }

    private void Awake()
    {
        // Sicherstellen, dass nur eine Instanz existiert
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Instanz wird nicht zerst�rt beim Szenenwechsel
        }
        else
        {
            Destroy(gameObject); // Doppelte Instanz wird zerst�rt
        }
    }

    private void Start()
    {
        // Beispiel-Aufruf beim Start
        if (!IsThisAchievementUnlocked("Minecraft?"))
        {
            UnlockAchievement("Minecraft?");
        }
    }

    public bool IsThisAchievementUnlocked(string id)
    {
        var ach = new Steamworks.Data.Achievement(id);
        if(ach.State){
            Debug.Log($"Achievement {id} ist aktiviert worden");
        }else{
            Debug.Log($"Achievement {id} ist noch nicht aktiviert worden");
        }
        return ach.State;
    }

    public void UnlockAchievement(string id)
    {
        var ach = new Steamworks.Data.Achievement(id);
        ach.Trigger();
        Debug.Log($"Achievement {id} unlocked");
    }

    public void ClearAchievementStatus(string id)
    {
        var ach = new Steamworks.Data.Achievement(id);
        ach.Clear();
        Debug.Log($"Achievement {id} cleared");
    }
}
