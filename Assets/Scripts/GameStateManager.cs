using System;
using UnityEngine;
using SkyloftGame.StateMachine;
using SkyloftGame.States;
using SkyloftGame.Level;
using SkyloftGame.Gameplay;

/// <summary>
/// Oyunun kompozisyon kökü (composition root) ve durum makinesi sürücüsü.
///
/// Alt sistemleri (seviye, spawner, skor, sayaç) tek noktada toplar ve onları
/// arayüz tipleri üzerinden state'lere sunar (DIP). State'ler somut sınıflara
/// değil ILevelService / IEnemySpawner / IScoreService / ICountdownTimer'a bağlıdır.
/// UI ve oyuncu, OnStateChanged olayına abone olarak gevşek bağlı kalır.
///
/// Bu manager, tüketicilerden (UI/oyuncu) önce çalışacak şekilde erken yürütme
/// sırasına alınır; böylece Instance ve durum makinesi herkesten önce hazırdır.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Alt Sistemler (sahne referansları)")]
    [SerializeField] private LevelManager _levelManager;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private ScoreService _scoreService;

    [Header("Başlangıç")]
    [SerializeField] private GameStateType _initialState = GameStateType.Menu;

    // Arayüz üzerinden açılan bağımlılıklar
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

    // ----------------------------------------------------------------- //
    //  Genel durum geçişleri
    // ----------------------------------------------------------------- //

    public void ChangeState(GameStateType state) => _machine.TransitionTo(state);

    public void GoToMenu()  => ChangeState(GameStateType.Menu);
    public void StartGame() => ChangeState(GameStateType.Playing);
    public void WinGame()   => ChangeState(GameStateType.GameWon);
    public void LoseGame()  => ChangeState(GameStateType.GameLost);

    // ----------------------------------------------------------------- //
    //  Üst seviye akış komutları (UI butonları bunları çağırır)
    // ----------------------------------------------------------------- //

    /// <summary>İlk seviyeden yeni oyun başlatır.</summary>
    public void StartNewGame()
    {
        Level?.Load(0);
        StartGame();
    }

    /// <summary>Mevcut seviyeyi yeniden oynatır (kazanma/kaybetme ekranından).</summary>
    public void ReplayLevel() => StartGame();

    /// <summary>Sonraki seviyeye geçer; yoksa menüye döner.</summary>
    public void GoToNextLevel()
    {
        if (Level != null && Level.TryAdvance()) StartGame();
        else GoToMenu();
    }

    private void HandleTimerElapsed()
    {
        // Süre dolunca, yalnızca oynanış sürerken zaferi tetikle.
        if (CurrentState == GameStateType.Playing) WinGame();
    }
}
