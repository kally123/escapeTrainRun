# 🚂 Escape Train Run - Kids Game Implementation Plan

## Game Overview

**Escape Train Run** is an endless runner game for kids inspired by Subway Surfers. Players can choose to play in three different environments:
1. 🚂 **Train Mode** - Running on train compartments
2. 🚌 **Bus Mode** - Jumping between buses in traffic
3. 🏃 **Ground Mode** - Running through parks and playgrounds

### Target Platforms
- **Primary**: Mobile (iOS & Android) + Windows Desktop
- **Engine**: Unity 3D (cross-platform support)
- **Age Group**: Kids 6-12 years

---

## 📋 Phase 1: Project Setup & Core Architecture

### 1.1 Technology Stack

| Component | Technology | Purpose |
|-----------|------------|---------|
| Game Engine | Unity 2022 LTS | Cross-platform game development |
| Language | C# | Game scripting |
| 3D Modeling | Blender (export to Unity) | Characters, environments |
| Audio | FMOD / Unity Audio | Sound effects & music |
| Analytics | Unity Analytics | Player behavior tracking |
| Backend | Azure Functions + Cosmos DB | Leaderboards, cloud saves |
| Ads (optional) | Unity Ads | Monetization |

### 1.2 Project Structure
```
EscapeTrainRun/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/                    # Core game systems
│   │   │   ├── GameManager.cs
│   │   │   ├── EventManager.cs
│   │   │   ├── AudioManager.cs
│   │   │   ├── SaveManager.cs
│   │   │   └── PoolManager.cs
│   │   ├── Player/                  # Player mechanics
│   │   │   ├── PlayerController.cs
│   │   │   ├── PlayerMovement.cs
│   │   │   ├── PlayerCollision.cs
│   │   │   ├── PlayerAnimation.cs
│   │   │   └── SwipeDetector.cs
│   │   ├── Environment/             # Level generation
│   │   │   ├── LevelGenerator.cs
│   │   │   ├── TrackSegment.cs
│   │   │   ├── ObstacleSpawner.cs
│   │   │   ├── CollectibleSpawner.cs
│   │   │   └── EnvironmentTheme.cs
│   │   ├── Obstacles/               # Obstacle types
│   │   │   ├── BaseObstacle.cs
│   │   │   ├── JumpObstacle.cs
│   │   │   ├── SlideObstacle.cs
│   │   │   └── MovingObstacle.cs
│   │   ├── Collectibles/            # Power-ups & coins
│   │   │   ├── Coin.cs
│   │   │   ├── PowerUp.cs
│   │   │   ├── Magnet.cs
│   │   │   ├── Shield.cs
│   │   │   ├── SpeedBoost.cs
│   │   │   └── Multiplier.cs
│   │   ├── Characters/              # Character system
│   │   │   ├── CharacterData.cs
│   │   │   ├── CharacterUnlock.cs
│   │   │   └── CharacterShop.cs
│   │   ├── UI/                      # User interface
│   │   │   ├── MainMenuUI.cs
│   │   │   ├── GameplayUI.cs
│   │   │   ├── PauseMenuUI.cs
│   │   │   ├── GameOverUI.cs
│   │   │   ├── ShopUI.cs
│   │   │   └── SettingsUI.cs
│   │   ├── Themes/                  # Environment themes
│   │   │   ├── TrainTheme.cs
│   │   │   ├── BusTheme.cs
│   │   │   └── GroundTheme.cs
│   │   ├── Services/                # Backend services
│   │   │   ├── LeaderboardService.cs
│   │   │   ├── CloudSaveService.cs
│   │   │   └── AchievementService.cs
│   │   └── Utils/                   # Utilities
│   │       ├── Constants.cs
│   │       ├── Extensions.cs
│   │       └── MathHelpers.cs
│   ├── Prefabs/
│   │   ├── Player/
│   │   ├── Obstacles/
│   │   ├── Collectibles/
│   │   ├── Environment/
│   │   └── UI/
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── GamePlay.unity
│   │   ├── Shop.unity
│   │   └── Loading.unity
│   ├── Art/
│   │   ├── Characters/
│   │   ├── Environments/
│   │   ├── UI/
│   │   └── Effects/
│   ├── Audio/
│   │   ├── Music/
│   │   ├── SFX/
│   │   └── Ambience/
│   ├── Animations/
│   ├── Materials/
│   └── Shaders/
├── Backend/                         # Cloud services
│   ├── Functions/
│   │   ├── LeaderboardFunction/
│   │   ├── SaveGameFunction/
│   │   └── AchievementsFunction/
│   └── Shared/
├── Docs/
│   ├── GameDesign.md
│   ├── TechnicalSpec.md
│   └── ArtGuidelines.md
└── Tests/
    ├── EditMode/
    └── PlayMode/
```

