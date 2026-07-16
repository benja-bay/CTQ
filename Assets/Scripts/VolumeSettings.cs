using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("Referencias")]
    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        float savedMusic = PlayerPrefs.GetFloat("MusicVol", 1f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVol", 1f);
        
        musicSlider.value = savedMusic;
        sfxSlider.value = savedSFX;
        
        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);
        
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float volume)
    {
        float dbVolume = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
        mainMixer.SetFloat("MusicVolume", dbVolume);
        
        PlayerPrefs.SetFloat("MusicVol", volume);
    }

    public void SetSFXVolume(float volume)
    {
        float dbVolume = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
        mainMixer.SetFloat("SFXVolume", dbVolume);
        
        PlayerPrefs.SetFloat("SFXVol", volume);
    }
}