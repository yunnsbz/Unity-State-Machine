using System.Collections;
using UnityEngine;

public class AIState_Attack : AIState
{
    public AIState_Attack(AIController controller, StateMachine stateMachine)
         : base(controller, stateMachine) { }

    public override void EnterState()
    {
        // hedefi al
        var target = controller.Perception.TargetOnSight;

        if (target != null)
        {
            controller.Perception.StartFollowingTargetSight();
            controller.Movement.StartLookingToTarget(target);
            controller.Movement.StartStrafing(target);
            controller.Attacks.StartAttackingTarget(target);

        }
        else throw new System.Exception("target on sight is null");
    }

    public override void ExitState()
    {
        controller.Movement.StopStrafing();
        controller.Attacks.StopAttacking();
        controller.Movement.StopLookingToTarget();
    }

    public override void UpdateState()
    {
        if (controller.Perception.TargetOnSight == null)
        {
            stateMachine.ChangeState(new AIState_SearchForTarget(controller, stateMachine));
        }
        else if (!controller.Perception.IsTargetInAttackRange())
        {
            stateMachine.ChangeState(new AIState_TakeRange(controller, stateMachine));
        }
    }
}