---

## 📋 Phase 2: Core Game Mechanics

### 2.1 Player Movement System

#### Swipe Controls (Mobile & Windows Touch)
| Action | Gesture | Result |
|--------|---------|--------|
| Move Left | Swipe Left | Lane change left |
| Move Right | Swipe Right | Lane change right |
| Jump | Swipe Up | Jump over obstacles |
| Slide | Swipe Down | Slide under obstacles |

#### Keyboard Controls (Windows)
| Key | Action |
|-----|--------|
| A / ← | Move Left |
| D / → | Move Right |
| W / ↑ / Space | Jump |
| S / ↓ | Slide |
| ESC | Pause |

### 2.2 Lane System
```
Lane Layout (Top View):
    ┌─────┬─────┬─────┐
    │  1  │  2  │  3  │
    │Left │Center│Right│
    └─────┴─────┴─────┘
         ↑
      Player
```

### 2.3 Movement Specifications
- **Lane Width**: 2.5 units
- **Lane Change Speed**: 10 units/second
- **Jump Height**: 2.5 units
- **Jump Duration**: 0.5 seconds
- **Slide Duration**: 0.8 seconds
- **Base Run Speed**: 15 units/second (increases over time)
- **Max Speed**: 35 units/second

---

## 📋 Phase 3: Environment Themes

### 3.1 🚂 Train Mode

#### Visual Elements
- Train compartments (passenger, cargo, engine)
- Railway tracks
- Train stations (occasional)
- Bridges over rivers
- Tunnels

#### Obstacles
| Obstacle | Height | Action Required |
|----------|--------|-----------------|
| Luggage Stack | Low | Jump |
| Hanging Bars | Medium | Slide |
| Closed Doors | Full | Change Lane |
| Moving Carts | Variable | Jump or Slide |
| Gift Boxes | Low | Jump |

#### Collectibles
- Golden Tickets (coins)
- Lunch Boxes (power-ups)
- Travel Stamps (bonus points)

### 3.2 🚌 Bus Mode

#### Visual Elements
- City buses
- Double-decker buses
- School buses
- Bus stops
- Traffic lights

#### Obstacles
| Obstacle | Height | Action Required |
|----------|--------|-----------------|
| Seats | Low | Jump |
| Standing Rails | Medium | Slide |
| Bus Doors | Full | Change Lane |
| Backpack Piles | Variable | Jump |
| Sports Gear | Low | Jump or Lane |

#### Collectibles
- Bus Tokens (coins)
- Backpacks (power-ups)
- Bus Passes (bonus points)

### 3.3 🏃 Ground Mode (Park/Playground)

#### Visual Elements
- Park paths
- Playground equipment
- Trees and bushes
- Benches
- Fountains

#### Obstacles
| Obstacle | Height | Action Required |
|----------|--------|-----------------|
| Park Benches | Low | Jump |
| Tree Branches | Medium | Slide |
| Playground Fence | Full | Change Lane |
| Sprinklers | Variable | Avoid |
| Dogs | Moving | Jump |

#### Collectibles
- Stars (coins)
- Treasure Chests (power-ups)
- Golden Leaves (bonus points)

---

## 📋 Phase 4: Power-Up System

### 4.1 Power-Up Types

