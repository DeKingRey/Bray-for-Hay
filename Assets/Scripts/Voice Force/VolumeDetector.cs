using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeDetector : MonoBehaviour
{
    [SerializeField] private int sampleWindow = 64;
    public float minVolume = 0.1f;
    public float maxVolume = 5f;
    public float micMultiplier = 10f;
    [SerializeField] private float maxNoiseRadius;

    [HideInInspector] public float currentNoiseRadius;
    private float loudness;

    private AudioClip microphoneClip;
    
    void Start()
    {
        MicrophoneToAudioClip();
    }

    void Update()
    {
        loudness = VolumeFromMicrophone() * micMultiplier;

        currentNoiseRadius = Mathf.Clamp(loudness * 10f, 0, maxNoiseRadius);
    }

    void MicrophoneToAudioClip()
    {
        // Gets the first microphone on device
        string microphoneName = Microphone.devices[0];

        // Records mic audio constantly
        microphoneClip = Microphone.Start(microphoneName, true, 20, AudioSettings.outputSampleRate);
    }

    public float VolumeFromMicrophone()
    {
        return VolumeFromClip(Microphone.GetPosition(Microphone.devices[0]), microphoneClip);
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
