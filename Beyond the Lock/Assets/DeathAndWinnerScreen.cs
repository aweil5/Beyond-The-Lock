using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathAndWinnerScreen : MonoBehaviour
{
    void Start()
    {
        // Unlock and show the cursor when the death screen is loaded
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlayAgain()
    {
        // Lock the cursor again when transitioning back to the game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene("MAIN GAME");
    }

    public void MainMenu()
    {
        // Unlock the cursor when transitioning to the main menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("StartingScreen");
    }
}
