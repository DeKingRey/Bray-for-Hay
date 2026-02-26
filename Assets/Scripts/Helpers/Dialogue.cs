using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Stores info for specific dialogue instance
public class Dialogue : MonoBehaviour
{
    [HideInInspector] public int dialogueIndex = 0;
    [HideInInspector] public bool textComplete = false; // For when current textbox is complete
    [HideInInspector] public bool dialogueComplete = false; // For when all dialogue is complete
    
    [SerializeField] private TextMeshProUGUI textArea;
    [SerializeField] private string[] dialogueTexts;

    private bool hasStarted = false;

    public IEnumerator TypeLetters(Dictionary<char, float> intervals, float defaultInterval)
    {
        hasStarted = true;
        textComplete = false;

        // Sets all letters to invisible
        textArea.maxVisibleCharacters = 0;
        textArea.text = dialogueTexts[dialogueIndex];

        // Force layout update
        textArea.ForceMeshUpdate();
        int total = textArea.textInfo.characterCount; // Gets total characters

        // Reveals each letter every few milliseconds
        for (int i = 0; i < total; i++)
        {
            // Max visible characters is used to avoid text wrapping on long words (so text is preset)
            textArea.maxVisibleCharacters = i + 1;

            char c = textArea.text[i];
            float letterInterval = defaultInterval; // Default interval

            if (intervals.ContainsKey(c))
                letterInterval = intervals[c];

            yield return new WaitForSeconds(letterInterval);
        }

        // Will stop playing text when all text boxs done
        if (dialogueIndex == dialogueTexts.Length - 1) 
            dialogueComplete = true;
        else 
            dialogueIndex++;

        textComplete = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasStarted)
        {
            DialogueManager.Instance.SetCurrentDialogue(this);
        }
    }
}
