using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AIController_Movement handles all navigation-based movement behaviors for an AI-controlled enemy,
/// including patrolling, strafing, range adjustments, targeting, and investigating positions.
/// It supports both static patrol points and dynamic roaming, and uses Unity's NavMesh system for pathfinding.
/// </summary>
public class AIController_Movement : MonoBehaviour
{
    // values:
    [Header("taking range")]
    public float RangeMinDistance = 8f;
    public float RangeMaxDistance = 15f;
    private float RangeTakingSpeed = 2f;
    private float RandomRangeRadius = 4f; // Max radius enemy can wander when trying to reposition within range

    [Header("patroling")]
    public float RandomPatrolRadius = 9f; // Max radius for random patrol movement
    public float PatrolWaitDuration = 0.9f;
    public Transform[] PatrolPoints;

    private AIController Controller;

    // coroutines:
    private Coroutine C_Patrol;
    private Coroutine C_TakeRange;
    private Coroutine C_LookToTarget;
    private Coroutine C_Investigate;
    private Coroutine C_Strafe;
    private Coroutine C_Push;
    private WaitForSeconds PatrolWait;


    private void Awake()
    {
        Controller = GetComponent<AIController>();
        PatrolWait = new WaitForSeconds(PatrolWaitDuration);
    }

    private void OnDisable()
    {
        // Stop all active coroutines and reset them
        StopCoroutineSafe(ref C_Patrol);
        StopCoroutineSafe(ref C_TakeRange);
        StopCoroutineSafe(ref C_LookToTarget);
        StopCoroutineSafe(ref C_Investigate);
        StopCoroutineSafe(ref C_Strafe);
        StopCoroutineSafe(ref C_Push);
    }

