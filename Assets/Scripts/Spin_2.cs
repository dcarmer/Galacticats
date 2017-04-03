using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin_2 : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //transform.Rotate(Vector3.right * Time.deltaTime);
        transform.Rotate(Vector3.down * Time.deltaTime * 50, Space.World);

    }
}
