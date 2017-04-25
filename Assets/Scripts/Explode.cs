using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explode : MonoBehaviour {
    public AudioClip[] audioClip;
    AudioSource audio;
    // Use this for initialization
    void Start () {
        GetComponent<AudioSource>().PlayOneShot(audioClip[0], 1.0f);
    }
	
	// Update is called once per frame
	void Update () {
        Destroy(gameObject, 5f);
    }
}
