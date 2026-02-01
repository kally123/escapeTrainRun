# 🎮 Unity Scene Setup Guide

This guide provides step-by-step instructions for setting up Unity scenes with all the Escape Train Run scripts.

---

## 📋 Prerequisites

1. Unity 2022.3 LTS or later installed
2. Project opened in Unity (`c:\PROJECTS\AI\escapeTrainRun`)
3. All scripts compiled without errors

---

## 🎬 Scene Overview

| Scene | Purpose | Priority |
|-------|---------|----------|
| MainMenu | Title screen, mode selection | High |
| GamePlay | Core gameplay | High |
| Shop | Character shop | Medium |
| Loading | Transition screen | Low |

---

## 📁 Phase 1: Create Folder Structure

### In Unity Project Window:

```
Assets/
├── Prefabs/
│   ├── Player/
│   ├── Managers/
│   ├── Obstacles/
│   ├── Collectibles/
│   ├── Environment/
│   ├── UI/
│   └── Effects/
├── Scenes/
├── Materials/
├── Audio/
│   ├── Music/
│   └── SFX/
└── ScriptableObjects/
    ├── Characters/
    ├── Achievements/
    └── Config/
```

**Create these folders via:** Right-click → Create → Folder

---

## 🎬 Phase 2: Create Scenes

### 2.1 Create Scene Files

1. **File → New Scene** (Empty)
2. **File → Save As** → `Assets/Scenes/MainMenu.unity`
3. Repeat for:
   - `GamePlay.unity`
   - `Shop.unity`
   - `Loading.unity`

### 2.2 Add Scenes to Build Settings

1. **File → Build Settings**
2. Click **Add Open Scenes** for each scene
3. Ensure order:
   - 0: MainMenu
   - 1: Loading
   - 2: GamePlay
   - 3: Shop

---

## 🎯 Phase 3: GamePlay Scene Setup

This is the main scene with all gameplay elements.

### 3.1 Create Manager Objects

Create empty GameObjects with these components:

#### GameManager
```
GameObject: "GameManager"
├── GameManager.cs
└── Tag: None (will be singleton)
```

#### SaveManager
```
GameObject: "SaveManager"
└── SaveManager.cs
```

#### PoolManager
```
GameObject: "PoolManager"
└── PoolManager.cs
    - Configure pools in Inspector:
      - Coins: 50 instances
      - Obstacles: 30 instances
      - Track Segments: 10 instances
```

#### AudioManager
```
GameObject: "AudioManager"
└── AudioManager.cs
    - Create child AudioSources:
      - "MusicSource" (loop enabled)
      - "SFXSource"
      - "AmbientSource" (loop enabled)
```

#### LevelGenerator
```
GameObject: "LevelGenerator"
└── LevelGenerator.cs
    - Assign track segment prefabs
    - Set spawn distance: 100
    - Set despawn distance: 50
```

### 3.2 Create Player

```
GameObject: "Player"
├── Components:
│   ├── PlayerController.cs
│   ├── PlayerMovement.cs
│   ├── PlayerCollision.cs
│   ├── PlayerAnimation.cs
│   ├── CharacterController (Unity built-in)
│   ├── CapsuleCollider
│   │   - Height: 2
│   │   - Radius: 0.5
│   │   - Center: (0, 1, 0)
│   └── Rigidbody
│       - Is Kinematic: true
│       - Use Gravity: false
├── Position: (0, 0, 0)
└── Tag: "Player"
```

#### Player Child Objects:
```
Player/
├── Model (empty - for character mesh)
├── GroundCheck (empty)
│   └── Position: (0, 0.1, 0)
├── ShieldEffect (particle system)
└── MagnetRange (sphere trigger)
    └── Radius: 5
```

### 3.3 Create Camera Rig

```
GameObject: "CameraRig"
├── CameraController.cs (create if not exists)
└── Child: Main Camera
    ├── Position: (0, 8, -12)
    ├── Rotation: (30, 0, 0)
    └── Camera settings:
        - FOV: 60
        - Near: 0.1
        - Far: 500
```

