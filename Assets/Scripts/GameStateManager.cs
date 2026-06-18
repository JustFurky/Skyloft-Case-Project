using System;
using UnityEngine;
using Zenject;
using SkyloftGame.StateMachine;
using SkyloftGame.States;
using SkyloftGame.Level;
using SkyloftGame.Gameplay;
using SkyloftGame.Data;

public class GameStateManager : MonoBehaviour
{
    [Header("Startup")]
    [SerializeField] private GameStateType _initialState = GameStateType.Menu;

    private ILevelService   _level;
    private IEnemySpawner   _spawner;
    private IScoreService   _score;
    private ICountdownTimer _timer;
    private DataManager     _data;

    public ILevelService   Level   => _level;
    public IEnemySpawner   Spawner => _spawner;
    public IScoreService   Score   => _score;
    public ICountdownTimer Timer   => _timer;
    public DataManager     Data    => _data;

    public GameStateType CurrentState  => _machine != null ? _machine.CurrentStateType  : GameStateType.None;
    public GameStateType PreviousState => _machine != null ? _machine.PreviousStateType : GameStateType.None;

    public event Action<GameStateType, GameStateType> OnStateChanged;

    private GameStateMachine _machine;

    [Inject]
    private void Construct(ILevelService level, IEnemySpawner spawner, IScoreService score,
                           ICountdownTimer timer, DataManager data)
    {
        _level   = level;
        _spawner = spawner;
        _score   = score;
        _timer   = timer;
        _data    = data;

        _timer.OnElapsed += HandleTimerElapsed;
        InitializeMachine();
    }

    private void OnDestroy()
    {
        if (_timer != null) _timer.OnElapsed -= HandleTimerElapsed;
    }

    private void InitializeMachine()
    {
        _machine = new GameStateMachine();
        _machine.RegisterState(GameStateType.Menu,     new MenuState(this));
        _machine.RegisterState(GameStateType.Playing,  new PlayingState(this));
        _machine.RegisterState(GameStateType.GameWon,  new GameWonState(this));
        _machine.RegisterState(GameStateType.GameLost, new GameLostState(this, _data));
        _machine.OnStateChanged += (prev, next) => OnStateChanged?.Invoke(prev, next);
        _machine.TransitionTo(_initialState);
    }

    private void Update()      => _machine?.Update();
    private void FixedUpdate() => _machine?.FixedUpdate();

    public void ChangeState(GameStateType state) => _machine.TransitionTo(state);

    public void GoToMenu()  => ChangeState(GameStateType.Menu);
    public void StartGame() => ChangeState(GameStateType.Playing);
    public void WinGame()   => ChangeState(GameStateType.GameWon);
    public void LoseGame()  => ChangeState(GameStateType.GameLost);

    public void StartNewGame()
    {
        _data?.ResetLevelProgress();
        Level?.Load(0);
        StartGame();
    }

    public void ContinueGame()
    {
        if (Level == null) { StartGame(); return; }

        int index = Mathf.Clamp(Level.HighestUnlockedIndex, 0, Mathf.Max(0, Level.Count - 1));
        Level.Load(index);
        StartGame();
    }

    public void ReplayLevel() => StartGame();

    public void GoToNextLevel()
    {
        if (Level != null && Level.TryAdvance()) StartGame();
        else GoToMenu();
    }

    private void HandleTimerElapsed()
    {
        if (CurrentState == GameStateType.Playing) WinGame();
    }
}
