using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Fuentes de Audio (Asignar en Inspector)")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource timeWarningSource;

    [Header("Música (BGM)")]
    public AudioClip menuMusic;
    public AudioClip lobbyMusic;
    public AudioClip gameMusic;

    [Header("Efectos Globales (SFX)")]
    public AudioClip buttonClickSound;
    public AudioClip orbPickupSound;
    public AudioClip countdownBeep;
    public AudioClip countdownGo;
    public AudioClip timeRunningOutSound;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null || musicSource.clip == musicClip) return;
        musicSource.clip = musicClip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null) sfxSource.PlayOneShot(clip, volume);
    }
    
    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    public void StartTimeWarning()
    {
        if (!timeWarningSource.isPlaying && timeRunningOutSound != null)
        {
            timeWarningSource.clip = timeRunningOutSound;
            timeWarningSource.loop = true;
            timeWarningSource.Play();
        }
    }
    
    public void StopTimeWarning()
    {
        if (timeWarningSource.isPlaying)
        {
            timeWarningSource.Stop();
        }
    }
}