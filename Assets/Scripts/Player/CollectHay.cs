using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectHay : MonoBehaviour
{
    [Header("Collection")]
    [SerializeField] private float hayHoldTime = 3f;
    [SerializeField] private Slider holdSlider;
    [SerializeField] private Animator sliderAnim;
    [SerializeField] private AudioClip collectSfx;

    [Space(10)]

    [Header("Cutscenes")]
    [SerializeField] private bool playCutscene = false;
    [SerializeField] private int cutsceneIndex;
    
    private bool playerTouching;
    private float elapsedHoldTime;
    private PlayerCutscenes cutscenePlayer;
    private bool collectedHay = false;

    void Start()
    {
        cutscenePlayer = FindObjectOfType<PlayerCutscenes>();
    }

    void Update()
    {
        // Player has to hold down E to collect hay
        if (playerTouching)
        {
            if (Input.GetKey(KeyCode.E))
            {
                elapsedHoldTime += Time.deltaTime;
                // Loads next scene once completed unless there is a cutscene to play
                if (elapsedHoldTime >= hayHoldTime && !collectedHay)
                {
                    if (!playCutscene)
                        GameManager.Instance.ChangeState(GameManager.GameState.LevelComplete, 0);
                    else cutscenePlayer.currentCutsceneIndex = cutsceneIndex;
                    SoundManager.Instance.PlayAudio(collectSfx, 0.6f, transform);

                    collectedHay = true;
                }
            } else 
            {
                elapsedHoldTime -= Time.deltaTime;
                if (elapsedHoldTime < 0) elapsedHoldTime = 0f;
            }
        } else  elapsedHoldTime = 0f;

        // Updates slider
        holdSlider.value = elapsedHoldTime / hayHoldTime;
    }

    void OnTriggerEnter(Collider obj)
    {
        if (obj.CompareTag("Player"))
        {
            playerTouching = true;
            Debug.Log("touching player");
            sliderAnim.SetBool("active", true);
        }
    }

    void OnTriggerExit(Collider obj)
    {
        if (obj.CompareTag("Player"))
        {
            playerTouching = false;
            sliderAnim.SetBool("active", false);
        }
    }
}
