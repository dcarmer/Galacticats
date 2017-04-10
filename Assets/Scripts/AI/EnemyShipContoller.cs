using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues; //For AnimBool
#endif

/*
 * Check thru move and in danger code
 * Make inheritable Editor
 * Add Raycasting for targeting
 * Fix Multi-Object Editing
 * Error with HIT_RESPONSE Edit+Apply in inspector(doesnt reflect accurate value)
 */
[RequireComponent(typeof(Rigidbody))]
public class EnemyShipContoller : EnemyAIAgent
{
    private Rigidbody ThisShip;

    [Tooltip("Max Thrust Output in Fwd/Bkwd Direction")][SerializeField]private float MaxLngThrust = 5;
    [Tooltip("Max Thrust Output in Lateral Direction")] [SerializeField]private float MaxLatThrust = 5;
    [Tooltip("Max Thrust Output in Angular Direction")] [SerializeField]private float MaxRotThrust = 5;
    [Tooltip("Prevent Motion Sickness")]                    [SerializeField]private bool Stabilize = true;
    [Tooltip("Prefered Range of target (meters)")]          [SerializeField]private float DesiredRange = 50; 
    [Tooltip("Additional Lookahead Time for dodging (sec)")][SerializeField]private float Foresight = 1;
    [Tooltip("Starboard Engine")]   [SerializeField]private EngineController LeftEngine;
    [Tooltip("Port Engine")]        [SerializeField]private EngineController RightEngine;
    [Tooltip("Shield Collider")]    [SerializeField]private Collider Sheild;

    private const float BOUNDS_RADIUS = 3;
    private float MaxReqDodgeTime { get { return Mathf.Sqrt(4 * BOUNDS_RADIUS * ThisShip.mass / Mathf.Min(MaxLngThrust, MaxLatThrust)); } }
    private float LookaheadTime { get { return MaxReqDodgeTime + Foresight; } }

    private Vector3 acceleration = Vector3.zero;
    private Vector3 angularAcceleration = Vector3.zero;

    private bool dodgeing = false; //Currently Dodging
    private Vector3 dodgeDir; //Dodge direction

    /*-----------------------------------Editor Support Start-----------------------------------*/
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (LeftEngine == null || RightEngine == null || Sheild == null)
        {
            throw new System.Exception("Missing Required Resource");
        }
        DesiredRange = Mathf.Clamp(DesiredRange, 0, MAX_RANGE);
        Foresight = Mathf.Max(0, Foresight);
        MaxLatThrust = Mathf.Max(0, MaxLatThrust);
        MaxLngThrust = Mathf.Max(0, MaxLngThrust);
        MaxRotThrust = Mathf.Max(0, MaxRotThrust);
    }
