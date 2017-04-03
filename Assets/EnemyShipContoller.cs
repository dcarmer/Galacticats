using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * Basically same thing as the turret ai, with movement controlls.
 * 
 * Includes Dodgeing, and 
 * 
 */
[HelpURL("https://docs.unity3d.com/ScriptReference/HelpURLAttribute.html")]
[RequireComponent(typeof(Rigidbody))]
public class EnemyShipContoller : MonoBehaviour
{
    //Ease of use
    private const string PROJ_TAG = "Projectiles";
    private const string ENEMYSHIP_TAG = "Enemy Ship";
    private const float BOUNDS_RADIUS = 3;

[Header("Constraint Configuration:")]
    [Tooltip("Max Distance of Projectile (meters)")] public float MAX_RANGE = 200;
    [Tooltip("Rotation Speed (Deg/sec)")]            public float ROT_SPEED = 90;
    [Tooltip("Fire Rate (sec)")]                     public float FIRE_RATE = 1;
    [Tooltip("Speed of Projectile (meters/sec)")]    public float PROJECTILE_SPEED = 10;
    [Tooltip("Max Thrust Output in Fwd/Bkwd Direction")] public float MAX_LNG_THRUST = 5;
    [Tooltip("Max Thrust Output in Lateral Direction")] public float MAX_LAT_THRUST = 5;
    [Tooltip("Max Thrust Output in Angular Direction")] public float MAX_TRQ_THRUST = 5;
[Header("AI Configuration: ")]
    [Tooltip("Will the AI attempt to halt velocity on lack of input")] public bool stabilize = true;
    [Tooltip("Resistance to Switching Targets")]             public float TARGET_AFFINITY = .3f;
    [Tooltip("Prefered Range of target (meters)")]           public float PREF_RANGE = 50;
    [Tooltip("Additional Lookahead Time for dodging (sec)")] public float FORESIGHT = 1;
    [Tooltip("Fire Constantly vs. Only when in Crosshairs")] public bool FIRE_CONSTANTLY = false;
[Header("Linked Resource Components:")]
    [Tooltip("Fired Projectile Prefab")]        public Rigidbody PROJECTILE;
    [Tooltip("Projectile Spawn Location")]      public Transform PROJ_SPAWN;
    [Tooltip("Projectile Spawn Location")] public GameObject STARBOARD_ENGINE;
    [Tooltip("Projectile Spawn Location")] public GameObject PORT_ENGINE;
    [Tooltip("List of Targetable RigidBodies")] public List<Rigidbody> TARGETS = new List<Rigidbody>();


    //THIS
    private Rigidbody SHIP;
    private float PROJECTILE_SPEED2; //Cached PROJECTILE_SPEED^2
    private float MAX_TIME; //Cached MAX_RANGE/PROJECTILE_SPEED
    private float REQ_DODGE_TIME; //Time needed in Worst case to complete full dodge
    private float LOOKAHEAD_TIME; //Cached Req dodge time + foresight
    Vector3 acceleration = Vector3.zero;
    Vector3 angularAcceleration = Vector3.zero;

    private bool dodgeing = false; //Currently Dodging
    private Vector3 dodgeDir; //Dodge direction
    private bool inCooldown = false; //Currently not able to fire
    private bool TargetInCrosshairs = false; //If Pointed at Target and should fire
    private Rigidbody Target; //Currently Tracking Target
    private List<Pair<Rigidbody, float>> priorityList = new List<Pair<Rigidbody, float>>();
    
    private IEnumerator afterTime(float t, System.Action action)
    {
        yield return new WaitForSeconds(t);
        action();
    }
    