| Power-Up | Duration | Effect | Visual |
|----------|----------|--------|--------|
| 🧲 Magnet | 10 sec | Attracts nearby coins | Blue glow around player |
| 🛡️ Shield | 1 hit | Protects from one crash | Bubble around player |
| ⚡ Speed Boost | 5 sec | 2x speed, invincible | Trail effect |
| 🌟 Star Power | 8 sec | Fly above obstacles | Golden wings |
| ×2 Multiplier | 15 sec | Double score | Score text glows |
| 🎁 Mystery Box | Instant | Random reward | Sparkle explosion |

### 4.2 Power-Up Spawn Rules
```csharp
public class PowerUpSpawnConfig
{
    public float BaseSpawnChance = 0.05f;      // 5% per segment
    public float MinSpawnDistance = 200f;       // Units between spawns
    public float SpawnChanceIncrease = 0.01f;   // Increases over time
    public int MaxActivePowerUps = 1;           // Only one active at a time
}
```

---

## 📋 Phase 5: Character System

### 5.1 Unlockable Characters

#### Starting Character
- **Timmy** - Adventurous boy with backpack

#### Unlockable Characters
| Character | Cost (Coins) | Special Ability |
|-----------|--------------|-----------------|
| Luna | 500 | +10% coin magnet range |
| Max | 1000 | +5% base speed |
| Robo-Kid | 5000 | Slower speed increase |
| Super Sara | 10000 | Double jump ability |
| Princess Penny | 15000 | Extra coins collected |
| Dino Dan | 20000 | Stomping jump effect |
| Ninja Nick | 25000 | Longer slide duration |

### 5.2 Character Customization
- **Outfits**: Unlockable through gameplay
- **Accessories**: Hats, glasses, backpacks
- **Effects**: Trail colors, jump effects

---

## 📋 Phase 6: User Interface Design

### 6.1 Main Menu Screen
```
┌─────────────────────────────────────┐
│         🚂 ESCAPE TRAIN RUN 🚂      │
│                                     │
│    ┌─────────────────────────┐      │
│    │    [CHARACTER PREVIEW]  │      │
│    └─────────────────────────┘      │
│                                     │
│         [ ▶ PLAY ]                  │
│                                     │
│    🚂 Train  🚌 Bus  🏃 Ground      │
│                                     │
│   [SHOP]  [SETTINGS]  [LEADERBOARD] │
│                                     │
│         Coins: 💰 1,234             │
│         High Score: 🏆 45,678       │
└─────────────────────────────────────┘
```

### 6.2 Gameplay HUD
```
┌─────────────────────────────────────┐
│ 💰 1,234          Score: 45,678     │
│                   ×2 Multiplier     │
│                                     │
│                                     │
│          [GAME AREA]                │
│                                     │
│                                     │
│                                     │
│                         [⏸️ PAUSE]  │
└─────────────────────────────────────┘
```

### 6.3 Game Over Screen
```
┌─────────────────────────────────────┐
│            GAME OVER!               │
│                                     │
│         🏆 NEW HIGH SCORE! 🏆       │
│                                     │
│           Score: 45,678             │
│           Coins: +234               │
│           Distance: 1.2 km          │
│                                     │
│    [ ▶ PLAY AGAIN ]                 │
│                                     │
│    [🎬 Watch Ad = 2x Coins]         │
│                                     │
│    [🏠 HOME]  [📊 LEADERBOARD]      │
└─────────────────────────────────────┘
```

---

## 📋 Phase 7: Audio Design

### 7.1 Music Tracks
| Scene | Style | BPM | Mood |
|-------|-------|-----|------|
| Main Menu | Cheerful orchestral | 100 | Exciting, inviting |
| Train Mode | Rhythmic locomotive | 120 | Adventurous |
| Bus Mode | Upbeat city pop | 130 | Energetic |
| Ground Mode | Playful adventure | 125 | Fun, carefree |
| Game Over | Short jingle | - | Encouraging |

### 7.2 Sound Effects
- Jump: "Boing" spring sound
- Slide: Whoosh sound
- Coin collect: Cheerful "ding"
- Power-up: Magical sparkle
- Crash: Cartoon "bonk" (not scary)
- Lane change: Quick swoosh
- Speed boost: Rocket sound
- Achievement: Celebration fanfare

