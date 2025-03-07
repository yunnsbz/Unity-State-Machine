using System.Collections;

public class AIState_TakeRange : AIState
{
    public AIState_TakeRange(AIController controller, StateMachine stateMachine)
        : base(controller, stateMachine) { }

    public override void EnterState()
    {
        controller.Movement.StartTakingRange(controller.Perception.TargetOnSight);
        controller.Movement.StartLookingToTarget(controller.Perception.TargetOnSight);
        controller.Perception.StartFollowingTargetSight();

        // range alýrken saldýrabilirsin:
        controller.Attacks.StartAttackingTarget(controller.Perception.TargetOnSight);
    }

    public override void ExitState()
    {
        controller.Attacks.StopAttacking();
        controller.Movement.StopTakingRange();
        controller.Movement.StopLookingToTarget();
    }

    public override void UpdateState()
    {
        if (controller.Perception.TargetOnSight == null)
        {
            stateMachine.ChangeState(new AIState_SearchForTarget(controller, stateMachine));
        }
        else if (controller.Perception.IsTargetInAttackRange())
        {
            stateMachine.ChangeState(new AIState_Attack(controller, stateMachine));
        }
        else
        {
            controller.Movement.StartTakingRange(controller.Perception.TargetOnSight);
        }
    }
}
