namespace SkyloftGame.States
{
    /// <summary>
    /// Ana menü durumu: oynanış alt sistemlerini boşa alır.
    /// UI yönlendirmesi UIController tarafından OnStateChanged ile yapılır.
    /// </summary>
    public class MenuState : StateMachine.IState
    {
        private readonly GameStateManager _manager;
        public MenuState(GameStateManager manager) => _manager = manager;

        public void Enter()
        {
            _manager.Spawner?.StopAndClear();
            _manager.Timer?.Stop();
        }

        public void Update()      { }
        public void FixedUpdate() { }
        public void Exit()        { }
    }
}
