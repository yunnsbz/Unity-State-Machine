using System.Collections;
using UnityEngine;

public class AIController_Perception : MonoBehaviour
{
    // values:
    public float PerceptionUpdateInterval = 0.05f;
    public float PerceptionOnSightUpdateInterval = 0.5f;
    public float MaxVisualPerceptionDistance = 20f;
    public float ForwardViewAngle = 60;

    // components:
    private AIController Controller;

    // coroutines:
    Coroutine Coroutine_LineofSight;
    Coroutine Coroutine_FolowTargetSight;
    Coroutine Coroutine_SearchForTarget;
    private WaitForSeconds Wait;
    private WaitForSeconds WaitOnSight;

    // raycast hit layers:
    public LayerMask targetLayerMask;
    public LayerMask nontargetLayerMask;

    // hedef:
    public Transform TargetOnSight { get; set; }
    public Vector3 TargetlastSeenPosition { get; set; }

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

    public virtual void StartLookingForTarget(bool IsPatrolling)
    {
        if (IsPatrolling)
        {
            if (Coroutine_LineofSight == null)
                Coroutine_LineofSight = StartCoroutine(CheckLineOfSight());
        }
        else
        {
            MaxVisualPerceptionDistance *= 2;
            ForwardViewAngle = 80;
            if (Coroutine_LineofSight == null)
                Coroutine_LineofSight = StartCoroutine(CheckLineOfSight());
        }
    }

    /// <summary>
    /// ranged sald�r�lar i�in ne �ok yak�n ne �ok uzak olmal�:
    /// </summary>
    public virtual bool IsTargetInAttackRange()
    {
        // mesafe hesaplan�r ve max min range ile kar��la�t�r�l�r
        var distance = Vector3.Distance(TargetOnSight.position, transform.position);
        if (distance < Controller.Movement.RangeMinDistance || distance > Controller.Movement.RangeMaxDistance)
            return false;
        else return true;
    }

    public virtual void StartFollowingTargetSight()
    {
        if (Coroutine_FolowTargetSight == null)
            Coroutine_FolowTargetSight = StartCoroutine(FollowTargetSight(TargetOnSight));
    }

    public virtual void StartSearchForTarget()
    {
        if (Coroutine_SearchForTarget == null)
            Coroutine_SearchForTarget = StartCoroutine(SearchForTarget());
    }

    private IEnumerator CheckLineOfSight()
    {
        while (enabled)
        {
            for (int i = 0; i < Controller.Targetables.Count; i++)
            {
                // mesafe kontrol�
                if (Vector3.Distance(transform.position, Controller.Targetables[i].transform.position) <= MaxVisualPerceptionDistance)
                {
                    var targetDir = CalculateTargetViewAngle(Controller.Targetables[i].transform);
                    Vector3 origin = transform.position + Vector3.up; // Ba�lang�� noktas� (y�kseklik ayar�)
                    // g�r�� a��s� kontrol�
                    if (targetDir != Vector3.zero)
                    {
                        // raycast ile duvar kontrol�
                        if (Physics.Raycast(origin, targetDir, out RaycastHit TargetHit, MaxVisualPerceptionDistance, targetLayerMask))
                        {
                            // Engelin hedefin �n�nde olup olmad���n� kontrol et
                            if (Physics.Raycast(origin, targetDir, out RaycastHit obstacleHit, MaxVisualPerceptionDistance, nontargetLayerMask))
                            {

                                // Engel hedefe daha yak�nsa
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

    private Vector3 CalculateTargetViewAngle(Transform target)
    {
        if (target == null)
            return Vector3.zero;

        // Hedefe olan y�n vekt�r�n� hesapla
        Vector3 hedefYonu = (target.position - transform.position).normalized;

        // �leri y�n ile hedef y�n� aras�ndaki a��y� dot product ile kontrol et
        float dot = Vector3.Dot(transform.forward, hedefYonu);

        // G�r�� a��s�n�n cosin�s de�erini hesapla
        float gorusCos = Mathf.Cos(ForwardViewAngle * Mathf.Deg2Rad);

        // E�er dot de�eri g�r�� a��s�n�n cosin�s�nden b�y�kse hedef g�r�� a��s�nda
        if (dot >= gorusCos)
            return hedefYonu;
        else return Vector3.zero;
    }

    // hedefin son konumunu her iki kontrolde bir kaydetsin diye kullan�l�r
    bool lastSeenCheck = true;

    /// <summary>
    /// e�er hedef g�r�� alan�ndan ��karsa IsTargetOnSight false olur
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
            // buraya her geldi�inde son konumu kaydetmesi �ok h�zl� oluyor bu y�zden her iki geli�te bir son konumu kaydetsin diye lastSeenCheck de�i�kenini bu �ekilde kulland�m
            if (lastSeenCheck)
            {
                TargetlastSeenPosition = Target.position;
                lastSeenCheck = false;
            }
            else lastSeenCheck = true;


            // raycast ile duvar kontrol�
            Vector3 origin = transform.position + Vector3.up; // Ba�lang�� noktas� (y�kseklik ayar�)
            var targetDir = (Target.transform.position - transform.position);

            // Engelin hedefin �n�nde olup olmad���n� kontrol et
            if (Physics.Raycast(origin, targetDir, out RaycastHit obstacleHit, distance, nontargetLayerMask))
            {
                var TargetDistance = (Target.transform.position - transform.position);

                // Engel hedefe daha yak�nsa
                if (obstacleHit.distance < TargetDistance.magnitude)
                {
                    if (checkAgain)
                    {
                        // son kaybetti�in yerden tekrar kontrol et
                        if (Physics.Raycast(origin, targetDir, out obstacleHit, distance, nontargetLayerMask))
                        {
                            TargetDistance = (Target.transform.position - transform.position);

                            // Engel hedefe daha yak�nsa
                            if (obstacleHit.distance < TargetDistance.magnitude)
                            {
                                lastSeenCheck = true;
                                // target lost:
                                TargetOnSight = null;
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


    private IEnumerator SearchForTarget()
    {
        for (int i = 0; i < Controller.Targetables.Count; i++)
        {
            var distance = MaxVisualPerceptionDistance * 3;
            while (enabled)
            {
                // raycast ile duvar kontrol�
                Vector3 origin = transform.position + Vector3.up; // Ba�lang�� noktas� (y�kseklik ayar�)
                var targetDir = (Controller.Targetables[i].transform.position - transform.position);

                // Engelin hedefin �n�nde olup olmad���n� kontrol et
                RaycastHit obstacleHit;
                if (Physics.Raycast(origin, targetDir, out obstacleHit, distance, nontargetLayerMask))
                {
                    var TargetDistance = (Controller.Targetables[i].transform.position - transform.position);

                    //  hedef Engelden daha yak�nsa
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
}
