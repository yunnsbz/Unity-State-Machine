using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This is the main class to be used for AI State Machine logic.
/// It handles initialization, state updates, and AI behavior flow.
/// </summary>
public class AIController : MonoBehaviour
{
    // components:
    public NavMeshAgent Agent;
    public AIController_Movement Movement;
    public AIController_Perception Perception;
    public AIController_Attack Attacks;

    // List of all targetable entities within perception range
    public List<Entity> Targetables;

    // values:
    public float AIUpdateWaitInterval = 0.2f;

    // system components:
    private StateMachine stateMachine;
    private WaitForSeconds Wait;

    private void Awake()
    {
        NullCheck();

        Wait = new WaitForSeconds(AIUpdateWaitInterval);

        stateMachine = new StateMachine();
    }

    private IEnumerator AIUpdate()
    {
        while (enabled)
        {
            if (Agent.enabled)
            {
                stateMachine.Update();
                yield return Wait;
            }
            yield return null;
        }
        yield break;
    }

    // Enemy should not be active immediately — wait for other systems to initialize
    protected virtual IEnumerator ActivateBehaviour()
    {
        yield return null;
        yield return null;
        Agent.enabled = true;
        if (Agent.isOnNavMesh)
        {
            stateMachine.ChangeState(new AIState_Idle(this, stateMachine));
            StartCoroutine(AIUpdate());
        }
        yield break;
    }

    public virtual void OnDisable()
    {
        if (Agent != null && Agent.enabled)
        {
            Agent.enabled = false;
        }
        StopAllCoroutines();
    }
    public virtual void OnEnable()
    {
        if (Agent != null && Agent.enabled)
        {
            Agent.enabled = false;
        }
        StartCoroutine(ActivateBehaviour());
    }

    public virtual void NullCheck()
    {
        if (Agent == null) throw new System.Exception("AIController can not find the nav mesh agent");
        if (Attacks == null) throw new System.Exception("AIControllerr can not find any attack class");
        if (Perception == null) throw new System.Exception("AIController can not find any Perception class");
        if (Movement == null) throw new System.Exception("AIController can not find any Movement class");
    }


    private void OnDrawGizmos()
    {
        if (stateMachine != null)
        {
            if (stateMachine.currentState is AIState_TakeRange)
            {
                if (Perception.TargetOnSight != null)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(transform.position + Vector3.up, (Perception.TargetOnSight.transform.position - transform.position) + Vector3.up);
                }
            }

            //if (stateMachine.currentState is AIState_)
            //{
            //    Gizmos.color = Color.green;
            //    Gizmos.DrawWireSphere(transform.position + Vector3.up, 2f);
            //}
            Gizmos.color = Color.red;

            // Calculate the left and right directions for view angle
            Quaternion leftRotation = Quaternion.AngleAxis(-Perception.ForwardViewAngle / 2, Vector3.up);
            Quaternion rightRotation = Quaternion.AngleAxis(Perception.ForwardViewAngle / 2, Vector3.up);

            Vector3 leftRayDirection = leftRotation * transform.forward;
            Vector3 rightRayDirection = rightRotation * transform.forward;

            float distance = Perception.MaxVisualPerceptionDistance;

            // If the enemy is in "Take Range" state (if they see the target), increase the perception distance
            if (stateMachine.currentState is AIState_TakeRange) distance = Perception.MaxVisualPerceptionDistance * 2;

            // Draw view angle rays
            Gizmos.DrawRay(transform.position, leftRayDirection * distance);
            Gizmos.DrawRay(transform.position, rightRayDirection * distance);

            // Draw a visual arc for the AI's field of view
            DrawViewArc(transform.position, transform.forward, Perception.ForwardViewAngle, distance);
        }

    }

    // Helper function to draw an arc representing the AI's field of view
    private void DrawViewArc(Vector3 position, Vector3 forward, float angle, float distance)
    {
        int segments = 20; // Yayýn segment sayýsý
        float segmentAngle = angle / segments;

        Vector3 previousPoint = position + (Quaternion.AngleAxis(-angle / 2, Vector3.up) * forward) * distance;

        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -angle / 2 + segmentAngle * i;
            Vector3 currentPoint = position + (Quaternion.AngleAxis(currentAngle, Vector3.up) * forward) * distance;

            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }

}
