using UnityEngine;

[ExecuteInEditMode]
public class Billboard : MonoBehaviour 
{
    private void OnEnable()
    {
        Camera.onPreCull += FaceCamera;
    }
    private void OnDisable()
    {
        Camera.onPreCull -= FaceCamera;
    }
    private void FaceCamera(Camera cam)
    {
        transform.LookAt(transform.position + cam.transform.forward, cam.transform.up);
    }
}