---

## 📋 Phase 8: Backend Services (Event-Driven)

### 8.1 Azure Functions Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     GAME CLIENT                              │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│                   Azure API Gateway                          │
└───────────────────────┬─────────────────────────────────────┘
                        │
        ┌───────────────┼───────────────┐
        ▼               ▼               ▼
┌───────────────┐ ┌───────────────┐ ┌───────────────┐
│ Leaderboard   │ │  Cloud Save   │ │ Achievements  │
│   Function    │ │   Function    │ │   Function    │
└───────┬───────┘ └───────┬───────┘ └───────┬───────┘
        │                 │                 │
        ▼                 ▼                 ▼
┌─────────────────────────────────────────────────────────────┐
│                     Azure Cosmos DB                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │ Leaderboard │  │  GameSaves  │  │Achievements │          │
│  │ Container   │  │  Container  │  │ Container   │          │
│  └─────────────┘  └─────────────┘  └─────────────┘          │
└─────────────────────────────────────────────────────────────┘
```

### 8.2 Event Structure (Following Guidelines)
```json
{
  "eventId": "uuid-v4",
  "eventType": "game.score.submitted.v1",
  "eventTime": "2026-01-31T10:00:00Z",
  "source": "escape-train-run",
  "subject": "player-12345",
  "dataVersion": "1.0",
  "data": {
    "playerId": "player-12345",
    "score": 45678,
    "gameMode": "train",
    "coinsCollected": 234,
    "distanceTraveled": 1200,
    "powerUpsUsed": ["magnet", "shield"]
  },
  "metadata": {
    "correlationId": "session-789",
    "platform": "windows",
    "gameVersion": "1.0.0"
  }
}
```

### 8.3 Leaderboard Service
```csharp
// Following microservice guidelines
public class LeaderboardEntry
{
    public string Id { get; init; }
    public string PlayerId { get; init; }
    public string PlayerName { get; init; }
    public int Score { get; init; }
    public string GameMode { get; init; }
    public DateTime Timestamp { get; init; }
}

// Leaderboard categories
- Global All-Time
- Global Weekly
- Global Daily
- Friends (if social features added)
- Per Game Mode (Train, Bus, Ground)
```

---

## 📋 Phase 9: Monetization (Kid-Friendly)

### 9.1 Approach
- **No gambling mechanics** (no loot boxes)
- **No pay-to-win** elements
- **Rewarded ads only** (opt-in)
- **Parental controls** for purchases

### 9.2 Revenue Streams
| Type | Description | Implementation |
|------|-------------|----------------|
| Rewarded Ads | Watch ad for 2x coins | After game over |
| Premium Characters | One-time purchase | In-app purchase |
| Remove Ads | One-time purchase | $2.99 |
| Coin Packs | For impatient players | With parental gate |

---

## 📋 Phase 10: Development Timeline

### Sprint 1-2: Core Foundation (Weeks 1-4)
- [ ] Unity project setup
- [ ] Basic player controller (movement, jump, slide)
- [ ] Lane system implementation
- [ ] Basic camera follow
- [ ] Input handling (touch + keyboard)

### Sprint 3-4: Level Generation (Weeks 5-8)
- [ ] Procedural track generation
- [ ] Object pooling system
- [ ] Basic obstacle spawning
- [ ] Coin spawning and collection
- [ ] Collision detection

### Sprint 5-6: Environment Themes (Weeks 9-12)
- [ ] Train theme implementation
- [ ] Bus theme implementation
- [ ] Ground/Park theme implementation
- [ ] Theme switching system
- [ ] Environment art integration

### Sprint 7-8: Power-Ups & Characters (Weeks 13-16)
- [ ] Power-up system
- [ ] All power-up types
- [ ] Character system
- [ ] Character shop
- [ ] Character animations

### Sprint 9-10: UI & Audio (Weeks 17-20)
- [ ] Main menu UI
- [ ] Gameplay HUD
- [ ] Settings menu
- [ ] Shop UI
- [ ] All audio integration
- [ ] Music and SFX

### Sprint 11-12: Backend & Polish (Weeks 21-24)
- [ ] Azure Functions setup
- [ ] Leaderboard implementation
- [ ] Cloud save system
- [ ] Achievement system
- [ ] Bug fixes and optimization
- [ ] Performance testing

### Sprint 13-14: Testing & Launch (Weeks 25-28)
- [ ] Beta testing
- [ ] Store assets preparation
- [ ] App Store submission (iOS)
- [ ] Google Play submission (Android)
- [ ] Microsoft Store submission (Windows)
- [ ] Launch!

---

## 📋 Phase 11: Testing Strategy

### 11.1 Unit Tests
```csharp
[Test]
public void Player_ChangeLane_MovesToCorrectPosition()
{
    // Arrange
    var player = CreateTestPlayer(lane: 2); // Center lane
    
    // Act
    player.ChangeLane(Direction.Left);
    
    // Assert
    Assert.AreEqual(1, player.CurrentLane);
    Assert.AreEqual(-2.5f, player.TargetPosition.x);
}

