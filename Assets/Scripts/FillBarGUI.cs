using UnityEngine;

public class FillBarGUI : MonoBehaviour 
{
    [SerializeField]private UnityEngine.UI.Image Background;
    [SerializeField]private UnityEngine.UI.Image Fill;
    public float value 
    { 
        set 
        {
            Vector2 sz = Background.rectTransform.sizeDelta;
            sz.x *= Mathf.Clamp01(value);
            Fill.rectTransform.sizeDelta = sz;
        } 
    }


}
