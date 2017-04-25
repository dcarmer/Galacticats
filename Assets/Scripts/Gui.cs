using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gui : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }




    public void PlayGame()
    {
        SceneManager.LoadScene("Level1", LoadSceneMode.Single);
    }

    public void quitGame()
    {
        Application.Quit();
    }

}
    
