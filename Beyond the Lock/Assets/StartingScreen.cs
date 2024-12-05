using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartingScreen : MonoBehaviour
{
    public void PlayGame(){
        SceneManager.LoadScene("MAIN GAME", LoadSceneMode.Single);
    }
}
