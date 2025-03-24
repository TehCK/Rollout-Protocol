using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    public AudioMixer audioMixer;

    [Header("Master Volume")]
    public Slider masterVolumeSlider;

    [Header("Music")]
    public Slider musicSlider;

    [Header("SFX")]
    public Slider sfxSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("masterVolume"))
            LoadMasterVolume();
        else
            SetMasterVolume();

        if (PlayerPrefs.HasKey("musicVolume"))
            LoadMusicVolume();
        else
            SetMusicVolume();

        if (PlayerPrefs.HasKey("sfxVolume"))
            LoadSFXVolume();
        else
            SetSFXVolume();
    }

    public void SetMasterVolume()
    {
        float volume = masterVolumeSlider.value;
        audioMixer.SetFloat("master", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("masterVolume", volume);
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        audioMixer.SetFloat("music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume()
    {
        float volume = sfxSlider.value;
        audioMixer.SetFloat("sfx", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    private void LoadMasterVolume()
    {
        masterVolumeSlider.value = PlayerPrefs.GetFloat("masterVolume");
        SetMasterVolume();
    }

    private void LoadMusicVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        SetMusicVolume();
    }

    private void LoadSFXVolume()
    {
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
        SetSFXVolume();
    }

}
