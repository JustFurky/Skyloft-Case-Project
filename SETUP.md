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
| 5 | **Joystick Input** | **Joystick Pack** (FloatingJoystick) — `PlayerController` paketin `Joystick.Direction`'ını doğrudan okur (paket `JoystickPack` asmdef'i ile SkyloftGame'den referanslanır) |
| 6 | **Object Pooling** | `Pool/` (`ObjectPooler`, `GameObjectPool`, `PooledObject`, `IPoolable`, **`PoolId`** SO-anahtar) — mermi, düşman, VFX; lazy auto-register, string key yok |
| 7 | **Data Persistence** | `Data/` (`GameData`, `IDataService`, `EncryptedJsonDataService`, `DataManager`) |
| 8 | **UI** | `UI/` (`UIPanel`, `UIController`, `MainMenuView`, `HudView`, `GameWonView`, `GameLostView`) |
| 9 | **Tweening** | **PrimeTween** — `UIPanel` (fade + scale göster/gizle), `GameWonView` (kill count-up `Tween.Custom`) |
| 10 | **Particle Effects** | `VFX/PooledParticle` + `Projectile` (isabet VFX) + `Enemy` (ölüm VFX) — hepsi havuzlu |
| 11 | **Optimization** | Object pooling, `LevelData.maxAliveEnemies` kapısı, NavMesh path throttling, `MaterialPropertyBlock`, `EnemyRegistry`/`PlayerLocator` (taramasız), kill-başına diske yazmama |

### Tweening notu
Tweening için **PrimeTween** (`com.kyrylokuzyk.primetween`) kullanılır. Tüm oyun kodu
`SkyloftGame` assembly definition'ı içinde olduğundan PrimeTween, `PrimeTween.Runtime`
referansı ile çözülür (bkz. `Assets/Scripts/SkyloftGame.asmdef`). Menü geçişleri
`UIPanel.Show/Hide` içinde `Sequence` + `Tween.Alpha`/`Tween.Scale`, "Game Won" skor
sayacı `Tween.Custom` ile animasyonludur. `UIPanel.useUnscaledTime`, pause menüsü gibi
`timeScale=0` iken animasyon gerektiren paneller için ölçeksiz zaman desteği sağlar.

### Ek sistemler (case kriterleri ötesi cila ve sağlamlık)
| Sistem | Sınıf(lar) | Not |
|--------|-----------|-----|
| **Mimari sınırlar** | `SkyloftGame.asmdef` + `SkyloftGame.EditMode.Tests.asmdef` | Katmanlar derleyici düzeyinde izole; derleme süresi düşer |
| **Birim testler** | `Tests/EditMode/` (`CountdownTimerTests`, `EncryptedJsonDataServiceTests`) | Saf-C# sayaç + şifreli kayıt round-trip; Test Runner ile çalışır |
| **Ses** | `Audio/` (`AudioCue`, `AudioEvents`, `AudioManager`) + `UI/ButtonClickSound` | Olay-tabanlı ses veri yolu; AudioManager yoksa sessizce no-op |
| **Duraklatma** | `Gameplay/PauseController` + `UI/PauseView` | `timeScale=0` overlay; Playing alt sistemlerini yıkmaz |
| **Güvenli alan** | `UI/SafeAreaFitter` | Çentikli ekranlarda UI'yi `Screen.safeArea`'ya oturtur |
| **Async (UniTask)** | `EnemySpawner`, `EnemyAI`, `Pool/PooledObject` | Coroutine/`Invoke` yerine alloc-free, `CancellationToken` ile iptal-edilebilir akış |

> **UniTask notu:** `EnemySpawner` dalga akışı, `EnemyAI` path döngüsü ve `PooledObject`
> zamanlı iadesi UniTask'e taşındı. İptal `CancellationTokenSource` ile yapılır ve token
> nesne yok edilmesine de bağlıdır (`GetCancellationTokenOnDestroy`). Pool'a iadede
> ilgili token iptal edilir (EnemyAI: `StopAI`, PooledObject: `OnDisable`). `Enemy`
> hit-flash gibi disable'da Unity'nin kendiliğinden durdurduğu kısa görsel coroutine'ler
> bilinçli olarak coroutine bırakıldı (gereksiz CTS yükü olmasın — KISS).

