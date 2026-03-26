using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicFade : MonoBehaviour
{
    private AudioSource source;
    public float fadeDuration;

    void Start()
    {
        source = GetComponent<AudioSource>();

        StartFade(0f, SoundManager.Instance.maxMusicVolume, false);
    }

    public void StartFade(float startVolume, float targetVolume, bool isSameTrack)
    {
        StartCoroutine(FadeAudio(startVolume, targetVolume, fadeDuration, isSameTrack));
    }

    private IEnumerator FadeAudio(float startVolume, float targetVolume, float fadeDuration, bool isSameTrack)
    {
        float elapsedTime = 0;
        source.volume = startVolume;

        // Only plays if the track is different (otherwise only volume changes)
        if (!isSameTrack) source.Play();

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeDuration);

            yield return null;
        }

        source.volume = targetVolume;
        if (targetVolume == 0) source.Stop();
    }
}
