using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathAndWinnerScreen : MonoBehaviour
{
    public void PlayAgain(){
        SceneManager.LoadScene("Dungeon");
    }

    public void MainMenu(){
        SceneManager.LoadScene("StartingScreen");
    }
}
