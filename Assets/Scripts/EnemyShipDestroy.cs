using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShipDestroy : MonoBehaviour {

    public GameObject Explosion;
    public int health = 3;
    // Use this for initialization
    void Start () {
        health = 3;   
	}

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            // Destroy(gameObject);
            Instantiate(Explosion, transform.position, transform.rotation);
            Destroy(gameObject);

        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Projectiles")
        {
            health = health - 1;
        }
    }
}
