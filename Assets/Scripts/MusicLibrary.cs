using UnityEngine;

[System.Serializable]
public struct MusicTrack
{
    public string trackName;
    public AudioClip clip;
}

public class MusicLibrary : MonoBehaviour
{
    public MusicTrack[] tracks;


    public AudioClip GetRandomClip()
    {
        if (tracks != null && tracks.Length > 0)
        {
            int randomIndex = Random.Range(0, tracks.Length);
            return tracks[randomIndex].clip;
        }
        return null; // Falls keine Tracks vorhanden sind
    }

    public AudioClip GetClipFromName(string trackName)
    {
        foreach (var track in tracks)
        {
            if (track.trackName == trackName)
            {
                return track.clip;
            }
        }
        return null;
    }
}