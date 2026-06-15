using UnityEngine;

namespace SkyloftGame.Enemy
{
    /// <summary>
    /// Düşman türüne ait tüm parametreleri tutan ScriptableObject.
    /// Her düşman varyantı (Basic, Elite, Boss) için ayrı bir asset oluştur:
    ///   Assets/Create/SkyloftGame/Enemy Data
    /// </summary>
    [CreateAssetMenu(menuName = "SkyloftGame/Enemy Data", fileName = "EnemyData_New")]
    public class EnemyData : ScriptableObject
    {
        // ------------------------------------------------------------------ //
        //  Kimlik
        // ------------------------------------------------------------------ //

        [Header("Kimlik")]
        [Tooltip("Bu düşman türünün benzersiz adı.")]
        public string enemyName = "Enemy";

        // ------------------------------------------------------------------ //
        //  Sağlık
        // ------------------------------------------------------------------ //

        [Header("Sağlık")]
        [Min(1f)] public float maxHp        = 50f;
        [Min(0f)] public float armor        = 0f;   // hasar azaltma (0-1 arası önerilir)

        // ------------------------------------------------------------------ //
        //  Hareket
        // ------------------------------------------------------------------ //

        [Header("Hareket (NavMesh)")]
        [Min(0.1f)] public float moveSpeed          = 3.5f;
        [Min(0.1f)] public float angularSpeed       = 120f;
        [Min(0f)]   public float acceleration       = 8f;
        [Min(0f)]   public float stoppingDistance   = 1.2f;

        [Tooltip("Hedef pozisyonu kaç saniyede bir yeniden hesapla. " +
                 "Düşük değer = daha hassas ama pahalı. Önerilen: 0.1-0.3")]
        [Range(0.05f, 1f)]
        public float pathUpdateInterval = 0.2f;

        // ------------------------------------------------------------------ //
        //  Saldırı
        // ------------------------------------------------------------------ //

        [Header("Saldırı")]
        [Min(0f)] public float attackDamage    = 10f;
        [Min(0f)] public float attackRange     = 1.5f;
        [Min(0.1f)] public float attackCooldown = 1f;

        // ------------------------------------------------------------------ //
        //  Ödül
        // ------------------------------------------------------------------ //

        [Header("Ödül")]
        [Min(0)]  public int scoreValue       = 10;
        [Min(0f)] public float dropChance     = 0.2f;
    }
}
