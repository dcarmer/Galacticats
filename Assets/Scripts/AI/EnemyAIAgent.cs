using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * Set up protected sets in editor
 * Restrict acess to fields
 * 
 * Account for target drag, self rotate speed
 * Add More Sounds(got hit, disabled, destroyed, invincible)
 * Improve health bar when disabled/recovering and invincible
 * Raycast Target check
 * Add Textures/Animations for fireing, impacts ect...
 * 
 * Add Noise Option for Aim
 * Add Cameras for viewing
 */

[HelpURL("https://github.com/dcarmer")][DisallowMultipleComponent][SelectionBase]
public abstract class EnemyAIAgent : MonoBehaviour
{
    public Rigidbody CurrentTarget { get; protected set; } //Currently Tracking Target
    protected bool TargetInCrosshairs = false; //If Pointed at Target and should fire

    [Tooltip("Fired Projectile Prefab")]         [SerializeField]protected Rigidbody ProjectilePrefab;
    [Tooltip("The Projectile Spawning Location")][SerializeField]protected Transform PROJ_SPAWN;
    [Tooltip("Speed of Projectile (Meters/Sec)")]public float PROJECTILE_SPEED = 10; // @ Should be protected set
    [Tooltip("Fire Rate (Sec)")]                 public float FIRE_DELAY = 1; // @ Should be protected set
    protected bool inCooldown = false;
    [Tooltip("Max Distance of Projectile (Meters)")][Delayed]public float MAX_RANGE = 200;  // @ Should be protected set
    protected float MAX_TIME { get { return MAX_RANGE / PROJECTILE_SPEED; } }

    [Tooltip("Rotation Speed (Deg/sec)")][SerializeField]protected float ROT_SPEED = 90;

    [Tooltip("Fire-ing Sound")]                  [SerializeField]protected AudioClip FireSound;
    [Tooltip("AudioSource for Generated Sounds")][SerializeField]protected AudioSource SoundSource;

    public enum HitResponse { Invincible, Disable, Destroy }
    private HitResponse _HitResponse;
    public HitResponse HIT_RESPONSE
    {
        get { return _HitResponse; }
        set
        {
            if (HIT_RESPONSE != value)
            {
                HealthBar.gameObject.SetActive(value != HitResponse.Invincible);
            }
            _HitResponse = value;
        }
    }
    [Tooltip("HealthBar")][SerializeField]protected FillBarGUI HealthBar;
    [Tooltip("Hits before HitResponse Effect")]public int HPCap = 3; // @ Should be protected set
    public int HPLeft = 3;
    [Tooltip("Disabled Duration (Sec)")]public float DISABLE_TIME = 5; // @ Should be protected set
    protected float TimeUntillEnable = 0;
    public bool Disabled = true;

    public delegate void Damaged();
    public event Damaged OnDamaged;

