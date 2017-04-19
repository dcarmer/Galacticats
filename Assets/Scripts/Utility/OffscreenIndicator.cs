using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Renderer))]
public class OffscreenIndicator : MonoBehaviour
{
    [SerializeField]private Image OffScreenAlertIconImage_PREFAB;
    [SerializeField]private Canvas IndicatorCanvas;
    
    
    private Image OffScreenAlertIcon;
    private Renderer rend;
    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }
    private void OnEnable()
    {
        if (OffScreenAlertIcon == null)
        {
            OffScreenAlertIcon = Instantiate(OffScreenAlertIconImage_PREFAB, IndicatorCanvas.transform);
            OffScreenAlertIcon.gameObject.SetActive(false);
            OffScreenAlertIcon.name = OffScreenAlertIconImage_PREFAB.name + "("+name+")";
            OffScreenAlertIcon.gameObject.hideFlags = HideFlags.NotEditable;
        }
        Camera.onPreCull += IndicateOffscreen;
    }
    private void OnDisable()
    {
        Camera.onPreCull -= IndicateOffscreen;
        if(OffScreenAlertIcon != null)
        {
            Destroy(OffScreenAlertIcon.gameObject);
        }

    }
    private void IndicateOffscreen(Camera cam)
    {
        //if (cam.name == "SceneCamera" || cam.name == "PreRenderCamera") { return; }

        if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(cam), rend.bounds)) //Object Visible on screen
        {
            OffScreenAlertIcon.gameObject.SetActive(false);
            return; 
        }


        OffScreenAlertIcon.gameObject.SetActive(true);
        IndicatorCanvas.worldCamera = cam; //Overlays Canvas on camera

        Vector3 screenDir = Vector3.ProjectOnPlane(transform.position - cam.transform.position, cam.transform.forward);
        OffScreenAlertIcon.rectTransform.rotation = Quaternion.LookRotation(cam.transform.forward, screenDir); //Rotates image to point at target
        screenDir = Quaternion.Inverse(IndicatorCanvas.transform.rotation) * screenDir; //Adjusts to be relative to x,y plane for GUI
        Vector2 scaled = Vector2.zero;
        float ctan = screenDir.x / screenDir.y;
        float width = cam.pixelWidth, height = cam.pixelHeight;
        if (Mathf.Abs(ctan) > cam.aspect) //Sections 1 or 3
        {
            if (screenDir.x > 0) //Section 1
            {
                scaled.x = width;      scaled.y = .5f * (height + width / ctan);
            }
            else //Section 3
            {
                scaled.x = 0;       scaled.y = .5f * (height - width / ctan);
            }
        }
        else //Sections 2 or 4
        {
            if (screenDir.y > 0) //Section 2
            {
                scaled.x = .5f * (width + height * ctan);      scaled.y = height;
            }
            else //Section 4
            {
                scaled.x = .5f * (width - height * ctan);      scaled.y = 0;
            }

        }
        OffScreenAlertIcon.rectTransform.anchoredPosition = scaled; //Sets image position on boarder nearest to target
    }
}