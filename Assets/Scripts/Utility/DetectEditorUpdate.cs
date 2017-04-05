using UnityEngine;

/*
 * Destroy Self on play, still be there when exit tho
 */

[ExecuteInEditMode][DisallowMultipleComponent]
public class DetectEditorUpdate : MonoBehaviour 
{
    private void Start()
    {
        hideFlags = HideFlags.NotEditable;
        enabled = !Application.isPlaying;
        int dependent = 0;
        foreach (MonoBehaviour c in gameObject.GetComponents(typeof(MonoBehaviour)))
        {
            if (c.GetType().GetMethod("OnEditorUpdate") != null)
            {
                dependent++;
            }
        }
        if (dependent == 0)
        {
            Debug.LogWarning("Prevented adding DetectEditorUpdate to deaf GameObject '"+gameObject.name+"'");
            DestroyImmediate(this);
        }
    }
    private void Update()
    {
        int dependent = 0;
        foreach (MonoBehaviour c in gameObject.GetComponents(typeof(MonoBehaviour)))
        {
            if (c.GetType().GetMethod("OnEditorUpdate") != null)
            {
                dependent++;
                c.Invoke("OnEditorUpdate", 0);
            }
        }
        if(dependent == 0)
        {
            DestroyImmediate(this);
            Debug.LogWarning("Removed DetectEditorUpdate from newly deaf GameObject '" + gameObject.name+"'");
        }
    }
}
