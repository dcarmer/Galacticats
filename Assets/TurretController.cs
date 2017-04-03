using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* TO-DO
 *      Add Player Dependent Priority
 */
/* Room for Improvement:
 *     Projectile Pooling
 *     AI account for rotation speed
 *     Remove Duplicate Target List
 *     Complie Out Debug
 *     Update vs FixedUpdate
 *     Optimize Physics Math
 *     Coordinate With other Turrets
 *     Improve Pruning
 *     Improve Priority(proximity to MAX_THETA)
 *     Add Turret Health
 *     Turret Physics Effects Projectile/AI
 *     Raycast to Target
 *     Account for Target Size/ Projectile AOE
 *     Add Animations/Textures/Sounds
 *     Make Manualy Controllable(FPS)
 *     Add Noise to Aim
 *     When no viable targets, Optimize Aim for most likely to become viable
 *     When no targets, Random Aim 
 */
[HelpURL("https://docs.unity3d.com/ScriptReference/HelpURLAttribute.html")]
[RequireComponent(typeof(Collider))]
public class TurretController : MonoBehaviour 
{
    [Header("Constraint Configuration:")]
        [Tooltip("Max Distance of Projectile (meters)")]          public float MAX_RANGE = 200;
        [Tooltip("Max Rotation from Azimuth (Deg)")][Range(0,180)]public float MAX_THETA = 90;
        [Tooltip("Rotation Speed (Deg/sec)")]                     public float ROT_SPEED = 90;
        [Tooltip("Fire Rate (sec)")]                              public float FIRE_RATE = 1;
        [Tooltip("Speed of Projectile (meters/sec)")]             public float PROJECTILE_SPEED = 10;
        [Tooltip("Disabled Duration (sec)")]                      public float DISABLE_TIME = 10;
    [Header("AI Configuration: ")]
        [Tooltip("Resistance to Switching Targets")]              public float TARGET_AFFINITY = .3f;
        [Tooltip("Fire Constantly vs. Only when in Crosshairs")]  public bool  FIRE_CONSTANTLY = false;
    [Header("Linked Resource Components:")]
        [Tooltip("The Root Mounting Object")]       public Transform TURRET;
        [Tooltip("The Pivoting Mechanism Object")]  public Transform BARBETTE;
        [Tooltip("Fired Projectile Prefab")]        public Rigidbody PROJECTILE;
        [Tooltip("List of Targetable RigidBodies")] public List<Rigidbody> TARGETS = new List<Rigidbody>();

    private const float BARREL_OFFSET = .6f; //Offset Spawn For Projectiles
    private float PROJECTILE_SPEED2; //Cached PROJECTILE_SPEED^2
    private float MAX_TIME; //Cached MAX_RANGE/PROJECTILE_SPEED


    private bool disabled = false;
    private bool inCooldown = false;
    private bool TargetInCrosshairs = false; //If Pointed at Target and should fire
    private float LastFire; //Time of most recent fire
    private float LastDisable; //Time of most recent disable
    private Rigidbody Target; //Currently Tracking Target
    private List<Pair<Rigidbody, float>> priorityList = new List<Pair<Rigidbody, float>>();

    private IEnumerator afterTime(float t, System.Action action)
    {
        yield return new WaitForSeconds(t);
        action();
    }

