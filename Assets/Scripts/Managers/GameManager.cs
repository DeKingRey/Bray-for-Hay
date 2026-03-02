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
        Menu,
        LevelComplete
    }

    private Animator gameOverAnim;
    private bool gameOver;

    private Animator levelCompleteAnim;
    private bool levelComplete;

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

    void Update()
    {
        if (!gameOverAnim)
        {
            GameObject gameOverUI = GameObject.FindWithTag("Game Over");
            if (gameOverUI != null) gameOverAnim = gameOverUI.GetComponent<Animator>();
        }
        if (!levelCompleteAnim)
        {
            GameObject levelCompleteUI = GameObject.FindWithTag("Level Complete");
            if (levelCompleteUI != null) levelCompleteAnim = levelCompleteUI.GetComponent<Animator>();
        }

        // Restarts level if space is pressed after game over
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeState(GameState.Playing, 0);
                gameOver = false;
                LoadScene(0);
            }
        }
        
        if (levelComplete)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeState(GameState.Playing, 0);
                levelComplete = false;
                LoadScene(1);
            }
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
                gameOver = true;
                gameOverAnim.SetTrigger("Activate");
                break;
            case GameState.Menu:
                break;
            case GameState.LevelComplete:
                levelComplete = true;
                levelCompleteAnim.SetTrigger("Activate");
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
        } else SceneManager.LoadScene(0); // Loads initial scene when complete
    }
}
