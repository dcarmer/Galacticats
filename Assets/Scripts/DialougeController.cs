using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialougeController : MonoBehaviour {

    public bool flag= true;
    public GameObject fancyFeast;
    public GameObject hairBall;
    public Text hairBallText;
    public GameObject death;
    public Text deathText;
    private GameObject[] stations;
    public int numOfStations = 0;
    public int numOfStationsOld = 0;
    public GameObject levelSplash;
    public Text levelText;

    public GameObject player;


    // Use this for initialization
    void Start () {
        int i = Application.loadedLevel;
        stations = GameObject.FindGameObjectsWithTag("Station");
        numOfStations = stations.Length;
        numOfStationsOld = numOfStations;

        levelSplash.SetActive(true);
        levelText.text = "LEVEL " + (i + 1);
        StartCoroutine(LevelSplash(levelSplash));



        //Application.LoadLevel(i + 1);
        
    }

   

    // Update is called once per frame
    void Update () {
        stations = GameObject.FindGameObjectsWithTag("Station");
        numOfStations = stations.Length;
        if (numOfStations == (numOfStationsOld -1))
            {
            if (flag)
            {

                hairBall.SetActive(true);
                hairBallText.text = "Lt. Hair Ball: There are " + numOfStations + " space stations left!";
                if (numOfStations == 0)
                {
                    StartCoroutine(LastStation(hairBall));
                }
                else
                {
                    StartCoroutine(Disapear(hairBall));
                }
                
                numOfStationsOld = numOfStations;
            }
            }

        if (PlayerMove.dead == true)
        {
            if (flag)
            {
                death.SetActive(true);
                deathText.text = "Cpt. Wonder Puss: NOOOOO..... that was our last chance.. Your SCORE is " + GameWideScore.score + " points.";
                StartCoroutine(GameOver(death));
                player.GetComponent<PlayerMove>().enabled = false;
            }
        }
        }

		
	

    private IEnumerator Disapear(GameObject cat)
    {
        yield return new WaitForSeconds(5.0f);

        cat.SetActive(false);
        flag = true;
        //throw new NotImplementedException();
    }
    private IEnumerator GameOver(GameObject cat)
    {
        yield return new WaitForSeconds(5.0f);

        cat.SetActive(false);
        flag = true;
        Application.LoadLevel("Level1");
        //throw new NotImplementedException();
    }
    private IEnumerator LastStation(GameObject cat)
    {
        yield return new WaitForSeconds(5.0f);

        cat.SetActive(false);
        flag = true;
        int i = Application.loadedLevel;
        Application.LoadLevel(i + 1);
        //throw new NotImplementedException();
    }

    private IEnumerator LevelSplash(GameObject cat)
    {
        //pause game
        Time.timeScale = 0.0000001f;
        yield return new WaitForSeconds(0.0000003f);

        cat.SetActive(false);

        Time.timeScale = 1;
        fancyFeast.SetActive(true);
        flag = false;
        StartCoroutine(Disapear(fancyFeast));
        //int i = Application.loadedLevel;
        //Application.LoadLevel(i + 1);
        //throw new NotImplementedException();
    }



}