    void Start () 
    {
        PROJECTILE_SPEED2 = PROJECTILE_SPEED * PROJECTILE_SPEED;
        MAX_TIME = MAX_RANGE / PROJECTILE_SPEED;
        foreach(Rigidbody rb in TARGETS)
        {
            priorityList.Add(new Pair<Rigidbody, float>(rb,Mathf.Infinity));
        }
    }
    private void FixedUpdate()
    {
        Debug.DrawRay(BARBETTE.position, TURRET.forward, Color.white);
        if (!disabled)
        {
            TargetInCrosshairs = false;
            Prioritize();
            foreach (Pair<Rigidbody, float> p in priorityList)
            {
                if (p.Key.gameObject.activeInHierarchy && p.Value != Mathf.Infinity && AimTurret(p.Key))
                {
                    break;
                }
            }
            FireTurret();
        }
    }
    private void Prioritize() //Updates Target Priorities and Orders list accordingly
    {
        foreach (Pair<Rigidbody,float> p in priorityList)
        {
            p.Value = EvaluatePriority(p.Key);
        }
        priorityList.Sort((x,y)=> x.Value.CompareTo(y.Value));
    }
    private float EvaluatePriority(Rigidbody rb)//How Good of a Target rb is for this turret, Lower = Better
    {
        //Determine Reaction Time Estimate between Target = D/(S+cosV) = D/(S+(D*V/D)) = D^2/(DS+D*V)
        Vector3 D = BARBETTE.position - rb.position;
        float Vn = (PROJECTILE_SPEED + Vector3.Dot(D.normalized, rb.velocity)); //Estimated Net Velocity relative to Turret Center
        if (Vn <= 0) { return Mathf.Infinity; } //At Current Velocity, Can't Hit (Makes Cautious Estimate, Only gives False Positives)
        float EstReactTime = D.magnitude / Vn;

        float affinity = (rb == Target ? 0 : TARGET_AFFINITY); //Accounts for Loyalty to Current Target

        return EstReactTime + affinity; //Add factors for more refined Priority controll
    }
    
    private void FireTurret()
    {
        Debug.Log(inCooldown );
        if (!inCooldown && (TargetInCrosshairs || FIRE_CONSTANTLY)) //Not in Cooldown and aiming correctly
        {
            Rigidbody Shot =  Instantiate(PROJECTILE, BARBETTE.position + BARREL_OFFSET * BARBETTE.forward, BARBETTE.rotation); //Create
            Shot.velocity = Shot.transform.forward*PROJECTILE_SPEED; //Set Speed
            Destroy(Shot.gameObject,MAX_TIME); //Delayed Destruction
            inCooldown = true;
            StartCoroutine(afterTime(FIRE_RATE, () => inCooldown = false));
        }
    }
    private bool AimTurret(Rigidbody rb) //Returns True if Succesfully Targeting
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
        Vector3 D = BARBETTE.position - rb.position;
        float DV = Vector3.Dot(D, V);
        float A = V.sqrMagnitude - PROJECTILE_SPEED2;
        float T = float.NaN;
        if(A == 0) //Linear Equation
        {
            if(DV > 0) //If Time Exists
            {
                T = D.sqrMagnitude / (2 * DV);
            }
        }
        else //Quadratic Equation
        {
            float Descriminant = DV*DV - A * D.sqrMagnitude;
            if(Descriminant == 0) //Redundant Root
            {
                T = DV / A;
            }
            else if(Descriminant > 0)//Two Real Roots
            {
                Descriminant = Mathf.Sqrt(Descriminant);
                float root = (Descriminant - DV) / A;
                if(root >= 0) //If Time Exists
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
        if(T <= MAX_TIME) //Is in Range
        {
            Vector3 ProjectedPosition = V * T - D;
            if(Vector3.Angle(TURRET.forward,ProjectedPosition) <= MAX_THETA) //In Fireing Volume
            {
                Debug.DrawRay(BARBETTE.position, ProjectedPosition, Color.green); //Lead Ray
                Debug.DrawLine(BARBETTE.position, rb.position, Color.blue); //Direct Ray
                                                                                
                Quaternion GoalRotation = Quaternion.LookRotation(ProjectedPosition, TURRET.forward);
                BARBETTE.rotation = Quaternion.RotateTowards(BARBETTE.rotation, GoalRotation, Time.deltaTime * ROT_SPEED); //Rotate Toward Target
                TargetInCrosshairs = BARBETTE.rotation == GoalRotation;//In Fireing Position
                Target = rb;
                return true;
            }
        }
        return false;
    }
    void OnTriggerEnter(Collider other)
    {
        Debug.Log(name +" Hit, Disabled");
        disabled = true;
        StartCoroutine(afterTime(DISABLE_TIME, () => disabled = false));
    }
    public class Pair<TKey, TValue> //Simple Class To Hold Related Pairs of Values
    {
        public TKey Key; public TValue Value;
        public Pair(TKey k, TValue v) { Key = k; Value = v; }
    }
}
