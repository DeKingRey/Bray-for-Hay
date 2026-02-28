using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Interval
{
    public char character;
    public float interval;
}

/// Handles dialogue within the scene
/// Updates current dialogue being displayed
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [SerializeField] private KeyCode[] nextDialogueKeys;
    [SerializeField] private GameObject dialogueBox;

    [SerializeField] private float defaultInterval = 0.05f;
    [SerializeField] private Interval[] intervals;
    private Dictionary<char, float> intervalLookup = new Dictionary<char, float>();
    
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

        foreach (var ci in intervals)
        {
            intervalLookup[ci.character] = ci.interval;
        }
    }

    void Update()
    {
        if (currentDialogue != null)
        {
            // Handles dialogue input
            // If all dialogue is complete, the text box will disappear
            // If the dialogue is complete, the next will start
            // If the key is pressed early, the dialogue will skip
            foreach (KeyCode key in nextDialogueKeys)
            {
                if (Input.GetKeyDown(key) || Input.GetMouseButtonDown((0)))
                {
                    if (currentDialogue.dialogueComplete)
                    {
                        currentDialogue = null;
                        dialogueBox.SetActive(false);
                        return;
                    } else if (currentDialogue.textComplete) 
                        currentDialogue.StartDialogue(intervalLookup, defaultInterval);
                    else currentDialogue.SkipDialogue();
                        
                }
            }
        }
    }

    public void SetCurrentDialogue(Dialogue dialogue)
    {
        currentDialogue = dialogue;

        dialogueBox.SetActive(true);

        // Auto starts letter typing
        currentDialogue.StartDialogue(intervalLookup, defaultInterval);
    }
}
