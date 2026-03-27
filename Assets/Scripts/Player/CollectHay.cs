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
    [SerializeField] private LayerMask hayLayer;

    [Space(10)]

    [Header("Cutscenes")]
    [SerializeField] private bool playCutscene = false;
    [SerializeField] private int cutsceneIndex;

    private ParticleSystem collectParticles;
    
    private bool playerTouching;
    private float elapsedHoldTime;
    private PlayerCutscenes cutscenePlayer;
    private bool collectedHay = false;

    private Camera mainCam;

    void Start()
    {
        cutscenePlayer = FindObjectOfType<PlayerCutscenes>();
        collectParticles = GetComponentInChildren<ParticleSystem>();
        mainCam = Camera.main;
        collectParticles.Stop();
    }

    void Update()
    {
        RaycastHit hit;
        // Updates slider
        if (!collectedHay) holdSlider.value = elapsedHoldTime / hayHoldTime;

        // Ensures that player is in range, in playing state, and looking at hay
        if (!playerTouching || GameManager.Instance.State != GameManager.GameState.Playing || 
            !Physics.SphereCast(mainCam.transform.position, 0.5f, mainCam.transform.forward, out hit, 10f, hayLayer))
        {
            elapsedHoldTime = 0f;
            sliderAnim.SetBool("active", false);
            return;
        }

        sliderAnim.SetBool("active", true);

        // Player has to hold down E to collect hay
        if (Input.GetKey(KeyCode.E))
        {
            elapsedHoldTime += Time.deltaTime;

            // Checks whether hay hass been  collected
            if (elapsedHoldTime >= hayHoldTime && !collectedHay)
            {
                // Loads next scene once completed unless there is a cutscene to play
                if (!playCutscene)
                    GameManager.Instance.ChangeState(GameManager.GameState.LevelComplete, 0);
                else 
                    cutscenePlayer.currentCutsceneIndex = cutsceneIndex;
                SoundManager.Instance.PlayAudio(collectSfx, 0.6f, transform, 0);

                collectParticles.Play();
                collectedHay = true;
            }
        } else 
        {
            // Decreases hold time while not holding
            elapsedHoldTime -= Time.deltaTime;
            if (elapsedHoldTime < 0) elapsedHoldTime = 0f;
        }
    }

    void OnTriggerEnter(Collider obj)
    {
        if (obj.CompareTag("Player"))
        {
            playerTouching = true;
        }
    }

    void OnTriggerExit(Collider obj)
    {
        if (obj.CompareTag("Player"))
        {
            playerTouching = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (mainCam == null) mainCam = Camera.main;

        Gizmos.color = Color.yellow;
        float sphereRadius = 0.5f;
        float maxDistance = 10f;

        // Draw the center line
        Gizmos.DrawLine(mainCam.transform.position, mainCam.transform.position + mainCam.transform.forward * maxDistance);

        // Draw spheres along the line to represent the radius
        int steps = 10;
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector3 pos = mainCam.transform.position + mainCam.transform.forward * maxDistance * t;
            Gizmos.DrawWireSphere(pos, sphereRadius);
        }
    }
}
