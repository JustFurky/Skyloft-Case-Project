namespace SkyloftGame.StateMachine
{
    /// <summary>
    /// Oyun genelinde kullanılan durum türlerini tanımlar.
    /// GameStateManager geçişleri bu enum üzerinden yönetir.
    /// </summary>
    public enum GameStateType
    {
        None    = 0,
        Menu    = 1,
        Playing = 2,
        GameWon = 3,
        GameLost = 4
    }
}
