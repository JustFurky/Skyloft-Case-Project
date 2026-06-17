using System;
using UnityEngine;

namespace SkyloftGame.Audio
{
    public static class AudioEvents
    {
        public static event Action<AudioCue> CuePlayed;

        public static void Play(AudioCue cue) => CuePlayed?.Invoke(cue);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => CuePlayed = null;
    }
}
