using UnityEngine;

public class SpaceStation : MonoBehaviour 
{
    private string ProjectileTag = "Projectiles";
    [SerializeField]private Renderer Sheild;
    [SerializeField]private GameObject Explosion;

    [SerializeField]private AnimationCurve Red, Green, Blue;

    [SerializeField]private int HPCap;
    [SerializeField][Range(2,8)]private int HPLeft;

    public AudioClip[] audioClip;
    AudioSource audio;

    private void Start()
    {
        HPLeft = HPCap;
        UpdateSheild();
    }
    private void UpdateSheild()
    {
        if (HPLeft <= 1)//Sheilds Down
        {
            Sheild.gameObject.SetActive(false);
        }
        else
        {
            float sheildPercent = 1-(float)(HPLeft-1) / (HPCap - 1);
            Sheild.material.color = new Color(Red.Evaluate(sheildPercent), Green.Evaluate(sheildPercent), Blue.Evaluate(sheildPercent), .6f);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == ProjectileTag)
        {
            GetComponent<AudioSource>().PlayOneShot(audioClip[0], 0.5f);
            HPLeft--;
            UpdateSheild();
            if(HPLeft <= 0)//Explode
            {
                Destroy(Instantiate(Explosion, transform.position, Quaternion.identity),2);//Spawn/Destroy Explosion Effect
                transform.parent.gameObject.SetActive(false);
            }
        }
    }
    private void OnValidate()
    {
        //UpdateSheild();
    }
}
