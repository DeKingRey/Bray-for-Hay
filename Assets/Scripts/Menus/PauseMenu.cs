using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;  

public class PauseMenu : MonoBehaviour 
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject optionsMenuUI;
    [SerializeField] private GameObject controlsUI;

    private bool isPaused;

    void Update()
    {
        if (GameManager.Instance.State == GameManager.GameState.GameOver) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }


    public void Resume()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Playing, 0f);

        pauseMenuUI.SetActive(false); 
        optionsMenuUI.SetActive(false);
        controlsUI.SetActive(false);
        
        // Disables Cursor
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;

        Time.timeScale = 1f; 
        isPaused = false;
    }
 

    public void Pause()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Paused, 0f);
        pauseMenuUI.SetActive(true);

        // Enables Cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
        isPaused = true;
    }


    public void LoadMenu()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Menu, 0.5f);

        Time.timeScale = 1f;
        isPaused = false;

        // Enables Cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameManager.Instance.LoadScene(-1, 0.5f);
    }

    public void Quit()
    {
        Application.Quit();
    }
}