using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] InputManager InputMainMenu;

    private void Start()
    {
        if (InputMainMenu != null)
        {
            InputMainMenu.OnMainMenuInput += BackToMainMenu;
        }
    }
    private void OnDestroy()
    {
        if (InputMainMenu != null)
        {
            InputMainMenu.OnMainMenuInput -= BackToMainMenu;

        }
    }
    private void BackToMainMenu ()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }
}