    /*-----------------------------------Editor Support Start-----------------------------------*/
    protected virtual void OnValidate()
    {
        //Make sure Linked Resources are present
        if (PROJ_SPAWN == null || ProjectilePrefab == null || SoundSource == null || HealthBar == null)
        {
            throw new Exception("Missing Required Resource in EnemyAIAgent");
        }

        //Make Sure Distances,Times,and Speeds are non-negative
        MAX_RANGE = Mathf.Max(0, MAX_RANGE);
        ROT_SPEED = Mathf.Max(0, ROT_SPEED);
        FIRE_DELAY = Mathf.Max(0, FIRE_DELAY);
        PROJECTILE_SPEED = Mathf.Max(0, PROJECTILE_SPEED);
        DISABLE_TIME = Mathf.Max(0, DISABLE_TIME);
        HealthBar.value = (float)HPLeft / HPCap;
    }
    /*------------------------------------Editor Support End------------------------------------*/
    protected virtual void Update()
    {
        if (Disabled)
        {
            TimeUntillEnable -= Time.deltaTime;
            if (TimeUntillEnable <= 0) //Re-enable
            {
                HPLeft = HPCap;
                HealthBar.value = (float)HPLeft / HPCap;
                Disabled = false;
                TimeUntillEnable = 0;
                Debug.Log("ReEnabled");
            }
        }
    }
    protected virtual void FixedUpdate()
    {
        if (!Disabled)
        {
            TargetInCrosshairs = false;
            foreach (KeyValuePair<float, Rigidbody> kvp in EnemyAIBrain.SELF.GetPriorityTargets(this))
            {
                if (LeadTarget(kvp.Value))
                {
                    CurrentTarget = kvp.Value;
                    break;
                }
            }
            Fire();
        }
    }
    protected void ResetProjectile(Rigidbody proj)
    {
        proj.transform.position = PROJ_SPAWN.position;
        proj.transform.rotation = PROJ_SPAWN.rotation;
        proj.velocity = proj.transform.forward * PROJECTILE_SPEED;
        StartCoroutine(afterTime(MAX_TIME, () => { proj.gameObject.SetActive(false); })); //Deactivate After time
    }
    protected void Fire()
    {
        if (!inCooldown && TargetInCrosshairs) //Not in Cooldown and aiming correctly
        {
            ObjectPool.Spawn(ProjectilePrefab, ResetProjectile);
            SoundSource.PlayOneShot(FireSound);
            inCooldown = true;
            StartCoroutine(afterTime(FIRE_DELAY, () => inCooldown = false));
        }
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.tag == EnemyAIBrain.SELF.PROJECTILE_TAG)
        {
            Debug.Log(name + " Hit");
            if (HIT_RESPONSE != HitResponse.Invincible)
            {
                HPLeft = Mathf.Max(0, HPLeft - 1);
                HealthBar.value = (float)HPLeft / HPCap;
                if (HPLeft == 0)
                {
                    switch (HIT_RESPONSE)
                    {
                        case HitResponse.Destroy:
                            Debug.Log(name + " Destroyed");
                            Destroy(gameObject);
                            break;
                        case HitResponse.Disable:
                            Debug.Log(name + " Disabled");
                            Disabled = true;
                            TimeUntillEnable = DISABLE_TIME;
                            break;
                    }
                }
                OnDamaged();
            }
        }
    }
    protected float TargetLeadTime(Rigidbody rb)
    {
        /*  Given:
             *      T = Time
             *      A = Turret Position, S = Projectile Speed
             *      B = Target Position, V = Target Velocity
             *  Define:
             *      D = (A-B), B(T) = VT-D
             *  Thus:
             *      ||B(T)|| = ST = sqrt[B(T)*B(T)] = sqrt[(VT-D)*(VT-D)] = sqrt[(|V|T)^2-2(D*V)T+|D|^2]
             *      0 = (|V|T)^2-2(D*V)T+|D|^2-(ST)^2 = (|V|^2-S^2)T^2-2(D*V)T+|D|^2
             *  Let:
             *      a = |V|^2-S^2,   b = -2(D*V),   c = |D|^2
             *  If a == 0 Then it's a linear equation:
             *      0 = -2(D*V)T+|D|^2
             *      If D*V > 0 Then Time Exists:
             *          T = |D|^2/(2(D*V))
             *  Else its a quadratic equation:
             *      discriminant = b^2-4ac
             *      If descriminant == 0 Then Redundant Root:
             *          T = -b/2a = (D*V)/(|V|^2-S^2)
             *      Else if descriminant > 0 Then Two Real Roots:
             *          T = (-b +/- sqrt(descriminant)) / 2a
             *          Use minimum > 0
             */
        Vector3 V = rb.velocity;
        Vector3 D = transform.position - rb.position;
        float DV = Vector3.Dot(D, V);
        float A = V.sqrMagnitude - PROJECTILE_SPEED * PROJECTILE_SPEED;
        float T = float.NaN;
        if (A == 0) //Linear Equation
        {
            if (DV > 0) //If Time Exists
            {
                T = D.sqrMagnitude / (2 * DV);
            }
        }
        else //Quadratic Equation
        {
            float Descriminant = DV * DV - A * D.sqrMagnitude;
            if (Descriminant == 0) //Redundant Root
            {
                T = DV / A;
            }
            else if (Descriminant > 0)//Two Real Roots
            {
                Descriminant = Mathf.Sqrt(Descriminant);
                float root = (Descriminant - DV) / A;
                if (root >= 0) //If Time Exists
                {
                    T = root;
                }
                root = (-Descriminant - DV) / A;
                if (root >= 0) //If Other Time Exists
                {
                    T = Mathf.Min(T, root); //Keep Smallest
                }
            }
        }
        //At this point, Time has been found or is NaN
        return T;
    }
    protected IEnumerator afterTime(float t, Action action)//Utility Function
    {
        yield return new WaitForSeconds(t); action();
    }
    /*-----------------------------------Abstract Methods Start---------------------------------*/
    protected abstract bool LeadTarget(Rigidbody rb);
    /*-----------------------------------Abstract Methods End-----------------------------------*/
}
