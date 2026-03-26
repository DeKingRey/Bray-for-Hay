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
    private VolumeDetector voiceDetector;

    void Start()
    {
        camSensitivity = FindObjectOfType<PlayerCam>();
        voiceDetector = FindObjectOfType<VolumeDetector>();
        PopulateMicDropdown();

        #region Getting Saved Values

        float savedCamSensitivity = PlayerPrefs.GetFloat("CamSensitivity", 0.5f);
        float savedMicSensitivity = PlayerPrefs.GetFloat("MicSensitivity", 200f);

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

        if (voiceDetector != null)
        {
            voiceDetector.micMultiplier = savedMicSensitivity;
        }
        micSensitivitySlider.value = savedMicSensitivity;

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
        micInputDropdown.onValueChanged.AddListener(SetMicInput);

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSoundFXVolume);

        #endregion
    }

    void PopulateMicDropdown()
    {
        micInputDropdown.ClearOptions();

        if (Microphone.devices.Length == 0)
        {
            micInputDropdown.AddOptions(new List<string> {"No Microphone Found"});
            micInputDropdown.interactable = false;
            return;
        }

        List<string> mics = new List<string>();

        // Adds dropdown options
        foreach (string device in Microphone.devices)
        {
            mics.Add(device);
        }

        micInputDropdown.AddOptions(mics);

        // Loads saved mic
        string savedMic = PlayerPrefs.GetString("MicInput", Microphone.devices.Length > 0 ? Microphone.devices[0] : "");

        int index = mics.IndexOf(savedMic);
        if (index < 0 ) index = 0;

        micInputDropdown.value = index;
        micInputDropdown.RefreshShownValue();

        SetMicInput(index);
    }

    public void SetCamSensitivity(float value)
    {
        if (camSensitivity != null) camSensitivity.sensitivity = value;
        PlayerPrefs.SetFloat("CamSensitivity", value);
        PlayerPrefs.Save();
    }

    public void SetMicSensitivity(float value)
    {
        if (voiceDetector != null) voiceDetector.micMultiplier = value;
        PlayerPrefs.SetFloat("MicSensitivity", value);
        PlayerPrefs.Save();
    }

    public void SetMicInput(int index)
    {
        string selectedMic = Microphone.devices[index];

        if (voiceDetector != null)
        {
            voiceDetector.selectedMic = selectedMic;
            voiceDetector.MicrophoneToAudioClip();
        }
        PlayerPrefs.SetString("MicInput", selectedMic);
        PlayerPrefs.Save();
    }

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
