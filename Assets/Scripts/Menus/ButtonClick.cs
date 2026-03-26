using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClick : MonoBehaviour
{
    [SerializeField] private AudioClip clickSfx;
    [SerializeField] private float clickVolume;
    [SerializeField] private AudioSource source;

    void Start()
    {
        if (source == null)
            source = GetComponent<AudioSource>();
    }

    public void OnClick()
    {
        SoundManager.Instance.PlayAudio(clickSfx, clickVolume, null, 0);
    }
}
