using System.Collections;
using UnityEngine;


/// <summary>
/// This class handles the AI perception system for detecting and tracking targets.
/// It includes visual perception with a line of sight check, range check, and search functionalities.
/// </summary>
public class AIController_Perception : MonoBehaviour
{
    // Values:
    public float PerceptionUpdateInterval = 0.05f; // coroutine wait interval at which perception updates
    public float PerceptionOnSightUpdateInterval = 0.5f; // coroutine wait interval when the target is in sight
    public float MaxVisualPerceptionDistance = 20f; // Maximum distance at which the AI can perceive a target visually
    public float ForwardViewAngle = 60; // The forward view angle in degrees 

    // Components:
    private AIController Controller;

    // Coroutines:
    Coroutine Coroutine_LineofSight;
    Coroutine Coroutine_FolowTargetSight;
    Coroutine Coroutine_SearchForTarget; 
    private WaitForSeconds Wait; // Wait time for perception updates
    private WaitForSeconds WaitOnSight; // Wait time when target is in sight

    // Raycast hit layers:
    public LayerMask targetLayerMask; // Layer mask for the target objects
    public LayerMask nontargetLayerMask; // Layer mask for non-target objects (walls, obstacles, etc.)

    // Target:
    public Transform TargetOnSight { get; set; } // The target that is currently in sight
    public Vector3 TargetlastKnownPosition { get; set; } // The last known position of the target

    private void Awake()
    {
        Controller = GetComponent<AIController>();
        Wait = new WaitForSeconds(PerceptionUpdateInterval);
        WaitOnSight = new WaitForSeconds(PerceptionOnSightUpdateInterval);
    }

    private void OnDisable()
    {
        if (Coroutine_LineofSight != null)
        {
            StopCoroutine(Coroutine_LineofSight);
            Coroutine_LineofSight = null;
        }
        if (Coroutine_FolowTargetSight != null)
        {
            StopCoroutine(Coroutine_FolowTargetSight);
            Coroutine_FolowTargetSight = null;
        }
        if (Coroutine_SearchForTarget != null)
        {
            StopCoroutine(Coroutine_SearchForTarget);
            Coroutine_SearchForTarget = null;
        }
    }

    #region Start functions

    /// <summary>
    /// Starts looking for a target. It will either patrol or focus on the target depending on the state.
    /// </summary>
    public virtual void StartLookingForTarget(bool IsPatrolling)
    {
        if (IsPatrolling)
        {
            // Start the line of sight check for patrolling AI
            if (Coroutine_LineofSight == null)
                Coroutine_LineofSight = StartCoroutine(CheckLineOfSight());
        }
        else
        {
            // Increase the visual range and field of view for focused search
            MaxVisualPerceptionDistance *= 2;
            ForwardViewAngle = 80;
            if (Coroutine_LineofSight == null)
                Coroutine_LineofSight = StartCoroutine(CheckLineOfSight());
        }
    }

    /// <summary>
    /// Starts following the target if it's within sight.
    /// </summary>
    public virtual void StartFollowingTargetSight()
    {
        if (Coroutine_FolowTargetSight == null)
            Coroutine_FolowTargetSight = StartCoroutine(FollowTargetSight(TargetOnSight));
    }

    /// <summary>
    /// Starts searching for the target if it's lost or not visible.
    /// </summary>
    public virtual void StartSearchForTarget()
    {
        if (Coroutine_SearchForTarget == null)
            Coroutine_SearchForTarget = StartCoroutine(SearchForTarget());
    }

    #endregion

    #region coroutine functions

    /// <summary>
    /// Checks line of sight to potential targets in the area and updates the target on sight.
    /// </summary>
    private IEnumerator CheckLineOfSight()
    {
        while (enabled)
        {
            for (int i = 0; i < Controller.Targetables.Count; i++)
            {
                // Distance check to see if the target is within visual range
                if (Vector3.Distance(transform.position, Controller.Targetables[i].transform.position) <= MaxVisualPerceptionDistance)
                {
                    var targetDir = CalculateTargetViewAngle(Controller.Targetables[i].transform);
                    Vector3 origin = transform.position + Vector3.up; // Starting point adjusted for height

                    // View angle check
                    if (targetDir != Vector3.zero)
                    {
                        // Raycast to check for obstacles between AI and target
                        if (Physics.Raycast(origin, targetDir, out RaycastHit TargetHit, MaxVisualPerceptionDistance, targetLayerMask))
                        {
                            // Check if an obstacle is closer than the target
                            if (Physics.Raycast(origin, targetDir, out RaycastHit obstacleHit, MaxVisualPerceptionDistance, nontargetLayerMask))
                            {

                                // If obstacle is closer than the target, it's blocking the view
                                if (obstacleHit.distance > TargetHit.distance)
                                {
                                    int index = i;
                                    TargetOnSight = Controller.Targetables[index].transform;
                                    Coroutine_LineofSight = null;
                                    yield break;
                                }
                                else
                                {
                                    TargetOnSight = null;
                                }
                            }
                        }
                        else
                        {
                            TargetOnSight = null;
                        }
                    }
                }
            }
            yield return Wait;
        }
        yield break;
    }

