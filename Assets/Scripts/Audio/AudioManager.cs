using System.Collections.Generic;
using UnityEngine;
using Zenject;
using SkyloftGame.Enemy;
using SkyloftGame.StateMachine;
using EnemyUnit = SkyloftGame.Enemy.Enemy;

namespace SkyloftGame.Audio
{
    public class AudioManager : MonoBehaviour
    {
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

        private GameStateManager _game;

        private readonly Dictionary<AudioCue, CueClip> _clipByCue = new();

        private void Awake()
        {
            foreach (var entry in _cues)
                _clipByCue[entry.cue] = entry;
        }

        [Inject]
        private void Construct(GameStateManager game)
        {
            _game = game;

            AudioEvents.CuePlayed += PlayCue;
            EnemyRegistry.Killed  += HandleEnemyKilled;
            _game.OnStateChanged  += HandleStateChanged;
            HandleStateChanged(GameStateType.None, _game.CurrentState);
        }

        private void OnDestroy()
        {
            AudioEvents.CuePlayed -= PlayCue;
            EnemyRegistry.Killed  -= HandleEnemyKilled;
            if (_game != null) _game.OnStateChanged -= HandleStateChanged;
        }

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