[Test]
public void PowerUp_Magnet_AttractsCoinsWithinRange()
{
    // Arrange
    var player = CreateTestPlayer();
    var magnet = new MagnetPowerUp(range: 5f);
    var coin = CreateTestCoin(distanceFromPlayer: 3f);
    
    // Act
    magnet.Activate(player);
    
    // Assert
    Assert.IsTrue(coin.IsBeingAttracted);
}
```

### 11.2 Play Mode Tests
- Complete run simulation
- Power-up activation sequences
- UI navigation flows
- Save/Load cycles

### 11.3 Performance Benchmarks
| Metric | Target (Mobile) | Target (Windows) |
|--------|-----------------|------------------|
| FPS | 60 fps stable | 60 fps stable |
| Memory | < 500 MB | < 1 GB |
| Load Time | < 3 seconds | < 2 seconds |
| Battery Usage | Moderate | N/A |

---

## 📋 Phase 12: Quality Checklist

### Pre-Launch Checklist
- [ ] All game modes playable
- [ ] No crashes in 100 consecutive runs
- [ ] Leaderboard syncs correctly
- [ ] Cloud save works across devices
- [ ] All characters unlock correctly
- [ ] All power-ups function properly
- [ ] Audio settings persist
- [ ] Touch controls responsive
- [ ] Keyboard controls work
- [ ] UI scales to all resolutions
- [ ] Parental controls functional
- [ ] Privacy policy in place
- [ ] COPPA compliant

### Performance Checklist
- [ ] No frame drops below 30 FPS
- [ ] Memory stays under limit
- [ ] No memory leaks after 30 min play
- [ ] Load times acceptable
- [ ] Smooth lane transitions
- [ ] Responsive controls (< 100ms input lag)

---

## 📋 Technical Specifications Summary

### Minimum Requirements

#### Mobile (iOS)
- iOS 12.0 or later
- iPhone 6s or newer
- 200 MB storage

#### Mobile (Android)
- Android 7.0 (API 24) or later
- 2 GB RAM minimum
- OpenGL ES 3.0 support
- 200 MB storage

#### Windows
- Windows 10/11
- DirectX 11 support
- 4 GB RAM
- 500 MB storage
- Integrated graphics or better

---

## 🎯 Key Success Metrics

| Metric | Target |
|--------|--------|
| Day 1 Retention | > 40% |
| Day 7 Retention | > 20% |
| Average Session Length | > 5 minutes |
| Crash-Free Rate | > 99.5% |
| App Store Rating | > 4.5 stars |
| Daily Active Users | Growth target TBD |

---

## 🚀 Next Steps

1. **Review and approve** this implementation plan
2. **Set up Unity project** with recommended structure
3. **Create basic prototype** with core mechanics
4. **Iterate based on playtesting** feedback
5. **Proceed through sprints** as outlined

---

*Document Version: 1.0*
*Last Updated: January 31, 2026*
*Project: Escape Train Run*
