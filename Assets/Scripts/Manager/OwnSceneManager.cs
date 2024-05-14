using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OwnSceneManager : MonoBehaviour
{
    public void SwitchScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }
}
