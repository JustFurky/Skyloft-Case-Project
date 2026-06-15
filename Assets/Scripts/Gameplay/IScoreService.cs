using System;

namespace SkyloftGame.Gameplay
{
    /// <summary>
    /// Tur içi (run) ve kalıcı (total) öldürme sayılarını yöneten servis sözleşmesi.
    /// "Game Won" menüsü RunKills'i, ilerleme ekranları TotalKills'i kullanır.
    /// </summary>
    public interface IScoreService
    {
        /// <summary>Mevcut turda öldürülen düşman sayısı.</summary>
        int RunKills { get; }

        /// <summary>Tüm zamanlardaki (kalıcı) toplam öldürme.</summary>
        int TotalKills { get; }

        event Action<int> OnRunKillsChanged;

        /// <summary>Yeni tur başında tur sayacını sıfırlar.</summary>
        void ResetRun();
    }
}
