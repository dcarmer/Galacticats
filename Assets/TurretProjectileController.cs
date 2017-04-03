using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretProjectileController : MonoBehaviour 
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit " + collision.gameObject.name);
        //Apply Damage to other.gameObject
        Destroy(gameObject); //Suicied
    }
}
