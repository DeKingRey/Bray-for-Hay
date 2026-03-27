using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None; // Enables cursor within the screen
        Cursor.visible = true;
    }

    public void Play()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Playing, 0.5f);
        GameManager.Instance.LoadScene(1, 0.5f); // Loads first level
    }

    public void Quit()
    {
        Application.Quit();
    }
}
