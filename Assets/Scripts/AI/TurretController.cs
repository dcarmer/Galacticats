using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues; //For AnimBool


/*
 * Gizmo Arc Occlusion
 * Add Player Controlls
 * Change variable names
 */
[RequireComponent(typeof(Collider),typeof(DetectEditorUpdate))]
public class TurretController : EnemyAIAgent 
{
    [Tooltip("The Pivoting Mechanism Object")][SerializeField]private Transform BARBETTE;
    [Tooltip("Max Rotation from Zenith (Deg)")][Range(0,180)][SerializeField]private float MAX_THETA = 90;
    /*-----------------------------------Editor Support Start-----------------------------------*/
    protected override void OnValidate()
    {
        base.OnValidate();
        if (BARBETTE == null)
        {
            throw new System.Exception("Missing Required Resource in TurretController");
        }
        //Makes sure BARBETTE rotation is within valid range(MAX_THETA Valid via RANGE attribute)
        Vector3 rot = BARBETTE.localEulerAngles;
        rot.x = Mathf.Min((rot.x + 90) % 360, MAX_THETA) - 90;
        BARBETTE.localEulerAngles = rot;
    }
    private void OnDrawGizmos()
    {
        if (Selection.activeTransform != null && Selection.activeTransform.IsChildOf(transform))
        {
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position, transform.up);
            Handles.color = Gizmos.color = Color.red;
            Vector3 rf = transform.right + transform.forward;
            Vector3 lf = transform.right - transform.forward;
            if (MAX_THETA < 180)
            {
                Vector3 straight = transform.up * MAX_RANGE;
                Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(MAX_THETA, transform.right) * straight);
                Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(-MAX_THETA, transform.right) * straight);
                Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(MAX_THETA, transform.forward) * straight);
                Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(-MAX_THETA, transform.forward) * straight);
                Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(MAX_THETA, rf) * straight);
                Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(-MAX_THETA, rf) * straight);
                Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(MAX_THETA, lf) * straight);
                Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(-MAX_THETA, lf) * straight);
                Handles.DrawWireDisc(transform.position + transform.up * Mathf.Cos(MAX_THETA * Mathf.Deg2Rad) * MAX_RANGE, transform.up, Mathf.Sin(MAX_THETA * Mathf.Deg2Rad) * MAX_RANGE);
            }
            Handles.DrawWireDisc(transform.position + transform.up * Mathf.Cos(MAX_THETA * Mathf.Deg2Rad / 4) * MAX_RANGE, transform.up, Mathf.Sin(MAX_THETA * Mathf.Deg2Rad / 4) * MAX_RANGE);
            Handles.DrawWireDisc(transform.position + transform.up * Mathf.Cos(MAX_THETA * Mathf.Deg2Rad * 2 / 4) * MAX_RANGE, transform.up, Mathf.Sin(MAX_THETA * Mathf.Deg2Rad * 2 / 4) * MAX_RANGE);
            Handles.DrawWireDisc(transform.position + transform.up * Mathf.Cos(MAX_THETA * Mathf.Deg2Rad * 3 / 4) * MAX_RANGE, transform.up, Mathf.Sin(MAX_THETA * Mathf.Deg2Rad * 3 / 4) * MAX_RANGE);
            
            Handles.DrawWireArc(transform.position, transform.right, transform.up, MAX_THETA, MAX_RANGE);
            Handles.DrawWireArc(transform.position, transform.right, transform.up, -MAX_THETA, MAX_RANGE);
            Handles.DrawWireArc(transform.position, transform.forward, transform.up, MAX_THETA, MAX_RANGE);
            Handles.DrawWireArc(transform.position, transform.forward, transform.up, -MAX_THETA, MAX_RANGE);
            Handles.DrawWireArc(transform.position, rf, transform.up, MAX_THETA, MAX_RANGE);
            Handles.DrawWireArc(transform.position, rf, transform.up, -MAX_THETA, MAX_RANGE);
            Handles.DrawWireArc(transform.position, lf, transform.up, MAX_THETA, MAX_RANGE);
            Handles.DrawWireArc(transform.position, lf, transform.up, -MAX_THETA, MAX_RANGE);
        }
    }
    public void OnEditorUpdate()
    {
        if (BARBETTE.hasChanged)
        {
            OnValidate();
            BARBETTE.hasChanged = false;
        }
    }
    /*------------------------------------Editor Support End------------------------------------*/
    protected override bool LeadTarget(Rigidbody rb)
    {
        float T = TargetLeadTime(rb);
        if (T <= MAX_TIME) //Is in Range
        {
            Vector3 ProjectedPosition = rb.velocity * T + rb.position - transform.position;
            if (Vector3.Angle(transform.up, ProjectedPosition) <= MAX_THETA) //In Fireing Volume
            {
                Debug.DrawRay(BARBETTE.position, ProjectedPosition, Color.green); //Lead Ray
                Debug.DrawLine(BARBETTE.position, rb.position, Color.blue); //Direct Ray


                Quaternion GoalRotation = Quaternion.LookRotation(ProjectedPosition, transform.up);
                BARBETTE.rotation = Quaternion.RotateTowards(BARBETTE.rotation, GoalRotation, Time.deltaTime * ROT_SPEED); //Rotate Toward Target
                TargetInCrosshairs = (BARBETTE.rotation == GoalRotation);// && Physics.Raycast(BARBETTE.position, ProjectedPosition, ProjectedPosition.magnitude,Physics.IgnoreRaycastLayer,QueryTriggerInteraction.Ignore);//In Fireing Position
                return true;
            }
        }
        return false;
    }
}


