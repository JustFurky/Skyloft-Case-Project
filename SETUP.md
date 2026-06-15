# Skyloft — Unity Developer Case Study

Top-down survival arena: bir joystick ile hareket eden kahraman, dalgalar halinde
gelen düşmanları yener. 3 dakikalık geri sayım dolunca **Game Won** menüsü öldürülen
düşman sayısını gösterir ve oyuncu bir sonraki, daha zorlu seviyeye geçer. İlerleme
(toplam öldürme + açılan seviye) oyun kapanıp açılsa da kalıcıdır.

> Unity **6000.2.6f2** (URP) · Input System · NavMesh AI

---

## 1. Mimari Genel Bakış

Tasarım; **SOLID + KISS + DRY + YAGNI** ilkelerini gözetir. Üç temel altyapı
(durum makinesi, object pooling, kalıcı veri) arayüzler üzerinden gevşek bağlıdır;
oyun akışı tek bir kompozisyon kökünde (`GameStateManager`) toplanır.

```
GameStateManager (composition root, [DefaultExecutionOrder(-100)])
│   durum makinesini sürer, alt sistemleri arayüz tipiyle state'lere sunar (DIP)
│
├── StateMachine  : IState + GameStateMachine  →  Menu / Playing / GameWon / GameLost
├── ILevelService : LevelManager  ← LevelDatabase[ LevelData(waves...) ]
├── IEnemySpawner : EnemySpawner  → ObjectPooler'dan dalga üretir
├── IScoreService : ScoreService  → EnemyRegistry.Killed dinler (tur + kalıcı skor)
└── ICountdownTimer: CountdownTimer (saf C#, PlayingState her kare Tick'ler)

DataManager (singleton, [-200])     → IDataService → EncryptedJsonDataService (AES + PlayerPrefs)
ObjectPooler (singleton, [-150])    → GameObjectPool (Unity ObjectPool sarmalayıcı)
PlayerLocator / EnemyRegistry       → taramasız merkezi erişim (optimizasyon)
```

UI ve Player; somut sınıflara değil `GameStateManager.OnStateChanged` olayına abone
olarak bağlanır — yeni bir ekran eklemek mevcut kodu değiştirmez (OCP).

---

## 2. Değerlendirme Kriteri → Kod Eşlemesi

| # | Kriter | Karşılayan sınıf(lar) |
|---|--------|-----------------------|
| 1 | **State Machine** | `StateMachine/` (`IState`, `GameStateMachine`, `GameStateType`) + `States/` + `GameStateManager` |
| 2 | **Level System** | `Level/LevelData`, `LevelDatabase`, `LevelManager` (veri-tabanlı, kademeli zorluk) |
| 3 | **Animator** | `Player/PlayerController` (Speed), `Enemy/EnemyAnimator` (Speed/Attack/Die köprüsü) |
| 4 | **NavMesh / AI** | `Enemy/EnemyAI` (NavMeshAgent, Chase/Attack FSM, path-throttle optimizasyonu) |
| 5 | **Joystick Input** | `Player/IMoveInput`, `MovementInputSource`, `VirtualJoystick`, `KeyboardInputSource` |
| 6 | **Object Pooling** | `Pool/` (`ObjectPooler`, `GameObjectPool`, `PooledObject`, `IPoolable`) — mermi, düşman, VFX |
| 7 | **Data Persistence** | `Data/` (`GameData`, `IDataService`, `EncryptedJsonDataService`, `DataManager`) |
| 8 | **UI** | `UI/` (`UIPanel`, `UIController`, `MainMenuView`, `HudView`, `GameWonView`, `GameLostView`) |
| 9 | **Tweening** | **PrimeTween** — `UIPanel` (fade + scale göster/gizle), `GameWonView` (kill count-up `Tween.Custom`) |
| 10 | **Particle Effects** | `VFX/PooledParticle` + `Projectile` (isabet VFX) + `Enemy` (ölüm VFX) — hepsi havuzlu |
| 11 | **Optimization** | Object pooling, `LevelData.maxAliveEnemies` kapısı, NavMesh path throttling, `MaterialPropertyBlock`, `EnemyRegistry`/`PlayerLocator` (taramasız), kill-başına diske yazmama |

### Tweening notu
Tweening için **PrimeTween** (`com.kyrylokuzyk.primetween`) kullanılır — paket
projeye eklidir ve `autoReferenced` olduğundan `using PrimeTween;` Assembly-CSharp'tan
doğrudan çözülür. Menü geçişleri `UIPanel.Show/Hide` içinde `Sequence` + `Tween.Alpha`/
`Tween.Scale`, "Game Won" skor sayacı `Tween.Custom` ile animasyonludur.

