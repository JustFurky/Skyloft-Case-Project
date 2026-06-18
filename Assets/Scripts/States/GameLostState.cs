using SkyloftGame.Data;

namespace SkyloftGame.States
{
    public class GameLostState : StateMachine.IState
    {
        private readonly GameStateManager _manager;
        private readonly DataManager      _data;

        public GameLostState(GameStateManager manager, DataManager data)
        {
            _manager = manager;
            _data    = data;
        }

        public void Enter()
        {
            _manager.Spawner?.StopAndClear();
            _manager.Timer?.Stop();
            _data?.Save();
        }

        public void Update()      { }
        public void FixedUpdate() { }
        public void Exit()        { }
    }
}
