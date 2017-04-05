using UnityEngine;

public class Boundary : MonoBehaviour 
{
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            //Display Warning, Begin Timed Destruction
            Debug.Log("Player Exited");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            //Cancel any Timed Destruction, Remove Warning if Any
            Debug.Log("Player Re-entered");
        }
    }
}
