using UnityEngine;
using UnityEngine.SceneManagement;

public class OwnSceneManager : MonoBehaviour
{
    public void SwitchScene(int newScene)
    {
        SceneManager.LoadScene(newScene);
    }
}
