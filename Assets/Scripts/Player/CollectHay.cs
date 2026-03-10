using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectHay : MonoBehaviour
{
    [SerializeField] private float hayHoldTime = 3f;
    [SerializeField] private Slider holdSlider;
    [SerializeField] private Animator sliderAnim;
    [SerializeField] private bool playCutscene = false;
    
    private bool playerTouching;
    private float elapsedHoldTime;

    void Update()
    {
        // Player has to hold down E to collect hay
        if (playerTouching)
        {
            if (Input.GetKey(KeyCode.E))
            {
                elapsedHoldTime += Time.deltaTime;
                if (elapsedHoldTime >= hayHoldTime)
                {
                    if (!playCutscene) GameManager.Instance.LoadScene(1);
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
