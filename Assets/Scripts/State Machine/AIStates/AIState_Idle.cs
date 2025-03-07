using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIState_Idle : AIState
{
    public AIState_Idle(AIController controller, StateMachine stateMachine) : base(controller, stateMachine) { }

    public override void EnterState()
    {
        // patrol yapabiliyor mu yapamýyor mu: eðer yapamýyorsa daha büyük bir algýlama mesafesi olur:
        var PatrolResult = controller.Movement.StartPatrolling();
        controller.Perception.StartLookingForTarget(PatrolResult);
    }

    public override void ExitState()
    {
        controller.Movement.StopPatrolling();
    }

    public override void UpdateState()
    {
        if (controller.Perception.TargetOnSight != null)
        {
            stateMachine.ChangeState(new AIState_TakeRange(controller, stateMachine));
        }
    }
}
