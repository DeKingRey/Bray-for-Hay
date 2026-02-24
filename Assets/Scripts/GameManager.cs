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
}
