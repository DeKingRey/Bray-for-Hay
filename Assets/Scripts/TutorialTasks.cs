using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Task 
{
    public GameObject door;
    public UnityEvent RunTask; // Holds task function to run
    public UnityEvent OnComplete; // Opens door when completed task (usually)
}

public class TutorialTasks : MonoBehaviour
{
    [SerializeField] private Task[] tasks;
    private int currentTaskIndex = 0;

    void Update()
    {
        tasks[currentTaskIndex].RunTask.Invoke();
    }

    public void CompleteTask()
    {
        if (currentTaskIndex < tasks.Length - 1) currentTaskIndex++;

        OpenDoor();
    }

    void OpenDoor()
    {
        Debug.Log("Opening Door");
    }

    /// Player has to read context first
    public void ContextTask()
    {
        Dialogue contextDialogue = GameObject.FindWithTag("Context")?.GetComponent<Dialogue>();
        if (contextDialogue.dialogueComplete)
            tasks[currentTaskIndex].OnComplete.Invoke();
    }
}
