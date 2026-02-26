using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Handles dialogue within the scene
/// Updates current dialogue being displayed
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [SerializeField] private KeyCode[] nextDialogueKeys;
    [SerializeField] private float letterInterval = 0.1f;
    [SerializeField] private GameObject dialogueBox;
    private Dialogue currentDialogue;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (currentDialogue != null)
        {
            // Plays dialogue if previous is complete and dialogue key is pressed
            if (currentDialogue.textComplete)
            {
                foreach (KeyCode key in nextDialogueKeys)
                {
                    if (Input.GetKeyDown(key))
                    {
                        if (currentDialogue.dialogueComplete)
                        {
                            currentDialogue = null;
                            dialogueBox.SetActive(false);
                            return;
                        } else
                            currentDialogue.StartCoroutine(currentDialogue.TypeLetters(letterInterval));
                    }
                }
            }
        }
    }

    public void SetCurrentDialogue(Dialogue dialogue)
    {
        currentDialogue = dialogue;

        dialogueBox.SetActive(true);

        // Auto starts letter typing
        currentDialogue.StartCoroutine(currentDialogue.TypeLetters(letterInterval));
    }
}
