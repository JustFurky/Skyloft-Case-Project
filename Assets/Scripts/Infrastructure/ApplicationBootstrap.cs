using UnityEngine;

namespace SkyloftGame.Infrastructure
{
    public static class ApplicationBootstrap
    {
        private const int TargetFrameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            QualitySettings.vSyncCount  = 0;
            Application.targetFrameRate = TargetFrameRate;
        }
    }
}
