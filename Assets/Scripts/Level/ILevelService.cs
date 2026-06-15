namespace SkyloftGame.Level
{
    /// <summary>
    /// Seviye yükleme, ilerleme ve kalıcı ilerleme kaydı sözleşmesi.
    /// State'ler ve UI bu arayüze bağımlıdır; somut LevelManager'a değil (DIP).
    /// </summary>
    public interface ILevelService
    {
        LevelData Current      { get; }
        int       CurrentIndex { get; }
        int       Count        { get; }
        bool      HasNext      { get; }

        /// <summary>Kalıcı olarak açılmış en yüksek seviye index'i.</summary>
        int HighestUnlockedIndex { get; }

        /// <summary>Belirtilen index'teki seviyeyi aktif hale getirir.</summary>
        void Load(int index);

        /// <summary>Sonraki seviyeye geçer; sonraki yoksa false döner.</summary>
        bool TryAdvance();

        /// <summary>Mevcut seviyenin tamamlandığını işaretler ve ilerlemeyi kalıcı kaydeder.</summary>
        void MarkCurrentCompleted();
    }
}
