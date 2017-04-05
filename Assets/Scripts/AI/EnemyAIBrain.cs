using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/* 
 * Prevent duplicate targets
 * Restrict acess to fields
 */

public class EnemyAIBrain : MonoBehaviour 
{
    public static EnemyAIBrain SELF = null;

    public string PROJECTILE_TAG = "Untagged";
    [Tooltip("Resistance to Switching Targets")][SerializeField] private float TARGET_AFFINITY = .3f;
    [Tooltip("AI Targetable Objects")]          [SerializeField] private List<Rigidbody> Targets;

    private void Awake()
    {
        if (SELF == null) { SELF = this; }
        else { throw new System.Exception("Duplicate AI Brains"); }
    }
    public void addTarget(Rigidbody rb)
    {
        if(rb == null) { throw new System.Exception("Adding Null Target"); }

        Targets.Add(rb);
    }
    private void LateUpdate()
    {
        foreach(Rigidbody rb in Targets)
        {
            if(rb == null || !rb.gameObject.activeInHierarchy) { Targets.Remove(rb); }
        }
    }
    public SortedList<float, Rigidbody> GetPriorityTargets(EnemyAIAgent ea)
    {
        if (ea == null) { throw new System.Exception("Priority for Null"); }

        SortedList<float, Rigidbody> targetPriority = new SortedList<float, Rigidbody>(Targets.Count);
        foreach (Rigidbody rb in Targets)
        {
            float score = EvaluatePriority(ea,rb);
            if (score != Mathf.Infinity)
            {
                targetPriority.Add(score, rb);
            }
        }
        return targetPriority;
    }
    private float EvaluatePriority(EnemyAIAgent ea, Rigidbody rb)//How Good of a Target rb is for this turret, Lower = Better
    {
        //Estimate Reaction Time for Target
        Vector3 D = ea.transform.position - rb.position; float Dmag = D.magnitude;
        float Vn = ea.PROJECTILE_SPEED + Vector3.Dot(D, rb.velocity) / Dmag; //Estimate Net Vel from Turret
        if (Vn <= 0) { return Mathf.Infinity; } //At Current Velocity, Can't Hit (Makes Cautious Estimate, Only gives False Positives)
        float EstReactTime = Dmag / Vn;
        float affinity = (rb == ea.CurrentTarget ? 0 : TARGET_AFFINITY); //Accounts for Loyalty to Current Target
        return EstReactTime + affinity; //Add factors for more refined Priority controll
    }
}


/*@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@*/
/*@@@@@@@@@@@@@@@@@@@@@@@@@@@@ Custom Editor Below @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@*/
/*@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@*/
[CustomEditor(typeof(EnemyAIBrain))]
public class EnemyAIBrainEditor : Editor
{
    private EnemyAIBrain instance;

    private SerializedProperty TARGET_AFFINITY, Targets;
    private void OnEnable()
    {
        instance = target as EnemyAIBrain;
        TARGET_AFFINITY = serializedObject.FindProperty("TARGET_AFFINITY");
        Targets = serializedObject.FindProperty("Targets");
    }
    public override void OnInspectorGUI()
    {
        instance.PROJECTILE_TAG = EditorGUILayout.TagField("Projectiles Tag", instance.PROJECTILE_TAG);
        EditorGUILayout.PropertyField(TARGET_AFFINITY);
        EditorGUILayout.PropertyField(Targets,true);
        serializedObject.ApplyModifiedProperties();
    }
}