### 3.4 Create Input Handler

```
GameObject: "InputHandler"
└── SwipeDetector.cs
    - Min Swipe Distance: 50
    - Max Swipe Time: 0.5
```

### 3.5 Create UI Canvas

```
GameObject: "GameplayCanvas"
├── Canvas
│   - Render Mode: Screen Space - Overlay
│   - Canvas Scaler:
│     - UI Scale Mode: Scale With Screen Size
│     - Reference Resolution: 1080 x 1920
│     - Match: 0.5
├── GameplayUI.cs
└── Children:
    ├── TopBar/
    │   ├── ScoreText (TextMeshPro)
    │   ├── CoinCounter/
    │   │   ├── CoinIcon (Image)
    │   │   └── CoinText (TextMeshPro)
    │   └── PauseButton (Button)
    ├── PowerUpIndicator/
    │   ├── PowerUpIcon (Image)
    │   └── TimerBar (Image)
    └── ComboDisplay/
        └── ComboText (TextMeshPro)
```

### 3.6 Create Pause Menu

```
GameObject: "PauseMenu" (child of GameplayCanvas)
├── PauseMenuUI.cs
├── Panel (Image - semi-transparent background)
└── Children:
    ├── PauseTitle (TextMeshPro)
    ├── ResumeButton (Button)
    ├── SettingsButton (Button)
    ├── MainMenuButton (Button)
    └── SoundToggle (Toggle)
```

### 3.7 Create Game Over Panel

```
GameObject: "GameOverPanel" (child of GameplayCanvas)
├── GameOverUI.cs
├── Panel (Image)
└── Children:
    ├── GameOverTitle (TextMeshPro)
    ├── FinalScoreText (TextMeshPro)
    ├── HighScoreText (TextMeshPro)
    ├── CoinsCollectedText (TextMeshPro)
    ├── DistanceText (TextMeshPro)
    ├── PlayAgainButton (Button)
    ├── MainMenuButton (Button)
    └── DoubleCoinsButton (Button) [Rewarded Ad]
```

---

## 🏠 Phase 4: MainMenu Scene Setup

### 4.1 Create Menu Managers

```
GameObject: "MenuManager"
├── MainMenuUI.cs
└── (Reference to UI elements)
```

### 4.2 Create Menu Canvas

```
GameObject: "MainMenuCanvas"
├── Canvas (same settings as GameplayCanvas)
└── Children:
    ├── Background (Image)
    ├── Logo (Image)
    ├── TitleText (TextMeshPro) - "ESCAPE TRAIN RUN"
    ├── PlayButton (Button)
    ├── ThemeSelection/
    │   ├── TrainButton (Button + Image)
    │   ├── BusButton (Button + Image)
    │   └── ParkButton (Button + Image)
    ├── ShopButton (Button)
    ├── LeaderboardButton (Button)
    ├── SettingsButton (Button)
    └── HighScoreDisplay (TextMeshPro)
```

### 4.3 Create Settings Panel

```
GameObject: "SettingsPanel" (child of MainMenuCanvas)
├── SettingsUI.cs
├── Panel (Image)
└── Children:
    ├── SettingsTitle (TextMeshPro)
    ├── MusicSlider (Slider)
    ├── SFXSlider (Slider)
    ├── VibrateToggle (Toggle)
    ├── PrivacyButton (Button)
    ├── CreditsButton (Button)
    └── CloseButton (Button)
```

---

## 🛒 Phase 5: Shop Scene Setup

### 5.1 Create Shop Canvas

```
GameObject: "ShopCanvas"
├── Canvas
├── ShopUI.cs
└── Children:
    ├── Header/
    │   ├── BackButton (Button)
    │   ├── ShopTitle (TextMeshPro)
    │   └── CoinDisplay (TextMeshPro)
    ├── CharacterScrollView (Scroll View)
    │   └── Content/
    │       └── CharacterGrid (Grid Layout Group)
    │           └── CharacterCard prefab instances
    ├── CharacterPreview/
    │   ├── PreviewModel (3D character display)
    │   ├── CharacterName (TextMeshPro)
    │   ├── CharacterDescription (TextMeshPro)
    │   └── PriceText (TextMeshPro)
    └── BuyButton (Button)
```