    void Start () 
    {
        SHIP = GetComponent<Rigidbody>();
        PROJECTILE_SPEED2 = PROJECTILE_SPEED * PROJECTILE_SPEED;
        MAX_TIME = MAX_RANGE / PROJECTILE_SPEED;
        //dx = vo*t + .5*at^2 = 2r = 0t + .5*Min(a)t^2
        //t = sqrt(4r/Min(a)) Min Time needed at max thrust at rest to be in distinct space
        REQ_DODGE_TIME = Mathf.Sqrt(4*BOUNDS_RADIUS/(SHIP.mass*Mathf.Min(MAX_LNG_THRUST, MAX_LAT_THRUST)));
        LOOKAHEAD_TIME = REQ_DODGE_TIME + FORESIGHT;
        foreach (Rigidbody rb in TARGETS)
        {
            priorityList.Add(new Pair<Rigidbody, float>(rb, Mathf.Infinity));
        }
        StartCoroutine(applyEngineEffects());
    }
    private IEnumerator applyEngineEffects()
    {
        Transform S_T = STARBOARD_ENGINE.transform;
        Transform P_T = PORT_ENGINE.transform;

        ParticleSystem S_PS = STARBOARD_ENGINE.GetComponent<ParticleSystem>();
        ParticleSystem P_PS = PORT_ENGINE.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule S_MM = S_PS.main; float S_BASE_SPEED = S_MM.startSpeedMultiplier;
        ParticleSystem.MainModule P_MM = P_PS.main; float P_BASE_SPEED = P_MM.startSpeedMultiplier;
        ParticleSystem.EmissionModule S_EM = S_PS.emission; float S_BASE_RATE = S_EM.rateOverTimeMultiplier;
        ParticleSystem.EmissionModule P_EM = P_PS.emission; float P_BASE_RATE = P_EM.rateOverTimeMultiplier;

        Light S_LIGHT = STARBOARD_ENGINE.GetComponent<Light>(); float S_BASE_BRIGHT = S_LIGHT.intensity;
        Light P_LIGHT = PORT_ENGINE.GetComponent<Light>(); float P_BASE_BRIGHT = P_LIGHT.intensity;

        Behaviour S_HALO = STARBOARD_ENGINE.GetComponent("Halo") as Behaviour;
        Behaviour P_HALO = PORT_ENGINE.GetComponent("Halo") as Behaviour;
        while (true)
        {
            float accMag = Vector3.ClampMagnitude(acceleration, 1).magnitude;
            if(acceleration != Vector3.zero)
            {
                S_T.rotation = P_T.rotation = Quaternion.LookRotation(acceleration);
            }
            S_MM.startSpeedMultiplier = accMag * S_BASE_SPEED;
            P_MM.startSpeedMultiplier = accMag * P_BASE_SPEED;
            S_EM.rateOverTimeMultiplier = accMag * S_BASE_RATE;
            P_EM.rateOverTimeMultiplier = accMag * P_BASE_RATE;
            S_LIGHT.intensity = accMag * S_BASE_BRIGHT;
            P_LIGHT.intensity = accMag * P_BASE_BRIGHT;

            S_HALO.enabled = P_HALO.enabled = accMag > 0.001;
            
            yield return null;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Enemy Ship hit by: "+collision.gameObject.name);
    }
    private Rigidbody inDanger()
    {
        GameObject[] projs = GameObject.FindGameObjectsWithTag(PROJ_TAG);
        foreach (GameObject g in projs)
        {
            Rigidbody rb = g.GetComponent<Rigidbody>();
            RaycastHit hitInfo;
            if (Physics.Raycast(rb.position, rb.velocity * LOOKAHEAD_TIME, out hitInfo) && hitInfo.rigidbody == SHIP)
            {
                return rb;
            }
        }
        return null;
    }
    private float beenDodging = 0;
    private void FixedUpdate()
    {
        TargetInCrosshairs = false;
        Prioritize();
        foreach (Pair<Rigidbody, float> p in priorityList)
        {
            if (p.Key.gameObject.activeInHierarchy && p.Value != Mathf.Infinity && Aim(p.Key))
            {
                break;
            }
        }
        Move();
        Fire();
        Debug.DrawRay(SHIP.position, SHIP.velocity, Color.cyan);
    }
    private void Move()
    {
        Vector3 netForce = Vector3.zero;
        Vector3 netTorque = Vector3.zero;
        if(!dodgeing)
        {
            Rigidbody threat = inDanger();
            if (threat != null)
            {
                //Get point on unit circle, rotate to lie on plane ortho to projectile
                dodgeDir = (Quaternion.FromToRotation(Vector3.forward, -threat.velocity) * Random.insideUnitCircle).normalized;
                dodgeing = true;
                StartCoroutine(afterTime(REQ_DODGE_TIME, () => dodgeing = false));
            }
            else if(Target != null && TargetInCrosshairs && Vector3.Distance(SHIP.position, Target.position) > PREF_RANGE)
            {
                netForce = transform.forward * MAX_LNG_THRUST;
            }
            
        }

        if(dodgeing)
        {
            float rad = Vector3.Angle(transform.forward, dodgeDir)*Mathf.Deg2Rad;
            Vector2 thrustCoe = new Vector2(Mathf.Cos(rad) / MAX_LNG_THRUST, Mathf.Sin(rad) / MAX_LAT_THRUST);
            thrustCoe /= Mathf.Max(Mathf.Abs(thrustCoe.x), thrustCoe.y);

            netForce += transform.forward * thrustCoe.x * MAX_LNG_THRUST; //Lng thrust
            netForce += Vector3.ProjectOnPlane(dodgeDir, transform.forward).normalized * thrustCoe.y * MAX_LAT_THRUST;//lat thrust
        }

        if(stabilize)
        {
            if(Vector3.Dot(netForce,transform.forward) == 0)//No thrust in fwd dir
            {
                Vector3 fwdVelDir = Vector3.Project(SHIP.velocity, transform.forward);
                float fwdVel = Vector3.Dot(SHIP.velocity, transform.forward);
                if (fwdVelDir.sqrMagnitude > 0.001)
                {
                    netForce += -fwdVelDir * MAX_LNG_THRUST; //Apply thrust in opposite dir
                }
            }
            if (Vector3.ProjectOnPlane(netForce, transform.forward) == Vector3.zero)//No thrust in lat dir
            {
                Vector3 latVelDir = Vector3.ProjectOnPlane(SHIP.velocity, transform.forward);
                if (latVelDir.sqrMagnitude > 0.001)
                {
                    netForce += -latVelDir.normalized * MAX_LAT_THRUST; //Apply thrust in opposite dir
                }
            }
            if(netTorque.sqrMagnitude > 0.001) //no trq thrust
            {
                if(SHIP.angularVelocity != Vector3.zero)
                {
                    netTorque += -SHIP.angularVelocity.normalized * MAX_TRQ_THRUST;
                }
            }
        }
        acceleration = netForce;
        angularAcceleration = netTorque;
        SHIP.AddForce(netForce);
        SHIP.AddTorque(netTorque);
    }
    private void Fire()
    {
        if (!inCooldown && (TargetInCrosshairs || FIRE_CONSTANTLY)) //Not in Cooldown and aiming correctly
        {
            Rigidbody Shot = Instantiate(PROJECTILE, PROJ_SPAWN.position, PROJ_SPAWN.rotation); //Create
            Shot.velocity = Shot.transform.forward * PROJECTILE_SPEED; //Set Speed
            Destroy(Shot.gameObject, MAX_TIME); //Delayed Destruction
            inCooldown = true;
            StartCoroutine(afterTime(FIRE_RATE,()=>inCooldown = false));
        }
    }
    private bool Aim(Rigidbody rb) //Returns True if Succesfully Targeting
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
        Vector3 D = SHIP.position - rb.position;
        float DV = Vector3.Dot(D, V);
        float A = V.sqrMagnitude - PROJECTILE_SPEED2;
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
        if (T <= MAX_TIME) //Is in Range
        {
            Vector3 ProjectedPosition = V * T - D;

            Debug.DrawRay(SHIP.position, ProjectedPosition, Color.green); //Lead Ray
            Debug.DrawLine(SHIP.position, rb.position, Color.blue); //Direct Ray

            Quaternion GoalRotation = Quaternion.LookRotation(ProjectedPosition, transform.up);
            SHIP.MoveRotation(Quaternion.RotateTowards(SHIP.rotation, GoalRotation, Time.fixedDeltaTime * ROT_SPEED)); //Rotate Toward Target
            TargetInCrosshairs = SHIP.rotation == GoalRotation;//In Fireing Position
            Target = rb;
            return true;
        }
        return false;
    }
    private void Prioritize() //Updates Target Priorities and Orders list accordingly
    {
        foreach (Pair<Rigidbody, float> p in priorityList)
        {
            p.Value = EvaluatePriority(p.Key);
        }
        priorityList.Sort((x, y) => x.Value.CompareTo(y.Value));
    }
    private float EvaluatePriority(Rigidbody rb)//How Good of a Target rb is, Lower = Better
    {
        //Determine Reaction Time Estimate between Target = D/(S+cosV) = D/(S+(D*V/D)) = D^2/(DS+D*V)
        Vector3 D = SHIP.position - rb.position;
        float Vn = (PROJECTILE_SPEED + Vector3.Dot(D.normalized, rb.velocity)); //Estimated Net Velocity relative to Turret Center
        if (Vn <= 0) { return Mathf.Infinity; } //At Current Velocity, Can't Hit (Makes Cautious Estimate, Only gives False Positives)
        float EstReactTime = D.magnitude / Vn;

        float affinity = (rb == Target ? 0 : TARGET_AFFINITY); //Accounts for Loyalty to Current Target

        return EstReactTime + affinity; //Add factors for more refined Priority controll
    }
    public class Pair<TKey, TValue> //Simple Class To Hold Related Pairs of Values
    {
        public TKey Key; public TValue Value;
        public Pair(TKey k, TValue v) { Key = k; Value = v; }
    }
}
