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
            DontDestroyOnLoad(gameObject); // Instanz wird nicht zerstört beim Szenenwechsel
        }
        else
        {
            Destroy(gameObject); // Doppelte Instanz wird zerstört
        }
    }

    private void Start()
    {
        // Beispiel-Aufruf beim Start
        if (!IsThisAchievementUnlocked("NEW_ACHIEVEMENT_1_0"))
        {
            UnlockAchievement("NEW_ACHIEVEMENT_1_0");
        }
    }

    public bool IsThisAchievementUnlocked(string id)
    {
        var ach = new Steamworks.Data.Achievement(id);
        Debug.Log($"Achievement {id} status: " + ach.State);
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
