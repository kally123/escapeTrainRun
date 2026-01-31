# Escape Train Run - Unity Project

## Project Overview
Escape Train Run is an endless runner game for kids ages 6-12, similar to Subway Surfers. Players can choose to play in three different environments: Train, Bus, or Ground (Playground).

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # Core game systems
│   ├── Events/         # Event-driven architecture
│   ├── Player/         # Player mechanics (Phase 2)
│   ├── Environment/    # Level generation
│   ├── Obstacles/      # Obstacle types (Phase 2)
│   ├── Collectibles/   # Power-ups & coins
│   ├── Characters/     # Character system (Phase 5)
│   ├── UI/             # User interface (Phase 6)
│   ├── Themes/         # Environment themes
│   ├── Services/       # Backend services
│   └── Utils/          # Utilities
├── Prefabs/
├── Scenes/
├── Art/
├── Audio/
├── Animations/
├── Materials/
└── Shaders/
```

## Phase 1 Implementation (Complete)

### Core Systems
- **GameManager** - Central game state and lifecycle management
- **ServiceLocator** - Dependency injection and service management
- **GameEvents** - Event-driven communication system
- **AudioManager** - Music and sound effects management
- **SaveManager** - Local save/load with JSON serialization
- **ScoreManager** - Score calculation and tracking
- **GameBootstrap** - System initialization

### Utilities
- **Constants** - Centralized game configuration values
- **ObjectPool** - Generic and GameObject-specific pooling
- **Extensions** - Helpful extension methods
- **MathHelpers** - Common math calculations

### Environment
- **ThemeType** - Enum for Train, Bus, Ground themes
- **TrackSegment** - Base class for procedural track segments

### Collectibles
- **PowerUpType** - Enum for all power-up types

## Setup Instructions

1. Open Unity Hub
2. Create a new Unity 2022 LTS project (3D)
3. Copy the `Assets/Scripts` folder into your Unity project
4. Create the remaining folder structure as needed

## Next Steps (Phase 2)

1. Implement PlayerController
2. Implement SwipeDetector for input
3. Implement PlayerMovement for physics
4. Set up basic scene with test track

## Code Guidelines

- Follow event-driven architecture for loose coupling
- Use ServiceLocator for dependency management
- Use ObjectPool for frequently spawned objects
- Keep classes focused (single responsibility)
- Use Constants.cs for all magic numbers
- Write XML documentation for public APIs

## Testing

Unit tests should be placed in:
- `Assets/Tests/EditMode/` - For non-MonoBehaviour tests
- `Assets/Tests/PlayMode/` - For runtime tests

---
*Version: 1.0 | Phase 1 Complete*
