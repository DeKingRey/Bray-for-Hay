using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField] private Slider camSensitivitySlider;
    [SerializeField] private Slider micSensitivitySlider;
    [SerializeField] private TMP_Dropdown micInputDropdown;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private PlayerCam camSensitivity;
    private VolumeDetector micSensitivity;

    void Start()
    {
        #region Getting Saved Values

        camSensitivity = FindObjectOfType<PlayerCam>();
        micSensitivity = FindObjectOfType<VolumeDetector>();

        float savedCamSensitivity = PlayerPrefs.GetFloat("CamSensitivity", 0.5f);
        float savedMicSensitivity = PlayerPrefs.GetFloat("MicSensitivity", 200f);

        /*int savedQuality = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(savedQuality, true);*/

        float savedMasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        #endregion

        #region Settting Saved Values

        if (camSensitivity != null)
        {
            camSensitivity.sensitivity = savedCamSensitivity;
        }
        camSensitivitySlider.value = savedCamSensitivity;

        if (micSensitivity != null)
        {
            micSensitivity.micMultiplier = savedMicSensitivity;
        }
        micSensitivitySlider.value = savedMicSensitivity;

        /*qualityDropdown.value = savedQuality;
        qualityDropdown.RefreshShownValue();*/

        SetMasterVolume(savedMasterVolume);
        masterSlider.value = savedMasterVolume;

        SetMusicVolume(savedMusicVolume);
        musicSlider.value = savedMusicVolume;

        SetSoundFXVolume(savedSFXVolume);
        sfxSlider.value = savedSFXVolume;

        #endregion

        #region Event Listeners

        camSensitivitySlider.onValueChanged.AddListener(SetCamSensitivity);
        micSensitivitySlider.onValueChanged.AddListener(SetMicSensitivity);
        //qualityDropdown.onValueChanged.AddListener(SetQuality);

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSoundFXVolume);

        #endregion
    }

    public void SetCamSensitivity(float value)
    {
        camSensitivity.sensitivity = value;
        PlayerPrefs.SetFloat("CamSensitivity", value);
        PlayerPrefs.Save();
    }

    public void SetMicSensitivity(float value)
    {
        micSensitivity.micMultiplier = value;
        PlayerPrefs.SetFloat("MicSensitivity", value);
        PlayerPrefs.Save();
    }

    /*public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("Quality", index);
        PlayerPrefs.Save();
    }*/

    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("masterVolume", Mathf.Log10(value) * 20f);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("musicVolume", Mathf.Log10(value) * 20f);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSoundFXVolume(float value)
    {
        audioMixer.SetFloat("soundFXVolume", Mathf.Log10(value) * 20f);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}
