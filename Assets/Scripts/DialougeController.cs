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
    private GameObject[] stations;
    public int numOfStations = 0;


    // Use this for initialization
    void Start () {
        stations = GameObject.FindGameObjectsWithTag("Station");
        numOfStations = stations.Length;
        fancyFeast.SetActive(true);
        flag = false;
        StartCoroutine(Disapear(fancyFeast));
	}

   

    // Update is called once per frame
    void Update () {
        if (flag)
        {
            if (numOfStations == 1)
            {
                
                hairBall.SetActive(true);
                hairBallText.text = "Lt. Hair Ball: There are " + numOfStations + " space stations left!";
                StartCoroutine(Disapear(hairBall));
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


}
