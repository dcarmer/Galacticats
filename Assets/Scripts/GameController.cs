using UnityEngine;

/*
 * Instantiate Players
 * Paused/not paused during menu?
 */

public class GameController : MonoBehaviour 
{
    private int numPlayers = 0;
    public GameObject StartMenu, PauseMenu;
    public GameObject PlayerPrefab;
    //Variable Player Number Screen Configurations
    private readonly Rect[][] SplitScreenConfig = new Rect[4][]
    {
        new Rect[1] { new Rect(0,   0,   1,   1) },
        new Rect[2] { new Rect(0, .5f,   1, .5f), new Rect(  0,   0,   1, .5f) },
        new Rect[3] { new Rect(0, .5f, .5f, .5f), new Rect(.5f, .5f, .5f, .5f), new Rect(0, 0,   1, .5f) },
        new Rect[4] { new Rect(0, .5f, .5f, .5f), new Rect(.5f, .5f, .5f, .5f), new Rect(0, 0, .5f, .5f), new Rect(.5f, 0, .5f, .5f) }
    };


    private void Start()
    {
        StartMenu.SetActive(true);
    }
    private void Update()
    {
        if(Input.GetButtonDown("Cancel") && !StartMenu.activeInHierarchy)
        {
            PauseGame(true);
        }
    }

    public void StartGame(int players) //Only called from start menu
    {
        StartMenu.SetActive(false);
        switch (players)
        {
            default: //AKA Solo
                numPlayers = 1;
                //PlayerScript = Instantiate<PlayerScript>(PlayerPrefab);
                //PlayerScript.Camera.rect = SplitScreenConfig[0][0];
                //solo stuff here
                break;
            case 2:
                numPlayers = 2;
                //PlayerScript1 = Instantiate<PlayerScript>(PlayerPrefab);
                //PlayerScript1.Camera.rect = SplitScreenConfig[1][0];
                //PlayerScript2 = Instantiate<PlayerScript>(PlayerPrefab);
                //PlayerScript2.Camera.rect = SplitScreenConfig[1][1];
                //Coop stuff here
                break;
        }
    }
    public void PauseGame(bool value)
    {
        if(value)
        {
            Time.timeScale = 0;
            PauseMenu.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            PauseMenu.SetActive(false);
        }
        
    }
    public void QuitGame()
    {
        //Ouit stuff Here
        PauseGame(false);
        PauseMenu.SetActive(false);
        StartMenu.SetActive(true);
    }
    public void ExitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
