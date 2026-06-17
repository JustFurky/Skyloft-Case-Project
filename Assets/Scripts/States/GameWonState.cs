namespace SkyloftGame.States
{
    public class GameWonState : StateMachine.IState
    {
        private readonly GameStateManager _manager;
        public GameWonState(GameStateManager manager) => _manager = manager;

        public void Enter()
        {
            _manager.Spawner?.StopAndClear();
            _manager.Timer?.Stop();
            _manager.Level?.MarkCurrentCompleted();
        }

        public void Update()      { }
        public void FixedUpdate() { }
        public void Exit()        { }
    }
}
