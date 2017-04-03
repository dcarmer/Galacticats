using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldCore : MonoBehaviour {

    bool blue;
    bool purple;
    bool yellow;
    bool red;
    int count = 0;

	// Use this for initialization
	void Start () {
        blue = true;
        purple = false;
        yellow = false;
        red = false;
        
        //red
        //gameObject.GetComponent<Renderer>().material.color =    new Color(2.0f, 0f,0f, 1.0f);
        //blue
        gameObject.GetComponent<Renderer>().material.color = new Color(0f, 0f, 2.0f, 1.0f);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "laser")
        {
            count++;
            if (count >= 6)
            {
                count = 0;
                if (blue)
                {
                    gameObject.GetComponent<Renderer>().material.color = new Color(1f, 0f, 1f, 1.0f);
                    purple = true;
                    blue = false;
                }
                else if (purple)
                {
                    gameObject.GetComponent<Renderer>().material.color = new Color(1f, 0.92f, 0.016f, 1.0f);
                    //new Color32(255, 255, 0, 1);
                    yellow = true;
                    purple = false;
                }
                else if (yellow)
                {
                    gameObject.GetComponent<Renderer>().material.color = new Color(2.0f, 0f, 0f, 1.0f);
                    red = true;
                    yellow = false;
                }
                else if (red)
                {
                    Destroy(transform.gameObject);
                }
                
            }
           
            //Destroy(transform.gameObject);
        }
    }
}
