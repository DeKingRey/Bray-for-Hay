using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState State;
    public LevelType Level;
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<int> OnSceneChanged;

    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        Menu,
        LevelComplete
    }

    public enum LevelType
    {
        Paddock,
        Mountains,
        Other
    }

    [SerializeField] private AudioClip gameOverSfx;

    private Animator gameOverAnim;
    private bool gameOver;
    private bool previousGameOver;

    private Animator levelCompleteAnim;
    private bool levelComplete;
    private bool previousLevelComplete;

    private bool fadeOut = false;

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

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (fadeOut)
        {
            Animator fadeAnim = GameObject.FindWithTag("Fade Screen").GetComponent<Animator>();
            fadeAnim.SetTrigger("FadeOut");
            fadeOut = false;
        }

        // Event Signal
        OnSceneChanged?.Invoke(newScene.buildIndex);
    }

    void Update()
    {
        if (!gameOverAnim)
        {
            GameObject gameOverUI = GameObject.FindWithTag("Game Over");
            if (gameOverUI != null) gameOverAnim = gameOverUI.GetComponent<Animator>();

            // Causes fade out when you just lost the game
            if (previousGameOver)
            {
                gameOverAnim.SetBool("previousGameOver", true);
                previousGameOver = false;
            }
        }

        if (!levelCompleteAnim)
        {
            GameObject levelCompleteUI = GameObject.FindWithTag("Level Complete");
            if (levelCompleteUI != null) levelCompleteAnim = levelCompleteUI.GetComponent<Animator>();

            // Causes fade out to play when player transitions between levels (specifically only levels)
            if (previousLevelComplete)
            {
                levelCompleteAnim.SetBool("previousComplete", true);
                previousLevelComplete = false;
            }
        }

        // Restarts level if space is pressed after game over
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeState(GameState.Playing, 0);
                gameOver = false;

                previousGameOver = true;
                LoadScene(0);
            }
        }
        
        if (levelComplete)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeState(GameState.Playing, 0);
                levelComplete = false;

                previousLevelComplete = true;
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
                Transform player = GameObject.FindWithTag("Player").GetComponent<Transform>();
                SoundManager.Instance.PlayAudio(gameOverSfx, 0.4f, player, 0);
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
    public void LoadScene(int indexToAdd, float delay = 0f)
    {
        StartCoroutine(TransitionToLevel(indexToAdd, delay));
    }

    private IEnumerator TransitionToLevel(int indexToAdd, float delay)
    {
        // Will fade in/out to transition between scenes
        if (delay > 0f) 
        {
            Animator fadeAnim = GameObject.FindWithTag("Fade Screen").GetComponent<Animator>();
            fadeAnim.SetTrigger("FadeIn");
            fadeOut = true;
        }

        yield return new WaitForSeconds(delay);
        
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        int loadSceneIndex = currentSceneIndex + indexToAdd;

        // Loads scene if possible
        if (loadSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(loadSceneIndex);
        } else
        {
            // Reloads menu when complete
            ChangeState(GameState.Menu, 0f);
            SceneManager.LoadScene(0);
        }
    }
}