---

## 3. Editör Kurulumu (sahne: `Assets/Scenes/Game Scene.unity`)

Kodlar hazırdır; aşağıdaki sahne/prefab bağlamaları yapılır.

### 3.1 Katmanlar & Tag'ler
`ProjectSettings/TagManager` ile eklendi:
- Tag: `Player`, `Enemy`
- Layer: `Player` (6), `Enemy` (7), `Projectile` (8)

### 3.2 Yöneticiler (boş GameObject'ler)
1. **DataManager** — `DataManager` bileşeni.
2. **ObjectPooler** — `ObjectPooler`. Havuzlar artık **PoolId** asset'leriyle anahtarlanır
   (string key yok) ve ilk kullanımda otomatik oluşur. `Prewarm` listesine ilk spawn
   hitch'i istemediğin PoolId'leri ekle (örn. `Enemy`, `Projectile`). Gerisi lazy.
3. **GameSystems** — `LevelManager` (+ `LevelDatabase` ata), `EnemySpawner` (+ **Wave
   Settings** ata), `ScoreService`, `PauseController`.
4. **GameStateManager** — `LevelManager`/`EnemySpawner`/`ScoreService` alanlarını (3) ile bağla.
5. **AudioManager** (opsiyonel) — `AudioManager`; `Music` + `Sfx` için iki `AudioSource`,
   `Menu/Gameplay Music` klipleri ve `SFX Eşlemeleri` listesinde her `AudioCue` için klip.
   Atanmayan klipler sessizce yok sayılır.

### 3.3 Seviye verisi
- `Assets/Create/SkyloftGame/Level Data` ile **3 adet** `LevelData` oluştur
  (`Level 1/2/3`). Kademeli zorluk için: `durationSeconds` aynı (180), `waves`
  sayısını/`count`'unu artır, `spawnInterval`'i düşür; her dalganın `Enemy` alanına
  bir **Enemy PoolId** ata (gerekirse farklı varyant için farklı PoolId).
- `Assets/Create/SkyloftGame/Level Database` → bu üç asset'i **index sırasıyla** ekle.

### 3.3b Veri asset'leri (ScriptableObject)
Tüm tuning verisi `Assets/Create/SkyloftGame/...` altından üretilen SO asset'lerinde tutulur;
prefab/sahne yalnızca asset'i referanslar (DRY + paylaşılabilir + tasarımcı dostu):
- **Pool Id** — her havuzlanabilir tür için bir asset (prefab + initialSize/maxSize).
  Oluştur: `Enemy`, `Projectile`, `HitVfx`, `DeathVfx`, `SpawnVfx`. Kod string yerine bu
  asset'leri referanslar; yeni tür eklemek = 1 PoolId asset + sürükle (ObjectPooler'a elle
  giriş yok). `maxSize = 0` = sınırsız (önerilen; havuza dönen nesneler hep yeniden kullanılır).
- **Player Data** — oyuncu can/hareket/arena. `PlayerController` **ve** `PlayerHealth`'e
  **aynı** asset atanır.
- **Weapon Data** — atış hızı, mermi hızı, `Projectile` = **Projectile PoolId** → `PlayerShooter`.
- **Player Animation Data** / **Enemy Animation Data** — animator state adları + crossFade →
  `PlayerAnimator` / `EnemyAnimator`.
- **Projectile Data** — hasar, ömür, hitLayers (`Enemy`), `Hit Vfx` = **HitVfx PoolId**, offset → `Projectile`.
- **Enemy Data** — düşman istatistikleri → `Enemy`.
- **Wave Settings** — dalga-arası geri sayım süresi (3 sn), ilk dalga öncesi sayım,
  blink süresi → `EnemySpawner` **ve** `HudView` (aynı asset).

