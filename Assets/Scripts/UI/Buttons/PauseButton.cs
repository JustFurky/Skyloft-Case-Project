using Zenject;
using SkyloftGame.Gameplay;

namespace SkyloftGame.UI
{
    public class PauseButton : ButtonBase
    {
        private PauseController _pause;

        [Inject]
        private void Construct(PauseController pause) => _pause = pause;

        protected override void OnClick() => _pause.Pause();
    }
}