#endif
    /*------------------------------------Editor Support End------------------------------------*/
    protected void Start() 
    {
        ThisShip = GetComponent<Rigidbody>();
    }
    protected void OnEnable()
    {
        OnDamaged += ManageSheild;
    }
    protected void OnDisable()
    {
        OnDamaged -= ManageSheild;
    }
    private void ManageSheild()
    {
        if (HPLeft == 1)
        {
            Sheild.gameObject.SetActive(false);
        }
    }
    protected override void Update()
    {
        base.Update();
        LeftEngine.MatchForce(acceleration);
        RightEngine.MatchForce(acceleration);
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if(!Disabled)
        {
            Move();
        }
    }
    protected override bool LeadTarget(Rigidbody rb) //Returns True if Succesfully Targeting
    {
        float T = TargetLeadTime(rb);
        if (T <= MAX_TIME) //Is in Range
        {
            Vector3 ProjectedPosition = rb.velocity * T + rb.position - transform.position;

            Debug.DrawRay(ThisShip.position, ProjectedPosition, Color.green); //Lead Ray
            Debug.DrawLine(ThisShip.position, rb.position, Color.blue); //Direct Ray

            Quaternion GoalRotation = Quaternion.LookRotation(ProjectedPosition, transform.up);
            ThisShip.MoveRotation(Quaternion.RotateTowards(ThisShip.rotation, GoalRotation, Time.fixedDeltaTime * ROT_SPEED)); //Rotate Toward Target
            TargetInCrosshairs = ThisShip.rotation == GoalRotation;//In Fireing Position
            return true;
        }
        return false;
    }



    private void Move()
    {
        Vector3 netForce = Vector3.zero;
        Vector3 netTorque = Vector3.zero;
        if (!dodgeing)
        {
            Rigidbody threat = inDanger();
            if (threat != null)
            {
                //Get point on unit circle, rotate to lie on plane ortho to projectile
                dodgeDir = (Quaternion.FromToRotation(Vector3.forward, -threat.velocity) * UnityEngine.Random.insideUnitCircle).normalized;
                dodgeing = true;
                StartCoroutine(afterTime(MaxReqDodgeTime, () => dodgeing = false));
            }
            else if (CurrentTarget != null && TargetInCrosshairs && Vector3.Distance(ThisShip.position, CurrentTarget.position) > DesiredRange)
            {
                netForce = transform.forward * MaxLngThrust;
            }

        }

        if (dodgeing)
        {
            float rad = Vector3.Angle(transform.forward, dodgeDir) * Mathf.Deg2Rad;
            Vector2 thrustCoe = new Vector2(Mathf.Cos(rad) / MaxLngThrust, Mathf.Sin(rad) / MaxLatThrust);
            thrustCoe /= Mathf.Max(Mathf.Abs(thrustCoe.x), thrustCoe.y);

            netForce += transform.forward * thrustCoe.x * MaxLngThrust; //Lng thrust
            netForce += Vector3.ProjectOnPlane(dodgeDir, transform.forward).normalized * thrustCoe.y * MaxLatThrust;//lat thrust
        }

        if (Stabilize)
        {
            if (Vector3.Dot(netForce, transform.forward) == 0)//No thrust in fwd dir
            {
                Vector3 fwdVelDir = Vector3.Project(ThisShip.velocity, transform.forward);
                if (fwdVelDir.sqrMagnitude > 0.001)
                {
                    netForce += -fwdVelDir * MaxLngThrust; //Apply thrust in opposite dir
                }
            }
            if (Vector3.ProjectOnPlane(netForce, transform.forward) == Vector3.zero)//No thrust in lat dir
            {
                Vector3 latVelDir = Vector3.ProjectOnPlane(ThisShip.velocity, transform.forward);
                if (latVelDir.sqrMagnitude > 0.001)
                {
                    netForce += -latVelDir.normalized * MaxLatThrust; //Apply thrust in opposite dir
                }
            }
            if (netTorque.sqrMagnitude > 0.001) //no trq thrust
            {
                if (ThisShip.angularVelocity != Vector3.zero)
                {
                    netTorque += -ThisShip.angularVelocity.normalized * MaxRotThrust;
                }
            }
        }
        acceleration = netForce;
        angularAcceleration = netTorque;
        ThisShip.AddForce(netForce);
        ThisShip.AddTorque(netTorque);
        Debug.DrawRay(ThisShip.position, ThisShip.velocity, Color.cyan);
    }
    private Rigidbody inDanger()
    {
        GameObject[] projs = GameObject.FindGameObjectsWithTag(EnemyAIBrain.SELF.PROJECTILE_TAG);
        foreach (GameObject g in projs)
        {
            Rigidbody rb = g.GetComponent<Rigidbody>();
            RaycastHit hitInfo;
            if (Physics.Raycast(rb.position, rb.velocity * LookaheadTime, out hitInfo) && hitInfo.rigidbody == ThisShip)
            {
                return rb;
            }
        }
        return null;
    }
}
#if UNITY_EDITOR
/*@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@*/
/*@@@@@@@@@@@@@@@@@@@@@@@@@@@@ Custom Editor Below @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@*/
/*@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@*/
[CustomEditor(typeof(EnemyShipContoller))] [CanEditMultipleObjects]
public class EnemyShipContollerEditor : Editor
{
    protected EnemyShipContoller instance;
    protected AnimBool HitOptFlg, HitOptDisableFlg;
    protected static bool showProjectile = true,
                        showMovement = true,
                        showAudio = true;

    protected SerializedProperty Sheild, LeftEngine, RightEngine,
                                ProjectilePrefab, PROJ_SPAWN, PROJECTILE_SPEED, FIRE_DELAY, MAX_RANGE,
                                MaxLngThrust, MaxLatThrust, MaxRotThrust, ROT_SPEED, Stabilize, DesiredRange, Foresight,
                                FireSound, SoundSource,
                                HealthBar, HPCap, HPLeft, DISABLE_TIME;

    protected EnumPropertyField HitResponse;