---

## 3. Editör Kurulumu (sahne: `Assets/Scenes/Game Scene.unity`)

Kodlar hazırdır; aşağıdaki sahne/prefab bağlamaları yapılır.

### 3.1 Katmanlar & Tag'ler
`ProjectSettings/TagManager` ile eklendi:
- Tag: `Player`, `Enemy`
- Layer: `Player` (6), `Enemy` (7), `Projectile` (8)

### 3.2 Yöneticiler (boş GameObject'ler)
1. **DataManager** — `DataManager` bileşeni.
2. **ObjectPooler** — `ObjectPooler`; `Configs` listesine pool'lar:
   `Enemy` (düşman prefab'ı), `Projectile`, `HitVfx`, `DeathVfx`.
3. **GameSystems** — `LevelManager` (+ `LevelDatabase` ata), `EnemySpawner`, `ScoreService`.
4. **GameStateManager** — `LevelManager`/`EnemySpawner`/`ScoreService` alanlarını (3) ile bağla.

### 3.3 Seviye verisi
- `Assets/Create/SkyloftGame/Level Data` ile **3 adet** `LevelData` oluştur
  (`Level 1/2/3`). Kademeli zorluk için: `durationSeconds` aynı (180), `waves`
  sayısını/`count`'unu artır, `spawnInterval`'i düşür, gerekirse daha güçlü
  düşman pool anahtarı kullan.
- `Assets/Create/SkyloftGame/Level Database` → bu üç asset'i **index sırasıyla** ekle.

### 3.4 Player prefab'ı
- `CharacterController` + `PlayerController` (`MovementSource` = sahnedeki VirtualJoystick),
  `PlayerHealth`, `PlayerShooter` (`Muzzle` transform'u ata), `Animator`.
- Tag `Player`, Layer `Player`. `PlayerController.OnEnable` kendini `PlayerLocator`'a yazar.
- **Can barı (prefab'a ait):** Player altına `World Space` bir Canvas + içine `Image`
  (Image Type = **Filled**, Horizontal). Canvas'a `PlayerHealthBar` ekle → `Fill` = bu Image,
  `Health` boş bırakılabilir (parent'tan otomatik bulur). Bar, `PlayerHealth.OnHealthChanged`
  olayını dinler; HUD'a referans gerektirmez (SRP). Can biterse oyun otomatik kaybedilir.

### 3.5 Enemy prefab'ı (havuzlanır)
- `NavMeshAgent` + `EnemyAI` + `Enemy` (`EnemyData` ScriptableObject ata) +
  `EnemyAnimator` + `Animator`, gövde `Renderer` (hit-flash), `Collider` (trigger değil),
  Layer `Enemy`. `PooledObject` otomatik eklenir.
- `Assets/Create/SkyloftGame/Enemy Data` ile varyantlar (Basic/Elite/Boss).

### 3.6 Projectile prefab'ı (havuzlanır)
- `Rigidbody` (kinematic, otomatik ayarlanır) + trigger `Collider` + `Projectile`,
  Layer `Projectile`, `HitLayers` = `Enemy`. `HitVfxKey` = `HitVfx`.

### 3.7 VFX prefab'ları (havuzlanır)
- `HitVfx` / `DeathVfx`: `ParticleSystem` + `PooledParticle` (Looping kapalı).

### 3.8 UI (Canvas)
- `UIController` → 4 panel referansı.
- Her panel: `CanvasGroup` + ilgili `*View` bileşeni. Buton/Text alanlarını bağla.
- `VirtualJoystick`: Canvas altında background + handle RectTransform'ları.
- `HudView` yalnızca **Timer Label + Kills Label** içerir (can barı artık Player prefab'ında).

### 3.9 NavMesh
- Zemin için **Navigation** penceresinden NavMesh bake et (düşmanlar bunu kullanır).

---

## 4. Oyun Akışı

```
Menu ──StartNewGame()──▶ Playing ──timer biter──▶ GameWon ──NextLevel()──▶ Playing(n+1)
                            │                          └──Replay()──▶ Playing
                            └──can biter──▶ GameLost ──Retry()/Menu()
```
- **Kalıcılık:** `AddEnemyKill()` belleği artırır; diske yazma seviye sonunda (`Save`/
  `UnlockLevel`) ve `OnApplicationQuit/Pause`'da yapılır (büyük dalgalarda yüzlerce
  PlayerPrefs yazımını önler). Veri AES-256 ile cihaza özgü anahtarla şifrelenir.

---

## 5. APK Çıktısı
`File > Build Settings > Android` → `Game Scene`'i ekle → `Build`.
(Submission e-postası: `muhammet.emin.arday@boombit.com`.)
```
