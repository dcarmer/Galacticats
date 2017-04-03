using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explode : MonoBehaviour {

    public GameObject explosion;

    // Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "laser")
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(transform.parent.gameObject);
      
        }
    }
}