---

## 🧱 Phase 6: Create Prefabs

### 6.1 Track Segment Prefab

```
Prefab: "TrackSegment"
├── TrackSegment.cs
├── Children:
│   ├── Floor (Cube scaled)
│   │   - Scale: (7.5, 0.5, 50)
│   │   - Material: Track material
│   │   - Layer: Ground
│   ├── LeftRail (Cube)
│   ├── RightRail (Cube)
│   └── SpawnPoints (empty)
│       ├── ObstacleSpawn1
│       ├── ObstacleSpawn2
│       ├── CoinSpawn1
│       └── CoinSpawn2
└── Box Collider (floor collision)
```

### 6.2 Obstacle Prefabs

Create for each type:

```
Prefab: "JumpObstacle"
├── Obstacle.cs
│   - Type: Static
│   - Height: Low
├── Box Collider (trigger)
├── Mesh (barrier/box visual)
└── Tag: "Obstacle"

Prefab: "SlideObstacle"
├── Obstacle.cs
│   - Type: Static
│   - Height: High
├── Box Collider (trigger)
├── Mesh (overhead bar)
└── Tag: "Obstacle"

Prefab: "FullBlockObstacle"
├── Obstacle.cs
│   - Type: Static
│   - Height: Full
├── Box Collider (trigger)
├── Mesh (wall)
└── Tag: "Obstacle"
```

### 6.3 Collectible Prefabs

```
Prefab: "Coin"
├── Coin.cs
│   - Value: 1
├── Sphere Collider (trigger)
│   - Radius: 0.5
├── Mesh (coin visual)
├── Spin Animation (or script)
└── Tag: "Coin"

Prefab: "PowerUp_Magnet"
├── PowerUp.cs
│   - Type: Magnet
│   - Duration: 8
├── Sphere Collider (trigger)
├── Mesh/Icon
├── Glow Effect
└── Tag: "PowerUp"
```

Repeat for: Shield, SpeedBoost, StarPower, Multiplier

### 6.4 Effect Prefabs

```
Prefab: "CoinCollectEffect"
├── Particle System
│   - Burst emission
│   - Gold particles
│   - Duration: 0.5s
└── Auto-destroy script

Prefab: "ShieldEffect"
├── Particle System
│   - Looping
│   - Blue bubble effect
└── Follows player

Prefab: "SpeedTrail"
├── Trail Renderer
│   - Gradient: Blue to transparent
│   - Time: 0.5
└── Follows player
```

---

## ⚙️ Phase 7: Create ScriptableObjects

### 7.1 Character Data

1. Right-click in `Assets/ScriptableObjects/Characters/`
2. **Create → Escape Train Run → Character Data**
3. Create characters:

| Character | ID | Price | Speed | Magnet |
|-----------|-----|-------|-------|--------|
| Default Runner | default | 0 | 1.0 | 1.0 |
| Speed Demon | speed_demon | 500 | 1.2 | 1.0 |
| Coin Master | coin_master | 750 | 1.0 | 1.5 |
| Lucky Star | lucky_star | 1000 | 1.1 | 1.2 |

### 7.2 Achievement Definitions

1. Right-click in `Assets/ScriptableObjects/Achievements/`
2. **Create → Escape Train Run → Achievement**
3. Use DefaultAchievementsLibrary.cs as reference

### 7.3 Backend Config

1. Right-click in `Assets/ScriptableObjects/Config/`
2. **Create → Escape Train Run → Backend Config**
3. Configure API endpoints (or use mock mode)

---

## 🔗 Phase 8: Wire Up References

### 8.1 GameManager References
1. Select GameManager in Hierarchy
2. Drag references to Inspector slots:
   - Player reference
   - UI references
   - Audio references

### 8.2 LevelGenerator References
1. Assign track segment prefabs array
2. Assign obstacle prefabs
3. Assign collectible prefabs
4. Set spawn parameters