    private void OnEnable()
    {
        instance = target as EnemyShipContoller;

        (HitOptFlg = new AnimBool(instance.HIT_RESPONSE != EnemyAIAgent.HitResponse.Invincible)).valueChanged.AddListener(Repaint);
        (HitOptDisableFlg = new AnimBool(instance.HIT_RESPONSE == EnemyAIAgent.HitResponse.Disable)).valueChanged.AddListener(Repaint);

        Sheild = serializedObject.FindProperty("Sheild");
        LeftEngine = serializedObject.FindProperty("LeftEngine");
        RightEngine = serializedObject.FindProperty("RightEngine");

        ProjectilePrefab = serializedObject.FindProperty("ProjectilePrefab");
        PROJ_SPAWN = serializedObject.FindProperty("PROJ_SPAWN");
        PROJECTILE_SPEED = serializedObject.FindProperty("PROJECTILE_SPEED");
        FIRE_DELAY = serializedObject.FindProperty("FIRE_DELAY");
        MAX_RANGE = serializedObject.FindProperty("MAX_RANGE");

        MaxLngThrust = serializedObject.FindProperty("MaxLngThrust");
        MaxLatThrust = serializedObject.FindProperty("MaxLatThrust");
        MaxRotThrust = serializedObject.FindProperty("MaxRotThrust");
        ROT_SPEED = serializedObject.FindProperty("ROT_SPEED");
        Stabilize = serializedObject.FindProperty("Stabilize");
        DesiredRange = serializedObject.FindProperty("DesiredRange");
        Foresight = serializedObject.FindProperty("Foresight");

        FireSound = serializedObject.FindProperty("FireSound");
        SoundSource = serializedObject.FindProperty("SoundSource");

        HitResponse = new EnumPropertyField(instance, "HIT_RESPONSE");
        HealthBar = serializedObject.FindProperty("HealthBar");
        HPCap = serializedObject.FindProperty("HPCap");
        HPLeft = serializedObject.FindProperty("HPLeft");
        DISABLE_TIME = serializedObject.FindProperty("DISABLE_TIME");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(Sheild);
        EditorGUILayout.PropertyField(LeftEngine);
        EditorGUILayout.PropertyField(RightEngine);

        //Projectile Foldout Group
        if (showProjectile = EditorGUILayout.Foldout(showProjectile, "Projectile", true))
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(ProjectilePrefab);
            EditorGUILayout.PropertyField(PROJ_SPAWN);
            EditorGUILayout.PropertyField(PROJECTILE_SPEED);
            EditorGUILayout.PropertyField(FIRE_DELAY);
            EditorGUILayout.PropertyField(MAX_RANGE);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }
        //Movement Foldout Group
        if (showMovement = EditorGUILayout.Foldout(showMovement, "Movement", true))
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(MaxLngThrust);
            EditorGUILayout.PropertyField(MaxLatThrust);
            EditorGUILayout.PropertyField(MaxRotThrust);
            EditorGUILayout.PropertyField(ROT_SPEED);
            EditorGUILayout.PropertyField(Stabilize);
            DesiredRange.floatValue = EditorGUILayout.Slider("DesiredRange", DesiredRange.floatValue, 0, MAX_RANGE.floatValue);
            //EditorGUILayout.PropertyField(DesiredRange);
            EditorGUILayout.PropertyField(Foresight);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }
        //Audio Foldout Group
        if (showAudio = EditorGUILayout.Foldout(showAudio, "Audio", true))
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(FireSound);
            EditorGUILayout.PropertyField(SoundSource);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        //Hit Response Dynamic Fade Group(s)
        HitResponse.set(EditorGUILayout.EnumPopup(HitResponse.name, HitResponse.get));
        HitOptFlg.target = instance.HIT_RESPONSE != EnemyAIAgent.HitResponse.Invincible;
        using (EditorGUILayout.FadeGroupScope HitOptGrp = new EditorGUILayout.FadeGroupScope(HitOptFlg.faded))
        {
            if (HitOptGrp.visible)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(HealthBar);
                EditorGUILayout.PropertyField(HPCap);
                EditorGUILayout.PropertyField(HPLeft);
                HitOptDisableFlg.target = instance.HIT_RESPONSE == EnemyAIAgent.HitResponse.Disable;
                using (EditorGUILayout.FadeGroupScope HitOptDisableGrp = new EditorGUILayout.FadeGroupScope(HitOptDisableFlg.faded))
                {
                    if (HitOptDisableGrp.visible)
                    {
                        EditorGUILayout.PropertyField(DISABLE_TIME);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
    protected class EnumPropertyField
    {
        private object instance;
        private System.Reflection.MethodInfo getter, setter;

        public string name { get; protected set; }
        public System.Enum get { get { return (System.Enum)getter.Invoke(instance, null); } }

        public EnumPropertyField(object instance, string name)
        {
            this.instance = instance;
            this.name = name;
            System.Reflection.PropertyInfo info = instance.GetType().GetProperty(name);
            setter = info.GetSetMethod();
            getter = info.GetGetMethod();
        }
        public void set(object value)
        {
            setter.Invoke(instance, new object[] { value });
        }
    }
}
#endif