/*@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@*/
/*@@@@@@@@@@@@@@@@@@@@@@@@@@@@ Custom Editor Below @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@*/
/*@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@*/
[CustomEditor(typeof(TurretController))] [CanEditMultipleObjects]
public class TurretControllerEditor : Editor
{
    protected TurretController instance;
    protected AnimBool HitOptFlg, HitOptDisableFlg;
    protected static bool showProjectile = true,
                        showMovement = true,
                        showAudio = true;

    protected SerializedProperty BARBETTE,
                                ProjectilePrefab, PROJ_SPAWN, PROJECTILE_SPEED, FIRE_DELAY, MAX_RANGE,
                                MAX_THETA, ROT_SPEED,
                                FireSound, SoundSource,
                                HealthBar, HPCap, HPLeft, DISABLE_TIME;

    protected EnumPropertyField HitResponse;

    private void OnEnable()
    {
        instance = target as TurretController;

        (HitOptFlg = new AnimBool(instance.HIT_RESPONSE != EnemyAIAgent.HitResponse.Invincible)).valueChanged.AddListener(Repaint);
        (HitOptDisableFlg = new AnimBool(instance.HIT_RESPONSE == EnemyAIAgent.HitResponse.Disable)).valueChanged.AddListener(Repaint);

        BARBETTE = serializedObject.FindProperty("BARBETTE");

        ProjectilePrefab = serializedObject.FindProperty("ProjectilePrefab");
        PROJ_SPAWN = serializedObject.FindProperty("PROJ_SPAWN");
        PROJECTILE_SPEED = serializedObject.FindProperty("PROJECTILE_SPEED");
        FIRE_DELAY = serializedObject.FindProperty("FIRE_DELAY");
        MAX_RANGE = serializedObject.FindProperty("MAX_RANGE");

        MAX_THETA = serializedObject.FindProperty("MAX_THETA");
        ROT_SPEED = serializedObject.FindProperty("ROT_SPEED");

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

        EditorGUILayout.PropertyField(BARBETTE);

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
            EditorGUILayout.PropertyField(MAX_THETA);
            EditorGUILayout.PropertyField(ROT_SPEED);
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