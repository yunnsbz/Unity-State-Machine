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
    /// ranged saldýrýlar için ne çok yakýn ne çok uzak olmalý:
    /// </summary>
    public virtual bool IsTargetInAttackRange()
    {
        // mesafe hesaplanýr ve max min range ile karþýlaþtýrýlýr
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
                // mesafe kontrolü
                if (Vector3.Distance(transform.position, Controller.Targetables[i].transform.position) <= MaxVisualPerceptionDistance)
                {
                    var targetDir = CalculateTargetViewAngle(Controller.Targetables[i].transform);
                    Vector3 origin = transform.position + Vector3.up; // Baþlangýç noktasý (yükseklik ayarý)
                    // görüþ açýsý kontrolü
                    if (targetDir != Vector3.zero)
                    {
                        // raycast ile duvar kontrolü
                        if (Physics.Raycast(origin, targetDir, out RaycastHit TargetHit, MaxVisualPerceptionDistance, targetLayerMask))
                        {
                            // Engelin hedefin önünde olup olmadýðýný kontrol et
                            if (Physics.Raycast(origin, targetDir, out RaycastHit obstacleHit, MaxVisualPerceptionDistance, nontargetLayerMask))
                            {

                                // Engel hedefe daha yakýnsa
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

        // Hedefe olan yön vektörünü hesapla
        Vector3 hedefYonu = (target.position - transform.position).normalized;

        // Ýleri yön ile hedef yönü arasýndaki açýyý dot product ile kontrol et
        float dot = Vector3.Dot(transform.forward, hedefYonu);

        // Görüþ açýsýnýn cosinüs deðerini hesapla
        float gorusCos = Mathf.Cos(ForwardViewAngle * Mathf.Deg2Rad);

        // Eðer dot deðeri görüþ açýsýnýn cosinüsünden büyükse hedef görüþ açýsýnda
        if (dot >= gorusCos)
            return hedefYonu;
        else return Vector3.zero;
    }

    // hedefin son konumunu her iki kontrolde bir kaydetsin diye kullanýlýr
    bool lastSeenCheck = true;

    /// <summary>
    /// eðer hedef görüþ alanýndan çýkarsa IsTargetOnSight false olur
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
            // buraya her geldiðinde son konumu kaydetmesi çok hýzlý oluyor bu yüzden her iki geliþte bir son konumu kaydetsin diye lastSeenCheck deðiþkenini bu þekilde kullandým
            if (lastSeenCheck)
            {
                TargetlastSeenPosition = Target.position;
                lastSeenCheck = false;
            }
            else lastSeenCheck = true;


            // raycast ile duvar kontrolü
            Vector3 origin = transform.position + Vector3.up; // Baþlangýç noktasý (yükseklik ayarý)
            var targetDir = (Target.transform.position - transform.position);

            // Engelin hedefin önünde olup olmadýðýný kontrol et
            if (Physics.Raycast(origin, targetDir, out RaycastHit obstacleHit, distance, nontargetLayerMask))
            {
                var TargetDistance = (Target.transform.position - transform.position);

                // Engel hedefe daha yakýnsa
                if (obstacleHit.distance < TargetDistance.magnitude)
                {
                    if (checkAgain)
                    {
                        // son kaybettiðin yerden tekrar kontrol et
                        if (Physics.Raycast(origin, targetDir, out obstacleHit, distance, nontargetLayerMask))
                        {
                            TargetDistance = (Target.transform.position - transform.position);

                            // Engel hedefe daha yakýnsa
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
                // raycast ile duvar kontrolü
                Vector3 origin = transform.position + Vector3.up; // Baþlangýç noktasý (yükseklik ayarý)
                var targetDir = (Controller.Targetables[i].transform.position - transform.position);

                // Engelin hedefin önünde olup olmadýðýný kontrol et
                RaycastHit obstacleHit;
                if (Physics.Raycast(origin, targetDir, out obstacleHit, distance, nontargetLayerMask))
                {
                    var TargetDistance = (Controller.Targetables[i].transform.position - transform.position);

                    //  hedef Engelden daha yakýnsa
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
