
public abstract class AIState
{
    protected AIController controller;
    protected StateMachine stateMachine;

    public AIState(AIController controller, StateMachine stateMachine)
    {
        this.controller = controller;
        this.stateMachine = stateMachine;
    }

    public abstract void EnterState(); // Duruma girerken yap�lacak i�lemler
    public abstract void ExitState();  // Durumdan ��karken yap�lacak i�lemler
    public abstract void UpdateState(); // Durumun s�rekli kontrol edilmesi gereken davran���
}
