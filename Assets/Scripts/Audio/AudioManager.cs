using System.Collections.Generic;
using UnityEngine;
using SkyloftGame.Enemy;
using SkyloftGame.StateMachine;
using EnemyUnit = SkyloftGame.Enemy.Enemy;

namespace SkyloftGame.Audio
{
    [DefaultExecutionOrder(-90)]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [System.Serializable]
        private struct CueClip
        {
            public AudioCue cue;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume;
        }

        [Header("Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        [Header("Music")]
        [SerializeField] private AudioClip _menuMusic;
        [SerializeField] private AudioClip _gameplayMusic;

        [Header("SFX Mappings")]
        [SerializeField] private CueClip[] _cues;

        private readonly Dictionary<AudioCue, CueClip> _clipByCue = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (transform.parent != null) transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            foreach (var entry in _cues)
                _clipByCue[entry.cue] = entry;
        }

        private void OnEnable()
        {
            AudioEvents.CuePlayed += PlayCue;
            EnemyRegistry.Killed  += HandleEnemyKilled;

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
                HandleStateChanged(GameStateType.None, GameStateManager.Instance.CurrentState);
            }
        }

        private void OnDisable()
        {
            AudioEvents.CuePlayed -= PlayCue;
            EnemyRegistry.Killed  -= HandleEnemyKilled;
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void HandleEnemyKilled(EnemyUnit _) => PlayCue(AudioCue.EnemyDeath);

        public void PlayCue(AudioCue cue)
        {
            if (_sfxSource == null) return;
            if (!_clipByCue.TryGetValue(cue, out var entry) || entry.clip == null) return;

            float volume = entry.volume > 0f ? entry.volume : 1f;
            _sfxSource.PlayOneShot(entry.clip, volume);
        }

        private void HandleStateChanged(GameStateType previous, GameStateType next)
        {
            switch (next)
            {
                case GameStateType.Menu:     PlayMusic(_menuMusic);     break;
                case GameStateType.Playing:  PlayMusic(_gameplayMusic); break;
                case GameStateType.GameWon:  PlayCue(AudioCue.PlayerWin);  break;
                case GameStateType.GameLost: PlayCue(AudioCue.PlayerLose); break;
            }
        }

        private void PlayMusic(AudioClip clip)
        {
            if (_musicSource == null || clip == null) return;
            if (_musicSource.clip == clip && _musicSource.isPlaying) return;

            _musicSource.clip = clip;
            _musicSource.loop = true;
            _musicSource.Play();
        }
    }
}
