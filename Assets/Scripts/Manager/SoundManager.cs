using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField]
    private MusicLibrary soundLibrary;
    [SerializeField]
    private AudioSource soundSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void PlaySound(string soundName, float volume = 1.0f)
    {
        AudioClip clip = soundLibrary.GetClipFromName(soundName);
        if (clip != null)
        {
            soundSource.clip = clip;
            soundSource.volume = volume;
            soundSource.loop = false; // Soundeffekte loopen nicht
            soundSource.Play();
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' nicht in der SoundLibrary gefunden.");
        }
    }

    public bool IsSoundPlaying(string soundName)
    {
        return soundSource.isPlaying && soundSource.clip != null && soundSource.clip.name == soundName;
    }

    public void StopSound()
    {
        if (soundSource.isPlaying)
        {
            soundSource.Stop();
        }
    }
}
