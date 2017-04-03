using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionCloud : MonoBehaviour {

	// Use this for initialization
	void Start () {
        
            StartCoroutine(Example());
      
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    IEnumerator Example()
    {
        
        yield return new WaitForSeconds(2);
        Destroy(gameObject);

    }
}
