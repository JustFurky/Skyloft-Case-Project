using Zenject;
using SkyloftGame.Audio;
using SkyloftGame.Data;
using SkyloftGame.Gameplay;
using SkyloftGame.Level;

namespace SkyloftGame.Infrastructure
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IDataService>().To<EncryptedJsonDataService>().AsSingle();
            Container.Bind<ICountdownTimer>().To<CountdownTimer>().AsSingle();

            Container.Bind<DataManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ILevelService>().To<LevelManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<IEnemySpawner>().To<EnemySpawner>().FromComponentInHierarchy().AsSingle();
            Container.Bind<IScoreService>().To<ScoreService>().FromComponentInHierarchy().AsSingle();
            Container.Bind<AudioManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PauseController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<GameStateManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}
