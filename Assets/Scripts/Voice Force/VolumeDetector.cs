using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FirstGearGames.SmoothCameraShaker;

public class VolumeDetector : MonoBehaviour
{
    [Header("Microphone Force Settings")]
    [Tooltip("'Strength' of microphone (sensitivity)")]
    public float micMultiplier = 200f;

    [Tooltip("Adds lift to the force - so it goes up")]
    public float upwardMultiplier = 0.35f;

    [Tooltip("Adds a spin to the force - torque")]
    public float torqueMultiplier = 6f;
    public float randomTorque = 2f;


    [Header("Microphone Detection Settings")]
    public float minVolume = 0.2f;
    public float maxVolume = 5f;

    [Tooltip("Size of the audio sample chunk used for volume detection. Lower - more reactive")]
    [SerializeField] private int sampleWindow = 64;

    [Space(10)]

    [Header("Audio Enemy Detection")]
    [Tooltip("How often sound is made. E.g. from footsteps")]
    [SerializeField] private float createSoundInterval = 0.15f;
    [SerializeField] private float maxNoiseRadius = 10f;
    [SerializeField] private float radiusMultiplier = 5f;

    [Header("Juice")]
    [SerializeField] private ShakeData voiceShake;
    [SerializeField] private float volumeShakeThreshold = 1f;
    [SerializeField] private float shakeMultiplier = 5f;
    
    private float soundTimer = 0f;
    private float loudness;

    private AudioClip microphoneClip;
    [HideInInspector] public string selectedMic;
    
    void Start()
    {
        if (Microphone.devices.Length < 0)
        {
            Debug.LogWarning("No microphone found!");
        }
        if (string.IsNullOrEmpty(selectedMic)) selectedMic = Microphone.devices[0];
        MicrophoneToAudioClip();
    }

    void Update()
    {
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        // Only runs sound bubble every few frames (for performance)
        soundTimer += Time.deltaTime;
        if (soundTimer >= createSoundInterval)
        {
            // Radius size depends on volume of mic
            loudness = VolumeFromMicrophone();
            float currentNoiseRadius = Mathf.Clamp(loudness * radiusMultiplier, 0, maxNoiseRadius);

            // Screen Shake
            if (loudness > volumeShakeThreshold)
            {
                ShakerInstance instance = CameraShakerHandler.Shake(voiceShake);
                instance.MultiplyMagnitude(loudness * shakeMultiplier, -1);
            }

            if (currentNoiseRadius > 0f) SoundManager.Instance.CreateSoundBubble(currentNoiseRadius);
            soundTimer = 0f;
        }
    }

    public void MicrophoneToAudioClip()
    {
        // Records mic audio constantly
        microphoneClip = Microphone.Start(selectedMic, true, 20, AudioSettings.outputSampleRate);
    }

    public float VolumeFromMicrophone()
    {
        return Mathf.Clamp(VolumeFromClip(Microphone.GetPosition(selectedMic), microphoneClip), 0, maxVolume);
    }

    float VolumeFromClip(int clipPosition, AudioClip clip)
    {
        // Gets recent data from the clip within the sample window
        int startPosition = clipPosition - sampleWindow;

        if (startPosition < 0) return 0;

        float[] waveData = new float[sampleWindow];
        clip.GetData(waveData, startPosition);

        // Computes total volume
        float totalVolume = 0;

        for (int i = 0; i < sampleWindow; i++)
        {
            totalVolume += Mathf.Abs(waveData[i]);
        }

        // Returns mean volume
        return totalVolume / sampleWindow;
    }
}
