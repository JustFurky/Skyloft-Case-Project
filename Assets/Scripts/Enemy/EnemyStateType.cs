using UnityEngine;

namespace SkyloftGame.Enemy
{
    /// <summary>
    /// Düşmanın anlık davranış durumları.
    /// EnemyAI bu enum üzerinden hangi eylemin çalışacağını belirler.
    /// </summary>
    public enum EnemyStateType
    {
        Chase    = 0,   // Oyuncuyu NavMesh üzerinde takip ediyor
        Attack   = 1,   // Saldırı menzilinde, saldırıyor
        Dead     = 2    // Öldü, pool'a dönüyor
    }
}
