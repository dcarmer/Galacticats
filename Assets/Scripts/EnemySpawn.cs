using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour {

    public GameObject enemy;
	// Use this for initialization
	void Start () {

        Instantiate(enemy, transform.position, transform.rotation);
    }
	
	// Update is called once per frame
	void Update () {


		
	}
}
