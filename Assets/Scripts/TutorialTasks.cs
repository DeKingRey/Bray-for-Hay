using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Task 
{
    public string taskName;
    public Animator doorAnim;
    public UnityEvent RunTask; // Holds task function to run
    public UnityEvent OnComplete; // Opens door when completed task (usually)
}

public class Box
{
    public Transform transform;
    public Vector3 initialPos;

    // Constructor
    public Box(Transform transform, Vector3 initialPos)
    {
        this.transform = transform;
        this.initialPos = initialPos;
    }
}

public class TutorialTasks : MonoBehaviour
{
    [SerializeField] private Task[] tasks;
    private int currentTaskIndex = 0;
    private bool tasksComplete;

    private List<Box> boxes = new List<Box>();

    void Start()
    {
        GameObject[] boxObjs = GameObject.FindGameObjectsWithTag("Force Box");
        foreach (GameObject box in boxObjs)
        {
            // Stores transforms and intial positions of boxes
            Transform boxTransform = box.GetComponent<Transform>();
            Box newBox = new Box(boxTransform, boxTransform.position);
            boxes.Add(newBox);
        }
    }

    void Update()
    {
        if (tasksComplete) return;

        tasks[currentTaskIndex].RunTask.Invoke();
    }

    public void CompleteTask()
    {
        OpenDoor();
        if (currentTaskIndex < tasks.Length - 1) currentTaskIndex++;
        else tasksComplete = true;
    }

    void OpenDoor()
    {
        tasks[currentTaskIndex].doorAnim.SetTrigger("OpenDoor");
    }

    /// Player has to read context first
    public void ContextTask()
    {
        Dialogue contextDialogue = GameObject.FindWithTag("Context")?.GetComponent<Dialogue>();
        if (contextDialogue.dialogueComplete)
            tasks[currentTaskIndex].OnComplete.Invoke();
    }

    /// Player has to jump
    public void JumpTask()
    {
        if (Input.GetKeyDown(KeyCode.Space)) tasks[currentTaskIndex].OnComplete.Invoke();
    }

    // Player has to sprint
    public void SprintTask()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) tasks[currentTaskIndex].OnComplete.Invoke();
    }

    /// Player has to crouch
    public void CrouchTask()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl)) tasks[currentTaskIndex].OnComplete.Invoke();
    }

    /// Player has to use voice to blow away box
    public void VoiceTask()
    {
        // Checks if any boxes have been moved
        foreach (Box box in boxes)
        {
            if (Vector3.Distance(box.transform.position, box.initialPos) > 1f) 
                tasks[currentTaskIndex].OnComplete.Invoke();
        }
    }

    public void EnemyTask()
    {
        Dialogue enemyDialogue = GameObject.FindWithTag("Enemy Dialogue")?.GetComponent<Dialogue>();
        if (enemyDialogue.dialogueComplete)
            tasks[currentTaskIndex].OnComplete.Invoke();
    }
}