    private void StopCoroutineSafe(ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    #region Patrolling

    /// <summary>
    /// Starts patrolling between predefined points. Returns true if patrolling can begin.
    /// </summary>
    public virtual bool StartPatrolling()
    {
        Controller.Agent.updateRotation = true;
        Controller.Agent.updatePosition = true;
        // önceden belirlenmiþ hareket noktasý varsa oraya geçiþ yapar
        if (PatrolPoints != null && PatrolPoints.Length >= 2)
        {
            if (C_Patrol == null)
            {
                C_Patrol = StartCoroutine(PatrolRoutine(PatrolPoints));
            }
            return true;
        }
        else return false;
    }

    /// <summary>
    /// Starts random patrolling around a defined radius.
    /// </summary>
    public virtual void StartRandomPatrol()
    {
        Controller.Agent.updateRotation = true;
        Controller.Agent.updatePosition = true;

        if (C_Patrol != null) StopCoroutine(C_Patrol);
        C_Patrol = StartCoroutine(RandomPatrolRoutine());
    }

    public virtual void StopPatrolling()
    {
        if (C_Patrol != null)
        {
            StopCoroutine(C_Patrol);
            Controller.Agent.isStopped = false;
            C_Patrol = null;
        }
    }

    #endregion

    #region Range Management

    /// <summary>
    /// Adjusts position to maintain a distance within defined min and max range from the target.
    /// </summary>
    public virtual void StartTakingRange(Transform target)
    {
        Controller.Agent.updateRotation = false;
        // mesafe hesaplanýr ve yaklaþmasý mý yoksa uzaklaþmasý mý gerektiðini hesaplar
        var distance = Vector3.Distance(target.position, transform.position);
        if (distance < RangeMinDistance)
        {
            // hedefin tersi yönde hareket et:
            var direction = transform.position - target.position;
            var GoToPosition = transform.position + direction.normalized * RangeTakingSpeed;
            var distanceToTake = Vector3.Distance(transform.position, GoToPosition);

            // eðer hedeften kaçamýyorsan rastgele bir pozisyon seçip oraya kaç sonra tekrar normal range alýrsýn
            if (Physics.Raycast(transform.position, direction, distanceToTake))
            {
                GoToPosition = GetRandomPointInDirection(direction, RandomRangeRadius);
                if (GoToPosition != Vector3.zero && C_TakeRange == null)
                {
                    C_TakeRange = StartCoroutine(GoToTarget(GoToPosition));
                }
            }
            else
            {
                // hedeften uzaklaþ
                if (C_TakeRange == null)
                    C_TakeRange = StartCoroutine(GoToTarget(GoToPosition));
            }
        }
        if (distance > RangeMaxDistance)
        {
            // hedefe doðru hareket et:
            var direction = target.position - transform.position;
            var GoToPosition = transform.position * RangeTakingSpeed;
            var distanceToTake = Vector3.Distance(transform.position, GoToPosition);

            // eðer hedeften kaçamýyorsan rastgele bir pozisyon seçip oraya kaç sonra tekrar normal range alýrsýn
            if (Physics.Raycast(transform.position, direction, distanceToTake))
            {
                GoToPosition = GetRandomPointInDirection(direction, RandomRangeRadius);
                if (GoToPosition != Vector3.zero && C_TakeRange == null)
                {
                    C_TakeRange = StartCoroutine(GoToTarget(GoToPosition));
                }
            }
            else
            {
                // hedeften uzaklaþ
                if (C_TakeRange == null)
                    C_TakeRange = StartCoroutine(GoToTarget(GoToPosition));
            }
        }
    }
    public virtual void StopTakingRange()
    {
        Controller.Agent.updateRotation = true;
        if (C_TakeRange != null)
        {
            StopCoroutine(C_TakeRange);
            C_TakeRange = null;
        }
    }

    #endregion

    #region Target Look

    /// <summary>
    /// Starts smoothly rotating toward a target (used during shooting or aiming).
    /// </summary>
    public virtual void StartLookingToTarget(Transform target)
    {
        if (C_LookToTarget == null)
        {
            C_LookToTarget = StartCoroutine(LookAtTarget(target));
        }
    }

    public virtual void StopLookingToTarget()
    {
        if (C_LookToTarget != null)
        {
            StopCoroutine(C_LookToTarget);
            C_LookToTarget = null;
        }
    }

    #endregion

    #region Investigation

    /// <summary>
    /// Moves to the last known position of the target (used when the target is lost).
    /// </summary>
    public virtual void StartInvestigateTarget(Vector3 TargetLastSeenPos)
    {
        // last seen'e doðru yürü
        if (C_Investigate == null)
        {
            C_Investigate = StartCoroutine(GoToTarget(TargetLastSeenPos));
        }
    }

    public virtual void StopInvestigateTarget()
    {
        if (C_Investigate != null)
        {
            StopCoroutine(C_Investigate);
            C_Investigate = null;
        }
    }

    #endregion

    #region Strafing

    /// <summary>
    /// Moves around the target while staying within range (used for tactical movement).
    /// </summary>
    public virtual void StartStrafing(Transform Target)
    {
        if (C_Strafe == null)
        {
            C_Strafe = StartCoroutine(WalkOnCircle(Target));
        }
    }

    public virtual void StopStrafing()
    {
        if (C_Strafe != null)
        {
            StopCoroutine(C_Strafe);
            C_Strafe = null;
        }
    }

    #endregion

    #region Push / Force Move

    private IEnumerator GoToTarget(Vector3 TargetPos)
    {
        Controller.Agent.isStopped = false;
        Controller.Agent.SetDestination(TargetPos);
        // Hedefe ulaþmayý bekle
        while (Controller.Agent.pathPending || Controller.Agent.remainingDistance > Controller.Agent.stoppingDistance + 0.1f)
        {
            yield return null;
        }
        C_TakeRange = null;
        yield break;
    }

    public void PushToTarget(Vector3 TargetPos)
    {
        if (C_Push == null)
        {
            C_Push = StartCoroutine(_PushToTarget(TargetPos));
        }
    }

    #endregion

    #region Core Movement Routines

    private IEnumerator _PushToTarget(Vector3 TargetPos)
    {
        Controller.Agent.isStopped = false;
        Controller.Agent.SetDestination(TargetPos);
        // Hedefe ulaþmayý bekle
        while (Controller.Agent.pathPending || Controller.Agent.remainingDistance > Controller.Agent.stoppingDistance + 0.1f)
        {
            yield return null;
        }
        C_Push = null;
        yield break;
    }

    private IEnumerator LookAtTarget(Transform target)
    {
        while (enabled)
        {
            yield return null;
            // Hedef yönüne doðru bir vektör hesapla
            Vector3 direction = (target.position - transform.position).normalized;

            // Dönüþün sadece yatay eksende (y ekseni) olmasý için:
            direction.y = 0;

            if (direction.magnitude > 0.05f) // Çok küçük yönler için iþlem yapma
            {
                // Ýstenen rotasyonu hesapla
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Mevcut rotasyonu hedef rotasyona doðru döndür
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Controller.Agent.angularSpeed * Time.deltaTime);
            }
        }
        yield break;
    }

    private IEnumerator PatrolRoutine(Transform[] _PatrolPoints)
    {
        if (PatrolPoints == null || PatrolPoints.Length == 0 || PatrolPoints[0] == null)
        {
            C_Patrol = null;
            yield break;
        }

        int iter = 0;
        while (enabled)
        {
            if (iter >= PatrolPoints.Length) iter = 0;
            // Rastgele bir hedef pozisyon seç
            Vector3 wanderTarget = _PatrolPoints[iter++].position;

            // NavMesh üzerinde geçerli bir pozisyon bulunursa oraya git
            if (wanderTarget != Vector3.zero)
            {
                Controller.Agent.isStopped = false;
                Controller.Agent.SetDestination(wanderTarget);
                //Debug.Log("patroling to: " + wanderTarget);
                // Hedefe ulaþmayý bekle
                while (Controller.Agent.pathPending || Controller.Agent.remainingDistance > Controller.Agent.stoppingDistance + 0.1f)
                {
                    yield return null;
                }
            }
            // Bir süre bekle
            yield return PatrolWait;
        }
    }

