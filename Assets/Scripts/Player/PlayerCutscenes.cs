using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Cutscene
{
    public float targetY;
    public float speed;
}

public class PlayerCutscenes : MonoBehaviour
{
    [SerializeField] private Cutscene[] cutscenes;
    public int currentCutsceneIndex = -1;

    void Update()
    {
        if (currentCutsceneIndex != -1)
            PlayCurrentCutscene();
    }
    
    void PlayCurrentCutscene()
    {
        GetComponent<PlayerController>().enabled = false;
        Cutscene cs = cutscenes[currentCutsceneIndex];
        Debug.Log("cutsceneplaying");
        Vector3 targetPosition = new Vector3(transform.position.x, cs.targetY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, 1 - Mathf.Exp(-cs.speed * Time.deltaTime));

        if (Vector3.Distance(transform.position, targetPosition) < 10f)
            GameManager.Instance.ChangeState(GameManager.GameState.LevelComplete, 0f);
    }
}
