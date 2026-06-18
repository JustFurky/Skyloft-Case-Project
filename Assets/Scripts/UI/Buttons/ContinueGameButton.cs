using Zenject;

namespace SkyloftGame.UI
{
    public class ContinueGameButton : ButtonBase
    {
        private GameStateManager _game;

        [Inject]
        private void Construct(GameStateManager game) => _game = game;

        protected override void OnClick() => _game.ContinueGame();
    }
}
