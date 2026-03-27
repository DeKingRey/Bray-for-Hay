using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoiceFeedbackController : MonoBehaviour
{
    [Header("Mic Input Slider")]
    [SerializeField] private float smoothSpeed = 10f;

    [Space(10)]

    [Header("Voice Particles")]
    [SerializeField] private int minBurstAmount;
    [SerializeField] private int maxBurstAmount;
    [SerializeField] private ParticleSystem voiceParticles;

    private Slider slider;
    private VolumeDetector volumeDetector;
    private float smoothedVolume;

    void Start()
    {
        volumeDetector = FindObjectOfType<VolumeDetector>();
        slider = GameObject.FindWithTag("Mic Slider").GetComponent<Slider>();
        slider.maxValue = volumeDetector.maxVolume;

        voiceParticles.Stop();
    }

    void Update()
    {
        float rawVolume = volumeDetector.VolumeFromMicrophone();
        if (rawVolume < volumeDetector.minVolume) rawVolume = 0f;

        smoothedVolume = Mathf.Lerp(smoothedVolume, rawVolume, smoothSpeed * Time.deltaTime);

        slider.value = Mathf.Clamp(smoothedVolume, 0f, slider.maxValue);

        if (rawVolume <= 0f || voiceParticles.isPlaying) return;

        var emission = voiceParticles.emission;
        int burstAmount = Mathf.RoundToInt(Mathf.Clamp(rawVolume * 30f, minBurstAmount, maxBurstAmount));
        ParticleSystem.Burst burst = new ParticleSystem.Burst(0f, burstAmount);
        emission.SetBursts(new ParticleSystem.Burst[] { burst });

        Debug.Log(burstAmount);

        voiceParticles.Play();
    }   
}
