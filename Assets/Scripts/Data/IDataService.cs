namespace SkyloftGame.Data
{
    /// <summary>
    /// Kalıcı oyun verilerini okuma / yazma sözleşmesi.
    /// Somut implementasyonlar (JSON, PlayerPrefs, bulut vb.) bu arayüzü uygular.
    /// Test ortamlarında kolayca mock edilebilir.
    /// </summary>
    public interface IDataService
    {
        void     Save(GameData data);
        GameData Load();
        void     Delete();
    }
}
