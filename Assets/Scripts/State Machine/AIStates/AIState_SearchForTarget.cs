using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIState_SearchForTarget : AIState
{
    public AIState_SearchForTarget(AIController controller, StateMachine stateMachine)
        : base(controller, stateMachine) { }

    bool IsInvestigting = false;

    public override void EnterState()
    {
        controller.Perception.StartSearchForTarget();
        // eðer hedefin son konumu biliniyorsa oraya gitsin
        controller.Movement.StartInvestigateTarget(controller.Perception.TargetlastKnownPosition);
        IsInvestigting = true;
    }

    public override void ExitState()
    {
        controller.Movement.StopPatrolling();
        controller.Movement.StopInvestigateTarget();
        IsInvestigting = false;
    }

    public override void UpdateState()
    {

        if (IsInvestigting && (!controller.Agent.pathPending && controller.Agent.remainingDistance <= controller.Agent.stoppingDistance) && (!controller.Agent.hasPath || controller.Agent.pathStatus == NavMeshPathStatus.PathComplete))
        {
            // Investigate iþlemi tamamlandý da agent yerinde duruyorsa boþta beklemesin patrol yapsýn
            controller.Movement.StartRandomPatrol();
        }
        if (controller.Perception.TargetOnSight != null)
        {
            stateMachine.ChangeState(new AIState_TakeRange(controller, stateMachine));
        }
    }
}
