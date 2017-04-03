using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour {
    private Rigidbody rb;
    public float speed = 5000;
    // Use this for initialization
    void Start() {
        speed = 5000;
        rb = GetComponent<Rigidbody>();
        rb.AddRelativeForce(Vector3.forward * speed);


    }

    // Update is called once per frame
    void Update() {

    }

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

}
