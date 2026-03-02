using UnityEngine;

namespace UnityWebGLMicrophone
{
    public class DisplayMics : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        void Awake()
        {
            Microphone.Init();
            Microphone.QueryAudioInput();
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        void Update()
        {
            Microphone.Update();

            string[]
        }
#endif
    }
}
