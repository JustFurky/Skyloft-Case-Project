using System;
using UnityEngine;
using SkyloftGame.StateMachine;
using SkyloftGame.States;
using SkyloftGame.Level;
using SkyloftGame.Gameplay;

[DefaultExecutionOrder(-100)]
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Subsystems (scene references)")]
    [SerializeField] private LevelManager _levelManager;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private ScoreService _scoreService;

    [Header("Startup")]
    [SerializeField] private GameStateType _initialState = GameStateType.Menu;

    public ILevelService   Level   => _levelManager;
    public IEnemySpawner   Spawner => _enemySpawner;
    public IScoreService   Score   => _scoreService;
    public ICountdownTimer Timer   { get; private set; }

    public GameStateType CurrentState  => _machine.CurrentStateType;
    public GameStateType PreviousState => _machine.PreviousStateType;

    public event Action<GameStateType, GameStateType> OnStateChanged;

    private GameStateMachine _machine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Timer = new CountdownTimer();
        Timer.OnElapsed += HandleTimerElapsed;

        InitializeMachine();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (Timer != null) Timer.OnElapsed -= HandleTimerElapsed;
    }

    private void InitializeMachine()
    {
        _machine = new GameStateMachine();
        _machine.RegisterState(GameStateType.Menu,     new MenuState(this));
        _machine.RegisterState(GameStateType.Playing,  new PlayingState(this));
        _machine.RegisterState(GameStateType.GameWon,  new GameWonState(this));
        _machine.RegisterState(GameStateType.GameLost, new GameLostState(this));
        _machine.OnStateChanged += (prev, next) => OnStateChanged?.Invoke(prev, next);
        _machine.TransitionTo(_initialState);
    }

    private void Update()      => _machine.Update();
    private void FixedUpdate() => _machine.FixedUpdate();

    public void ChangeState(GameStateType state) => _machine.TransitionTo(state);

    public void GoToMenu()  => ChangeState(GameStateType.Menu);
    public void StartGame() => ChangeState(GameStateType.Playing);
    public void WinGame()   => ChangeState(GameStateType.GameWon);
    public void LoseGame()  => ChangeState(GameStateType.GameLost);

    public void StartNewGame()
    {
        SkyloftGame.Data.DataManager.Instance?.ResetLevelProgress();
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