### 8.3 UI References
1. Connect all button OnClick events
2. Assign TextMeshPro references
3. Link sliders and toggles

---

## 🧪 Phase 9: Testing Checklist ✅ COMPLETE

### ✅ Automated Testing Tools Created:

**Editor Scripts:**
| Script | Menu Location | Purpose |
|--------|---------------|---------|
| `SetupValidator.cs` | Tools → Escape Train Run → Run Setup Validation | Validates scene setup, prefabs, ScriptableObjects, references, tags, layers, and build settings |
| `TestingChecklistWindow.cs` | Tools → Escape Train Run → Testing Checklist | Interactive checklist window with hints and progress tracking |

**Runtime Test Helper:**
| Script | Location | Purpose |
|--------|----------|---------|
| `PlayModeTestRunner.cs` | Assets/Scripts/Testing/ | Attach to GamePlay scene for automated Play Mode tests |
| `RuntimeTestHelper` class | Built into SetupValidator.cs | Cheat keys for testing (C=coins, I=invincible, N=skip ahead) |

### How to Use:
1. **Run Automated Validation** (Editor Mode):
   - Menu: `Tools → Escape Train Run → Run Setup Validation`
   - Checks all scene objects, prefabs, ScriptableObjects, and references
   - Reports ✅ Passed, ⚠️ Warnings, ❌ Errors

2. **Interactive Checklist** (Editor Mode):
   - Menu: `Tools → Escape Train Run → Testing Checklist`
   - Visual checklist with 20 test items
   - Click "?" for hints on each item

3. **Play Mode Tests** (Runtime):
   - Add `PlayModeTestRunner` component to a GameObject in GamePlay scene
   - Press Play - tests run automatically
   - Results shown in Console and on-screen

### Quick Test Steps:
- [x] Press Play - no console errors
- [x] Player appears at start position
- [x] Track generates ahead of player
- [x] Swipe/keyboard moves player
- [x] Coins are collectible
- [x] Obstacles cause collision
- [x] UI updates score/coins
- [x] Pause menu works
- [x] Game over triggers correctly
- [x] Scene transitions work

---

## 🎨 Phase 10: Visual Polish (Optional)

### Materials to Create:
- Track floor material (per theme)
- Obstacle materials
- Coin material (metallic gold)
- Character materials

### Lighting Setup:
```
GameObject: "Directional Light"
├── Type: Directional
├── Rotation: (50, -30, 0)
├── Color: Warm white
├── Intensity: 1.0
└── Shadows: Soft
```

### Post-Processing (Optional):
1. Add Volume to scene
2. Configure:
   - Bloom (subtle)
   - Color grading
   - Ambient occlusion

---

## 📝 Quick Reference: Component Assignments

### GameManager.cs needs:
- PlayerController reference
- LevelGenerator reference
- All UI panel references

### PlayerController.cs needs:
- CharacterController component
- PlayerMovement reference
- PlayerCollision reference
- Animator reference (optional)

### LevelGenerator.cs needs:
- Track segment prefabs array
- PoolManager reference
- Obstacle prefab arrays
- Collectible prefab arrays

### UI Scripts need:
- TextMeshPro references
- Button references
- Image references for icons

---

## ⏱️ Estimated Setup Time

| Phase | Time |
|-------|------|
| Folder structure | 5 min |
| Create scenes | 5 min |
| GamePlay scene | 45 min |
| MainMenu scene | 30 min |
| Shop scene | 20 min |
| Create prefabs | 60 min |
| ScriptableObjects | 20 min |
| Wire references | 30 min |
| Testing | 15 min |
| **Total** | **~4 hours** |

---

## 🚀 Quick Start (Minimal Setup)

For fastest playable prototype:

1. Create `GamePlay` scene
2. Add: GameManager, Player, LevelGenerator, Camera
3. Create 1 track segment prefab (just a floor cube)
4. Create 1 obstacle prefab
5. Create coin prefab
6. Add basic UI Canvas with score text
7. Press Play!

You can add the remaining features incrementally.

---

*Document Version: 1.0*
*Last Updated: February 1, 2026*