> Bu bileşenler asset'i atanmadan çalışırsa NRE atmaz; Console'da `LogError` ile uyarır ve
> güvenli şekilde devre dışı kalır (PlayerHealth varsayılan 100 can kullanır).

### 3.4 Player prefab'ı
- `CharacterController` + `PlayerController` (`Joystick` = sahnedeki **FloatingJoystick**,
  `Data` = **Player Data**), `PlayerHealth` (`Data` = **aynı Player Data**),
  `PlayerShooter` (`Muzzle` transform'u, `Weapon` = **Weapon Data**),
  `PlayerAnimator` (`Anim Data` = **Player Animation Data**), `Animator`.
- Tag `Player`, Layer `Player`. `PlayerController.OnEnable` kendini `PlayerLocator`'a yazar.
- **Can barı (prefab'a ait):** Player altına `World Space` bir Canvas + içine `Image`
  (Image Type = **Filled**, Horizontal). Canvas'a `PlayerHealthBar` ekle → `Fill` = bu Image,
  `Health` boş bırakılabilir (parent'tan otomatik bulur). Bar, `PlayerHealth.OnHealthChanged`
  olayını dinler; HUD'a referans gerektirmez (SRP). Can biterse oyun otomatik kaybedilir.

### 3.5 Enemy prefab'ı (havuzlanır)
- `NavMeshAgent` + `EnemyAI` + `Enemy` (`EnemyData` ata; `Spawn Vfx` = **SpawnVfx PoolId**,
  `Death Vfx` = **DeathVfx PoolId**) + `EnemyAnimator` (`Anim Data` = **Enemy Animation Data**)
  + `Animator`, gövde `Renderer` (hit-flash), `Collider` (trigger değil), Layer `Enemy`.
  `PooledObject` otomatik eklenir.
- Bu prefab'ı bir **Enemy PoolId** asset'ine ata; dalgalar o PoolId'yi referanslar.
- `Assets/Create/SkyloftGame/Enemy Data` ile varyantlar (Basic/Elite/Boss).

### 3.6 Projectile prefab'ı (havuzlanır)
- `Rigidbody` (kinematic, otomatik ayarlanır) + trigger `Collider` + `Projectile`
  (`Data` = **Projectile Data**), Layer `Projectile`. Hasar/ömür/hitLayers/Hit VFX artık
  Projectile Data asset'inde (hitLayers = `Enemy`, `Hit Vfx` = **HitVfx PoolId**).
- Bu prefab'ı bir **Projectile PoolId** asset'ine ata; `WeaponData.Projectile` onu referanslar.

### 3.7 VFX prefab'ları (havuzlanır)
- `HitVfx` / `DeathVfx` / `SpawnVfx`: `ParticleSystem` + `PooledParticle` (Looping kapalı).
  Her birini bir **PoolId** asset'ine ata. `SpawnVfx`, her düşman doğduğunda `Enemy.OnSpawn`
  içinde oynatılır. (PooledParticle unutulursa pool güvenlik ağı runtime'da otomatik ekler.)

### 3.8 UI (Canvas)
- **Güvenli alan:** Canvas'ın hemen altına `SafeArea` adlı bir RectTransform (stretch) +
  `SafeAreaFitter` bileşeni ekle; tüm panelleri bunun çocuğu yap.
- `UIController` → 4 panel referansı (Menu/HUD/GameWon/GameLost).
- Her panel: `CanvasGroup` + ilgili `*View` bileşeni. Buton/Text alanlarını bağla.
- **Joystick (Joystick Pack — FloatingJoystick):**
  - Canvas altına paketin **Floating Joystick** prefab'ını koy.
  - `PlayerController.Joystick` alanına bu **FloatingJoystick**'i sürükle.
  - `PlayerController` paketin `Joystick.Direction`'ını **doğrudan** okur (adapter/köprü yok).
    Paket `Assets/Joystick Pack/Scripts/JoystickPack.asmdef` ile assembly haline geldi ve
    `SkyloftGame.asmdef` bunu referanslıyor.
  - Editörde joystick fare ile sürüklenerek test edilebilir (klavye girdisi kaldırıldı).
- `HudView`: **Timer Label + Kills Label + Pause Button + Wave Countdown Label** (TMP) +
  **Wave Settings** asset'i. Wave Countdown Label dalga arası her saniye sönüp güncel değerle
  yanar. Can barı artık Player prefab'ında.
- `MainMenuView`: **Start** (kalınan seviyeden devam) + **Start New Game** (ilerlemeyi
  sıfırlayıp 1'den) butonlarını ilgili alanlara bağla.
- **PauseView** (5. panel, UIController'a bağlanmaz): `CanvasGroup` + `PauseView`
  (Resume/Menu butonları). UIPanel'de **Use Unscaled Time** işaretli olmalı (timeScale=0
  iken animasyon için). Pasif başlatılabilir; PauseController olayıyla açılır.
- **Tık sesi (opsiyonel):** Ses istenen butonlara `ButtonClickSound` ekle.

### 3.9 NavMesh
- Zemin için **Navigation** penceresinden NavMesh bake et.
- Hem düşman hareketi hem **oyuncunun oyun alanı sınırı** bunu kullanır: `PlayerController`,
  oyuncuyu doğrudan NavMesh'in üstünde tutar (`SamplePosition` + `FindClosestEdge`).
  Dikdörtgen değil **gerçek yürünebilir şekle** uyar — L, çıkıntı, delik, ada fark etmez.
  Yeni harita yaparken merkez/yarıçap ayarlamak gerekmez — sadece NavMesh bake et.
  `PlayerData.edgePadding` oyuncunun kenara/köşeye ne kadar yaklaşabileceğini belirler
  (0 = ta kenara kadar).

---

## 4. Oyun Akışı

```
Menu ──Start/Continue()──▶ Playing ──timer biter──▶ GameWon ──NextLevel()──▶ Playing(n+1)
       └─StartNewGame()──▶     │  ▲                      └──Replay()──▶ Playing
                               │  └──Resume()── (Pause: timeScale=0 overlay)
                               └──can biter──▶ GameLost ──Retry()/Menu()
```
- **Devam/Yeni oyun:** Menüde **Start**, kalıcı en yüksek açılan seviyeyi yükler
  (`ContinueGame`); **Start New Game** ilerlemeyi sıfırlayıp 1'den başlar (`StartNewGame`).
- **Dalgalar (sıralı):** `EnemySpawner` dalgaları sırayla yürütür — bir dalga **temizlenince**
  (tüm düşmanları ölünce) sonraki dalga başlamadan önce `WaveSettings.betweenWaveCountdown`
  saniye geri sayar. HUD'daki geri sayım metni her saniye sönüp güncel değerle yanar
  (`WaveCountdownTick`/`WaveStarted` olayları). Tüm dalgalar temizlenince seviye kazanılır.
- **Duraklatma:** HUD'daki pause butonu `PauseController.Pause()` → `Time.timeScale=0`.
  Playing alt sistemleri yıkılmaz; herhangi bir durum geçişinde timeScale güvenle 1'e döner.
- **Kalıcılık:** `AddEnemyKill()` belleği artırır; diske yazma seviye sonunda (`Save`/
  `UnlockLevel`) ve `OnApplicationQuit/Pause`'da yapılır (büyük dalgalarda yüzlerce
  PlayerPrefs yazımını önler). Veri AES-256 (kayıt başına rastgele IV) ile cihaza özgü
  anahtarla şifrelenir.

---

## 5. Testler
`Window > General > Test Runner > EditMode > Run All`. Saf-C# `CountdownTimer` ve şifreli
kayıt servisi round-trip testleri burada koşar (`Assets/Tests/EditMode/`).

## 6. APK Çıktısı
`File > Build Settings > Android` → `Game Scene`'i ekle → `Build`.
(Submission e-postası: `muhammet.emin.arday@boombit.com`.)
```
