using UnityEngine;

public class TurretProjectileController : MonoBehaviour 
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit " + collision.gameObject.name);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.attachedRigidbody != null)
        {
            Debug.Log("Triggered " + other.attachedRigidbody.name);
        }
        else
        {
            Debug.Log("Triggered " + other.name);
        }
        
    }
    private void OnDestroy()
    {
        Debug.Log("Destoyed");
    }
}