    IEnumerator RandomPatrolRoutine()
    {
        while (enabled)
        {
            // Rastgele bir hedef pozisyon seç
            Vector3 wanderTarget = GetRandomPoint();

            // NavMesh üzerinde geçerli bir pozisyon bulunursa oraya git
            if (wanderTarget != Vector3.zero)
            {
                Controller.Agent.isStopped = false;
                Controller.Agent.SetDestination(wanderTarget);
                //Debug.Log("New destination set");

                // Hedefe ulaþmayý bekle
                while (Controller.Agent.pathPending || Controller.Agent.remainingDistance > Controller.Agent.stoppingDistance + 0.1f)
                {
                    yield return null;
                }
                //Debug.Log("waited to reach target");
            }
            // Bir süre bekle
            //Debug.Log("wandering around");
            yield return PatrolWait;
        }
    }

    IEnumerator WalkOnCircle(Transform Target)
    {
        while (enabled)
        {
            Vector3 StrafeTarget = GetStrafePosition(Target);

            // NavMesh üzerinde geçerli bir pozisyon bulunursa oraya git
            if (StrafeTarget != Vector3.zero)
            {
                Controller.Agent.isStopped = false;
                Controller.Agent.SetDestination(StrafeTarget);
                // Hedefe ulaþmayý bekle
                while (Controller.Agent.pathPending || Controller.Agent.remainingDistance > Controller.Agent.stoppingDistance + 0.1f)
                {
                    yield return null;
                }
            }
            // Bir süre bekle
            yield return PatrolWait;
        }
    }

    #endregion

    #region Helper Methods

    private Vector3 GetStrafePosition(Transform Target)
    {
        float distance = 1.5f;

        NavMeshHit hit;

        // Çember üzerinde bir noktayý hesapla
        Vector3 candidatePosition = transform.position + Vector3.Cross(transform.forward, Vector3.up) * distance;

        // NavMesh kontrolü
        if (NavMesh.SamplePosition(candidatePosition, out hit, distance, NavMesh.AllAreas))
        {
            // bu seçilen pozisyonda hedefe halen ulaþabiliyorsa:
            if (Physics.Raycast(candidatePosition, Target.position))
            {
                return candidatePosition;
            }
        }

        candidatePosition = transform.position + -Vector3.Cross(transform.forward, Vector3.up) * distance;
        if (NavMesh.SamplePosition(candidatePosition, out hit, distance, NavMesh.AllAreas))
        {
            // bu seçilen pozisyonda hedefe halen ulaþabiliyorsa:
            if (Physics.Raycast(candidatePosition, Target.position))
            {
                return candidatePosition;
            }
        }

        return Vector3.zero;
    }

    Vector3 GetRandomPoint()
    {
        int tryCount = 10;
        NavMeshHit hit;
        while (tryCount > 0)
        {
            Vector3 randomDirection = Random.insideUnitSphere * RandomPatrolRadius; // Belirli bir yarýçap içinde rastgele bir nokta seç
            randomDirection += transform.position;

            if (NavMesh.SamplePosition(randomDirection, out hit, RandomPatrolRadius, NavMesh.AllAreas))
            {
                return hit.position; // Geçerli bir NavMesh pozisyonu döndür
            }
            tryCount--;
        }
        return Vector3.zero; // NavMesh üzerinde geçerli bir nokta yoksa
    }


    /// <returns>verilen yönde 170 derecelik bir açý ilepozisyon arayýp return eder</returns>
    Vector3 GetRandomPointInDirection(Vector3 Direction, float RangeRadius)
    {
        int tryCount = 10;
        NavMeshHit hit;

        // Normalize direction to ensure consistent behavior
        Direction = Direction.normalized;

        while (tryCount > 0)
        {
            // Generate a random angle within 170 degrees (±85 degrees from the given direction)
            float randomAngle = Random.Range(-85f, 85f);
            Quaternion rotation = Quaternion.Euler(0, randomAngle, 0);

            // Rotate the direction by the random angle
            Vector3 adjustedDirection = rotation * Direction;

            // Generate a random point in the given direction
            Vector3 randomPoint = transform.position + adjustedDirection * RangeRadius;

            // Check if the point is on the NavMesh
            if (NavMesh.SamplePosition(randomPoint, out hit, RangeRadius, NavMesh.AllAreas))
            {
                return hit.position; // Return a valid NavMesh position
            }

            tryCount--;
        }

        // If no valid position is found, return zero vector
        return Vector3.zero;
    }

    #endregion
}
