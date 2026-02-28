using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState State;
    public static event Action<GameState> OnGameStateChanged;

    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        Menu
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ChangeState(GameState newState, float delay)
    {
        if (State == newState) return;

        StartCoroutine(TransitionToState(newState, delay));
    }

    private IEnumerator TransitionToState(GameState newState, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        State = newState;
        HandleStateChange();
    }

    private void HandleStateChange()
    {
        switch (State)
        {
            case GameState.Playing:
                break;
            case GameState.Paused:  
                break;
            case GameState.GameOver:
                //SceneManager.LoadScene("Game");
                //ChangeState(GameState.Playing, 0);
                break;
            case GameState.Menu:
                break;
        }
        
        // Event Signal
        OnGameStateChanged?.Invoke(State);
    }

    /// Loads scene depending on index to add
    /// If next level is needed to be loaded then index to add = 1
    /// If current scene needs to be reloaded index to add = 0
    public void LoadScene(int indexToAdd)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        int loadSceneIndex = currentSceneIndex + indexToAdd;

        // Loads scene if possible
        if (loadSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(loadSceneIndex);
        } else Debug.Log("No more scenes");
    }
}
