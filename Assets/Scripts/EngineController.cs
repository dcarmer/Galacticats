using UnityEngine;

[RequireComponent(typeof(ParticleSystem),typeof(Light),typeof(Behaviour))]
public class EngineController : MonoBehaviour 
{
    private ParticleSystem.MainModule PSMain;
    private ParticleSystem.EmissionModule PSEmiss;
    private float BaseSpeed, BaseRate;

    private Light FlameLight;
    private float BaseBright;

    private Behaviour Halo;

    private void Start () 
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        PSMain = ps.main;
        PSEmiss = ps.emission;
        BaseSpeed = PSMain.startSpeedMultiplier;
        BaseRate = PSEmiss.rateOverTimeMultiplier;

        FlameLight = GetComponent<Light>();
        BaseBright = FlameLight.intensity;

        Halo = GetComponent("Halo") as Behaviour;
    }

    public void MatchForce(Vector3 force)
    {
        float thrust = Mathf.Min(force.magnitude, 1);
        if (thrust != 0)
        {
            transform.rotation = Quaternion.LookRotation(force);
        }
        PSMain.startSpeedMultiplier = thrust * BaseSpeed;
        PSEmiss.rateOverTimeMultiplier = thrust * BaseRate;
        FlameLight.intensity = thrust * BaseBright;
        Halo.enabled = thrust > 0.001;
    }
}
