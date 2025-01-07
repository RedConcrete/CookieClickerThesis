using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField]
    private MusicLibrary musicLibrary;
    [SerializeField]
    private AudioSource musicSource;
    [SerializeField]
    private AudioLowPassFilter lowPassFilter; // Low-pass filter for muffled effect

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

    public void PlayMusic(string trackName, float fadeDuration = 0.5f, bool isMuffled = false)
    {
        StartCoroutine(AnimateMusicCrossfade(musicLibrary.GetClipFromName(trackName), fadeDuration, isMuffled));
    }

    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f, bool isMuffled = false)
    {
        float percent = 0;

        // Fade out current music
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(1f, 0, percent);
            yield return null;
        }

        // Change the track
        musicSource.clip = nextTrack;
        musicSource.Play();

        // Apply or remove muffled effect
        if (isMuffled)
        {
            lowPassFilter.cutoffFrequency = 800; // Adjust this value for desired muffling
        }
        else
        {
            lowPassFilter.cutoffFrequency = 22000; // Reset to default (no muffling)
        }

        // Fade in new music
        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(0, 1f, percent);
            yield return null;
        }
    }

    public bool IsTrackPlaying(string trackName)
    {
        return musicSource.isPlaying && musicSource.clip != null && musicSource.clip.name == trackName;
    }

    // Muffle the currently playing track
    public void MuffleCurrentTrack(float fadeDuration = 0f)
    {
        if (musicSource.isPlaying)
        {
            StartCoroutine(AnimateMuffling(true, fadeDuration));
        }
    }

    // Unmuffle the currently playing track
    public void UnmuffleCurrentTrack(float fadeDuration = 1f)
    {
        if (musicSource.isPlaying)
        {
            StartCoroutine(AnimateMuffling(false, fadeDuration));
        }
    }

    // Coroutine to smoothly apply or remove the muffling effect
    private IEnumerator AnimateMuffling(bool muffle, float fadeDuration)
    {
        float startValue = lowPassFilter.cutoffFrequency;
        float endValue = muffle ? 500f : 22000f; // 800 for muffled, 22000 for clear sound
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime / fadeDuration;
            lowPassFilter.cutoffFrequency = Mathf.Lerp(startValue, endValue, percent);
            yield return null;
        }

        lowPassFilter.cutoffFrequency = endValue; // Ensure the final value is set
    }

    public bool IsTrackMuffled()
    {
        // Prüft, ob der LowPassFilter auf einen dumpfen Wert gesetzt ist (z.B. 800 Hz)
        return lowPassFilter.cutoffFrequency <= 800f;
    }

}
