using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController_Attack : MonoBehaviour
{

    private AIController controller;

    private void Awake()
    {
        controller = GetComponent<AIController>();
    }

    private void Start()
    {
        
    }


    public virtual void StartAttackingTarget(Transform target)
    {
        
    }
    public virtual void StopAttacking()
    {
        
    }
}
