namespace SkyloftGame.Level
{
    public interface ILevelService
    {
        LevelData Current      { get; }
        int       CurrentIndex { get; }
        int       Count        { get; }
        bool      HasNext      { get; }

        int HighestUnlockedIndex { get; }

        void Load(int index);

        bool TryAdvance();

        void MarkCurrentCompleted();
    }
}
