namespace SkyloftGame.States
{
    /// <summary>
    /// Zafer durumu: spawn ve sayaç durdurulur, ilerleme kalıcı kaydedilir.
    /// "Game Won" menüsü UIController üzerinden gösterilir ve tur öldürme sayısını sunar.
    /// </summary>
    public class GameWonState : StateMachine.IState
    {
        private readonly GameStateManager _manager;
        public GameWonState(GameStateManager manager) => _manager = manager;

        public void Enter()
        {
            _manager.Spawner?.StopAndClear();
            _manager.Timer?.Stop();
            _manager.Level?.MarkCurrentCompleted();   // sonraki seviyeyi aç + diske yaz
        }

        public void Update()      { }
        public void FixedUpdate() { }
        public void Exit()        { }
    }
}