    // Variable to track if the target's last seen position should be updated
    bool lastSeenCheck = true;

    /// <summary>
    /// Follows the target's position and updates the last seen position.
    /// </summary>
    private IEnumerator FollowTargetSight(Transform Target)
    {
        if (Target == null)
        {
            Coroutine_FolowTargetSight = null;
            yield break;
        }

        yield return null;
        bool checkAgain = false;
        var distance = MaxVisualPerceptionDistance * 2;
        while (enabled)
        {
            // Save the last known position of the target every other iteration
            if (lastSeenCheck)
            {
                TargetlastKnownPosition = Target.position;
                lastSeenCheck = false;
            }
            else lastSeenCheck = true;


            // Raycast to check for obstacles between AI and target
            Vector3 origin = transform.position + Vector3.up; // Starting point adjusted for height
            var targetDir = (Target.transform.position - transform.position);

            // Check if an obstacle is blocking the target
            if (Physics.Raycast(origin, targetDir, out RaycastHit obstacleHit, distance, nontargetLayerMask))
            {
                var TargetDistance = (Target.transform.position - transform.position);

                // If the obstacle is closer than the target, stop following
                if (obstacleHit.distance < TargetDistance.magnitude)
                {
                    if (checkAgain)
                    {
                        // Recheck from the last seen position
                        if (Physics.Raycast(origin, targetDir, out obstacleHit, distance, nontargetLayerMask))
                        {
                            TargetDistance = (Target.transform.position - transform.position);

                            // If the obstacle is still closer, stop following the target
                            if (obstacleHit.distance < TargetDistance.magnitude)
                            {
                                lastSeenCheck = true;
                                TargetOnSight = null; // target lost
                                Coroutine_FolowTargetSight = null;
                                yield break;
                            }
                        }
                    }
                    else
                    {
                        checkAgain = true;
                    }
                }
            }
            yield return WaitOnSight;
        }
        yield break;
    }

    /// <summary>
    /// Searches for the target by checking all possible targets within range and line of sight.
    /// </summary>
    private IEnumerator SearchForTarget()
    {
        for (int i = 0; i < Controller.Targetables.Count; i++)
        {
            var distance = MaxVisualPerceptionDistance * 3;
            while (enabled)
            {
                // Raycast to check for obstacles between AI and target
                Vector3 origin = transform.position + Vector3.up; // Starting point adjusted for height
                var targetDir = (Controller.Targetables[i].transform.position - transform.position);

                // Check if an obstacle is blocking the target's sight
                RaycastHit obstacleHit;
                if (Physics.Raycast(origin, targetDir, out obstacleHit, distance, nontargetLayerMask))
                {
                    var TargetDistance = (Controller.Targetables[i].transform.position - transform.position);

                    // If the obstacle is further than the target, the AI can see the target
                    if (obstacleHit.distance > TargetDistance.magnitude)
                    {
                        int index = i;
                        TargetOnSight = Controller.Targetables[index].transform;
                        Coroutine_SearchForTarget = null;
                        yield break;
                    }
                }
                yield return WaitOnSight;
            }
        }
        yield break;
    }

    #endregion

    #region Helper functions

    /// <summary>
    /// Checks if the target is within the attack range based on minimum and maximum range distances.
    /// </summary>
    public virtual bool IsTargetInAttackRange()
    {
        // mesafe hesaplanýr ve max min range ile karþýlaþtýrýlýr
        var distance = Vector3.Distance(TargetOnSight.position, transform.position);
        if (distance < Controller.Movement.RangeMinDistance || distance > Controller.Movement.RangeMaxDistance)
            return false;
        else return true;
    }

    /// <summary>
    /// Calculates the angle between the AI's forward direction and the target's direction to check if the target is within view angle.
    /// </summary>
    private Vector3 CalculateTargetViewAngle(Transform target)
    {
        if (target == null)
            return Vector3.zero;

        // Calculate direction to the target
        Vector3 hedefYonu = (target.position - transform.position).normalized;

        // Check the dot product between AI's forward vector and the target direction to determine if the target is within the view angle
        float dot = Vector3.Dot(transform.forward, hedefYonu);

        // Calculate cosine of the view angle
        float gorusCos = Mathf.Cos(ForwardViewAngle * Mathf.Deg2Rad);

        // If the dot value is greater than the cosine of the view angle, the target is within the AI's field of view
        if (dot >= gorusCos)
            return hedefYonu;
        else return Vector3.zero;
    }

    #endregion
}
