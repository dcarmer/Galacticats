using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWideScore : MonoBehaviour {

    public static float score;
    private GameObject[] stations;
    public int numOfStations = 0;
    public int numOfStationsOld = 0;
    public int numMinus = 0;
    // Use this for initialization
    void Start () {
        stations = GameObject.FindGameObjectsWithTag("Station");
        numOfStations = stations.Length;
        numOfStationsOld = numOfStations;
        numMinus = numOfStationsOld - 1;

    }
	
	// Update is called once per frame
	void Update () {
        stations = GameObject.FindGameObjectsWithTag("Station");
        numOfStations = stations.Length;
        
        if (numOfStations == (numMinus))
        {
            score = score + 1000;
            numOfStationsOld = numOfStations;
            numMinus = numOfStationsOld - 1;
            Debug.Log("Score: " + score);
        }
	}
}
