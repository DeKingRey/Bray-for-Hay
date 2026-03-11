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

    [Space(10)]

    [Header("Cutscenes")]
    [SerializeField] private bool playCutscene = false;
    [SerializeField] private int cutsceneIndex;
    
    private bool playerTouching;
    private float elapsedHoldTime;
    private PlayerCutscenes cutscenePlayer;

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
                if (elapsedHoldTime >= hayHoldTime)
                {
                    if (!playCutscene) GameManager.Instance.LoadScene(1);
                    else cutscenePlayer.currentCutsceneIndex = cutsceneIndex;
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
