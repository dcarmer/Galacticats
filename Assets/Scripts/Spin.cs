using UnityEngine;

public class Spin : MonoBehaviour 
{
    [SerializeField]private Vector3 RotationDirection = Vector3.up;

    private void Update () 
    {
        transform.Rotate(RotationDirection * Time.deltaTime, Space.World);
    }
}
