using TMPro;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField playerTextField;

    private void Start()
    {
        if (!MusicManager.Instance.IsTrackPlaying("Caketown 1"))
        {
            MusicManager.Instance.PlayMusic("Main");
        }
    }

    public void Login()
    {
        if (WebAPI.Instance != null)
        {
            Debug.Log("Player login in with " + playerTextField.text);
            StartCoroutine(WebAPI.Instance.GetPlayer(WebAPI.Instance.GetSteamID().ToString(), true));
        }
        else
        {
            Debug.LogError("WebAPI instance is not initialized!");
        }
    }

    public void Register()
    {
        if (WebAPI.Instance != null)
        {
            StartCoroutine(WebAPI.Instance.PostPlayer());
        }
        else
        {
            Debug.LogError("WebAPI instance is not initialized!");
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
