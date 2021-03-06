﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerMove : MonoBehaviour {

    public static bool dead = false;
    private Rigidbody rb;
    public float speed = 50;
    public float tSpeed = 60;
    public float rotSpeed;
    public GameObject laser;
    public GameObject shot;
    public GameObject shot2;
    private Vector3 offset = new Vector3(0.5f, 0, 5f);
    private Vector3 offset2 = new Vector3(-0.5f, 0, 5f);
    public bool shoot = true;
    public int health = 5;

    public GameObject explosion;

    public GameObject deathCam;

    public GameObject healthGui;
    public Text heatlhText;

    public AudioClip[] audioClip;
    AudioSource audio;
    // Use this for initialization
    void Start () {
        dead = false;
        rb = GetComponent<Rigidbody>();
        rotSpeed = 80;

    }
	
	// Update is called once per frame
	void Update () {

        heatlhText.text = "Health: " + health;
        //health
        if (health <= 0)
        {
            //game over

            //spawn camera
            //Instantiate(deathCam, transform.position, transform.rotation);
            dead = true;
            GetComponentInChildren<Renderer>().enabled = false;
            //GetComponent<Renderer>().enabled = false;

            GetComponent<Rigidbody>().Sleep();


            //Destroy(gameObject);

        }

        if (Input.GetAxis("Horizontal") < 0)
        {
            rb.AddRelativeForce(Vector3.left * speed);
            //transform.Rotate(Vector3.down * Time.deltaTime * rotSpeed, Space.Self);
        }
        if (Input.GetAxis("Horizontal") > 0)
        {
            rb.AddRelativeForce(Vector3.right * speed);
            //transform.Rotate(Vector3.up * Time.deltaTime * rotSpeed, Space.Self);
        }
        if (Input.GetAxis("Vertical") < 0)
        {
            rb.AddRelativeForce(Vector3.back * tSpeed);
        }
        if (Input.GetAxis("Vertical") > 0)
        {
            rb.AddRelativeForce(Vector3.forward * tSpeed);
        }
        /*
        if (Input.GetKey("y"))
        {
            // rb.AddTorque(Vector3(0,speed,0));
            transform.Rotate(Vector3.right * Time.deltaTime * rotSpeed,Space.Self);
        }
       */
        if (Input.GetAxis("RightStickY") < -0.5)
        {
            // rb.AddTorque(Vector3(0,speed,0));
           // transform.Rotate(Vector3.right * Time.deltaTime * rotSpeed, Space.Self);
        }
        if (Input.GetAxis("RightStickY") > 0.5)
        {
            // rb.AddTorque(Vector3(0,speed,0));
           // transform.Rotate(Vector3.left * Time.deltaTime * rotSpeed, Space.Self);
        }

        if (Input.GetAxis("RightStickX") < -0.5)
        {
            //rb.AddTorque(Vector3(0,speed,0));
            //transform.Rotate(Vector3.forward * Time.deltaTime * rotSpeed, Space.Self);
        }
        if (Input.GetAxis("RightStickX") > 0.5)
        {
            //rb.AddTorque(Vector3(0,speed,0));
            //transform.Rotate(Vector3.back * Time.deltaTime * rotSpeed, Space.Self);
        }


        Vector3 world = transform.rotation * offset;
        Vector3 spawn = transform.position + world;

        Vector3 world2 = transform.rotation * offset2;
        Vector3 spawn2 = transform.position + world2;


        if (Input.GetButtonDown("Fire1"))
        {
            // rb.AddTorque(Vector3(0,speed,0));

           shot = Instantiate(laser, spawn, Quaternion.Euler(0, 0, 90));
           shot2 = Instantiate(laser, spawn2, Quaternion.Euler(0, 0, 90));

            shot.GetComponent<Rigidbody>().velocity = transform.GetComponent<Rigidbody>().velocity;
             shot.GetComponent<Rigidbody>().rotation = transform.GetComponent<Rigidbody>().rotation;

            shot2.GetComponent<Rigidbody>().velocity = transform.GetComponent<Rigidbody>().velocity;
            shot2.GetComponent<Rigidbody>().rotation = transform.GetComponent<Rigidbody>().rotation;
            // shot.GetComponent<Rigidbody>().rotation = Quaternion.Euler(0, 0, 90);
        }
        if (Input.GetAxis("RightTrigger") < 0)
        {
            if (shoot)
            {
                GetComponent<AudioSource>().PlayOneShot(audioClip[0], 0.5f);
                shot = Instantiate(laser, spawn, Quaternion.Euler(0, 0, 90));
                shot2 = Instantiate(laser, spawn2, Quaternion.Euler(0, 0, 90));

                shot.GetComponent<Rigidbody>().velocity = transform.GetComponent<Rigidbody>().velocity;
                shot.GetComponent<Rigidbody>().rotation = transform.GetComponent<Rigidbody>().rotation;

                shot2.GetComponent<Rigidbody>().velocity = transform.GetComponent<Rigidbody>().velocity;
                shot2.GetComponent<Rigidbody>().rotation = transform.GetComponent<Rigidbody>().rotation;
                StartCoroutine(Wait());
            }
        }
            //pitch
            float turn = Input.GetAxis("RightStickX");
        rb.AddTorque(transform.up * 70f * turn);


        float turn2 = Input.GetAxis("RightStickY");
        rb.AddTorque(transform.right * 50f * turn2);
    }

    IEnumerator Wait()
    {
        shoot = false;
        yield return new WaitForSeconds(0.2f);
        shoot = true;

    }

    void OnCollisionEnter(Collision collision)
    {
        GetComponent<AudioSource>().PlayOneShot(audioClip[1], 1.0f);
        health = health - 1;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Projectiles")
        {
            GetComponent<AudioSource>().PlayOneShot(audioClip[1], 1.0f);
            health = health - 1;
        }
    }
}
