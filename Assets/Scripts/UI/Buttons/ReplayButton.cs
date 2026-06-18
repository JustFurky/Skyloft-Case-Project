using Zenject;

namespace SkyloftGame.UI
{
    public class ReplayButton : ButtonBase
    {
        private GameStateManager _game;

        [Inject]
        private void Construct(GameStateManager game) => _game = game;

        protected override void OnClick() => _game.ReplayLevel();
    }
}
