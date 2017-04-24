using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class point : MonoBehaviour {

    public Text pointText;
    // Use this for initialization
    void Start () {
        pointText.text = "Points: " + GameWideScore.score; 
	}
	
	// Update is called once per frame
	void Update () {
        pointText.text = "Points: " + GameWideScore.score;
    }
}
