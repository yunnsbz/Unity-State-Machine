

public class StateMachine
{
    public AIState currentState { get; private set; }

    public void ChangeState(AIState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }

    public void Update()
    {
        currentState?.UpdateState();
    }
}
