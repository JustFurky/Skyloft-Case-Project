using SkyloftGame.Data;

namespace SkyloftGame.States
{
    public class GameLostState : StateMachine.IState
    {
        private readonly GameStateManager _manager;
        public GameLostState(GameStateManager manager) => _manager = manager;

        public void Enter()
        {
            _manager.Spawner?.StopAndClear();
            _manager.Timer?.Stop();
            DataManager.Instance?.Save();
        }

        public void Update()      { }
        public void FixedUpdate() { }
        public void Exit()        { }
    }
}
