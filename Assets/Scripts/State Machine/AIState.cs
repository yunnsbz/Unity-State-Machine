
public abstract class AIState
{
    protected AIController controller;
    protected StateMachine stateMachine;

    public AIState(AIController controller, StateMachine stateMachine)
    {
        this.controller = controller;
        this.stateMachine = stateMachine;
    }

    public abstract void EnterState(); // Duruma girerken yapýlacak iþlemler
    public abstract void ExitState();  // Durumdan çýkarken yapýlacak iþlemler
    public abstract void UpdateState(); // Durumun sürekli kontrol edilmesi gereken davranýþý
}
