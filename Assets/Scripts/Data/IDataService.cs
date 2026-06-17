namespace SkyloftGame.Data
{
    public interface IDataService
    {
        void     Save(GameData data);
        GameData Load();
        void     Delete();
    }
}
