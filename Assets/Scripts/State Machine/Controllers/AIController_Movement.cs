using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AIController_Movement : MonoBehaviour
{
    // values:
    [Header("taking range")]
    public float RangeMinDistance = 8f;
    public float RangeMaxDistance = 15f;
    private float RangeTakingSpeed = 2f;
    private float RandomRangeRadius = 4f; // D��man�n dola�abilece�i maksimum yar��ap

    [Header("patroling")]
    public float RandomPatrolRadius = 9f; // D��man�n dola�abilece�i maksimum yar��ap
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
        PatrolWait = new WaitForSeconds(0.9f);
    }

    private void OnDisable()
    {
        if (C_Patrol != null)
        {
            StopCoroutine(C_Patrol);
            C_Patrol = null;
        }
        if (C_TakeRange != null)
        {
            StopCoroutine(C_TakeRange);
            C_TakeRange = null;
        }
        if (C_LookToTarget != null)
        {
            StopCoroutine(C_LookToTarget);
            C_LookToTarget = null;
        }
        if (C_Investigate != null)
        {
            StopCoroutine(C_Investigate);
            C_Investigate = null;
        }
        if (C_Strafe != null)
        {
            StopCoroutine(C_Strafe);
            C_Strafe = null;
        }
        if (C_Push != null)
        {
            StopCoroutine(C_Push);
            C_Push = null;
        }
    }


    /// <returns>patrol yapabiliyorsa true d�nd�r�r</returns>
    public virtual bool StartPatrolling()
    {
        Controller.Agent.updateRotation = true;
        Controller.Agent.updatePosition = true;
        // �nceden belirlenmi� hareket noktas� varsa oraya ge�i� yapar
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

    public virtual void StartTakingRange(Transform target)
    {
        Controller.Agent.updateRotation = false;
        // mesafe hesaplan�r ve yakla�mas� m� yoksa uzakla�mas� m� gerekti�ini hesaplar
        var distance = Vector3.Distance(target.position, transform.position);
        if (distance < RangeMinDistance)
        {
            // hedefin tersi y�nde hareket et:
            var direction = transform.position - target.position;
            var GoToPosition = transform.position + direction.normalized * RangeTakingSpeed;
            var distanceToTake = Vector3.Distance(transform.position, GoToPosition);

            // e�er hedeften ka�am�yorsan rastgele bir pozisyon se�ip oraya ka� sonra tekrar normal range al�rs�n
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
                // hedeften uzakla�
                if (C_TakeRange == null)
                    C_TakeRange = StartCoroutine(GoToTarget(GoToPosition));
            }
        }
        if (distance > RangeMaxDistance)
        {
            // hedefe do�ru hareket et:
            var direction = target.position - transform.position;
            var GoToPosition = transform.position * RangeTakingSpeed;
            var distanceToTake = Vector3.Distance(transform.position, GoToPosition);

            // e�er hedeften ka�am�yorsan rastgele bir pozisyon se�ip oraya ka� sonra tekrar normal range al�rs�n
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
                // hedeften uzakla�
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

    /// <summary>
    /// ate� ederken kullan�l�r. hedefe ni�an almak i�in kullan�l�r
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

    /// <summary>
    /// hedefin en son g�r�ld��� noktaya gider
    /// </summary>
    public virtual void StartInvestigateTarget(Vector3 TargetLastSeenPos)
    {
        // last seen'e do�ru y�r�
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


    /// <summary>
    /// range'den ��kmadan hareket eder
    /// </summary>
    /// 
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

    private IEnumerator GoToTarget(Vector3 TargetPos)
    {
        Controller.Agent.isStopped = false;
        Controller.Agent.SetDestination(TargetPos);
        // Hedefe ula�may� bekle
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

    private IEnumerator _PushToTarget(Vector3 TargetPos)
    {
        Controller.Agent.isStopped = false;
        Controller.Agent.SetDestination(TargetPos);
        // Hedefe ula�may� bekle
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
            // Hedef y�n�ne do�ru bir vekt�r hesapla
            Vector3 direction = (target.position - transform.position).normalized;

            // D�n���n sadece yatay eksende (y ekseni) olmas� i�in:
            direction.y = 0;

            if (direction.magnitude > 0.05f) // �ok k���k y�nler i�in i�lem yapma
            {
                // �stenen rotasyonu hesapla
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Mevcut rotasyonu hedef rotasyona do�ru d�nd�r
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
            // Rastgele bir hedef pozisyon se�
            Vector3 wanderTarget = _PatrolPoints[iter++].position;

            // NavMesh �zerinde ge�erli bir pozisyon bulunursa oraya git
            if (wanderTarget != Vector3.zero)
            {
                Controller.Agent.isStopped = false;
                Controller.Agent.SetDestination(wanderTarget);
                //Debug.Log("patroling to: " + wanderTarget);
                // Hedefe ula�may� bekle
                while (Controller.Agent.pathPending || Controller.Agent.remainingDistance > Controller.Agent.stoppingDistance + 0.1f)
                {
                    yield return null;
                }
            }
            // Bir s�re bekle
            yield return PatrolWait;
        }
    }

    IEnumerator RandomPatrolRoutine()
    {
        while (enabled)
        {
            // Rastgele bir hedef pozisyon se�
            Vector3 wanderTarget = GetRandomPoint();

            // NavMesh �zerinde ge�erli bir pozisyon bulunursa oraya git
            if (wanderTarget != Vector3.zero)
            {
                Controller.Agent.isStopped = false;
                Controller.Agent.SetDestination(wanderTarget);
                //Debug.Log("New destination set");

                // Hedefe ula�may� bekle
                while (Controller.Agent.pathPending || Controller.Agent.remainingDistance > Controller.Agent.stoppingDistance + 0.1f)
                {
                    yield return null;
                }
                //Debug.Log("waited to reach target");
            }
            // Bir s�re bekle
            //Debug.Log("wandering around");
            yield return PatrolWait;
        }
    }

    IEnumerator WalkOnCircle(Transform Target)
    {
        while (enabled)
        {
            Vector3 StrafeTarget = GetStrafePosition(Target);

            // NavMesh �zerinde ge�erli bir pozisyon bulunursa oraya git
            if (StrafeTarget != Vector3.zero)
            {
                Controller.Agent.isStopped = false;
                Controller.Agent.SetDestination(StrafeTarget);
                // Hedefe ula�may� bekle
                while (Controller.Agent.pathPending || Controller.Agent.remainingDistance > Controller.Agent.stoppingDistance + 0.1f)
                {
                    yield return null;
                }
            }
            // Bir s�re bekle
            yield return PatrolWait;
        }
    }

    private Vector3 GetStrafePosition(Transform Target)
    {
        float distance = 1.5f;

        NavMeshHit hit;

        // �ember �zerinde bir noktay� hesapla
        Vector3 candidatePosition = transform.position + Vector3.Cross(transform.forward, Vector3.up) * distance;

        // NavMesh kontrol�
        if (NavMesh.SamplePosition(candidatePosition, out hit, distance, NavMesh.AllAreas))
        {
            // bu se�ilen pozisyonda hedefe halen ula�abiliyorsa:
            if (Physics.Raycast(candidatePosition, Target.position))
            {
                return candidatePosition;
            }
        }

        candidatePosition = transform.position + -Vector3.Cross(transform.forward, Vector3.up) * distance;
        if (NavMesh.SamplePosition(candidatePosition, out hit, distance, NavMesh.AllAreas))
        {
            // bu se�ilen pozisyonda hedefe halen ula�abiliyorsa:
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
            Vector3 randomDirection = Random.insideUnitSphere * RandomPatrolRadius; // Belirli bir yar��ap i�inde rastgele bir nokta se�
            randomDirection += transform.position;

            if (NavMesh.SamplePosition(randomDirection, out hit, RandomPatrolRadius, NavMesh.AllAreas))
            {
                return hit.position; // Ge�erli bir NavMesh pozisyonu d�nd�r
            }
            tryCount--;
        }
        return Vector3.zero; // NavMesh �zerinde ge�erli bir nokta yoksa
    }


    /// <returns>verilen y�nde 170 derecelik bir a�� ilepozisyon aray�p return eder</returns>
    Vector3 GetRandomPointInDirection(Vector3 Direction, float RangeRadius)
    {
        int tryCount = 10;
        NavMeshHit hit;

        // Normalize direction to ensure consistent behavior
        Direction = Direction.normalized;

        while (tryCount > 0)
        {
            // Generate a random angle within 170 degrees (�85 degrees from the given direction)
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

}
