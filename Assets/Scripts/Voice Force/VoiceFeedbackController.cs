using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoiceFeedbackController : MonoBehaviour
{
    [Header("Mic Input Slider")]
    [SerializeField] private Slider slider;
    [SerializeField] private float smoothSpeed = 10f;

    private VolumeDetector volumeDetector;
    private float smoothedVolume;

    void Start()
    {
        volumeDetector = FindObjectOfType<VolumeDetector>();
    }

    void Update()
    {
        float rawVolume = volumeDetector.VolumeFromMicrophone() * volumeDetector.micMultiplier;
        if (rawVolume < volumeDetector.minVolume) rawVolume = 0f;

        smoothedVolume = Mathf.Lerp(smoothedVolume, rawVolume, smoothSpeed * Time.deltaTime);

        slider.value = Mathf.Clamp(smoothedVolume, 0f, slider.maxValue);
    }   
}
