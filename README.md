# 🚂 Escape Train Run

An endless runner game for kids (ages 6-12) inspired by Subway Surfers, built with Unity 2022 LTS.

![Unity](https://img.shields.io/badge/Unity-2022%20LTS-blue)
![Platform](https://img.shields.io/badge/Platform-iOS%20%7C%20Android%20%7C%20Windows-green)
![License](https://img.shields.io/badge/License-Proprietary-red)
![Status](https://img.shields.io/badge/Status-Development%20Complete-brightgreen)

## 🎮 Game Overview

**Escape Train Run** is an endless runner where players dodge obstacles, collect coins, and unlock characters across three exciting environments:

- 🚂 **Train Mode** - Run across train compartments
- 🚌 **Bus Mode** - Jump between buses in city traffic  
- 🌳 **Park Mode** - Sprint through parks and playgrounds

## ✨ Features

### Core Gameplay
- **Swipe Controls** - Intuitive touch controls for mobile
- **Keyboard Support** - WASD/Arrow keys for desktop
- **3-Lane System** - Classic endless runner mechanics
- **Progressive Difficulty** - Speed increases over time

### Power-Ups
| Power-Up | Effect | Duration |
|----------|--------|----------|
| 🧲 Magnet | Attracts nearby coins | 8 seconds |
| 🛡️ Shield | Protects from one hit | 10 seconds |
| ⚡ Speed Boost | Temporary speed increase | 5 seconds |
| ⭐ Star Power | Invincibility + coin magnet | 8 seconds |
| 2️⃣ Multiplier | Double coins & score | 15 seconds |

### Characters
- Multiple unlockable characters
- Unique abilities for each character
- Character shop with coin purchases

### Backend Services
- 🏆 Global leaderboards
- ☁️ Cloud save synchronization
- 🏅 Achievement system (45+ achievements)

## 🛠️ Technical Stack

| Component | Technology |
|-----------|------------|
| Game Engine | Unity 2022 LTS |
| Language | C# |
| Testing | NUnit + Unity Test Framework |
| Backend | Azure Functions + Cosmos DB |
| Audio | Unity Audio System |

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # GameManager, SaveManager, PoolManager, Events
│   ├── Player/         # PlayerController, Movement, Collision, Input
│   ├── Environment/    # LevelGenerator, TrackSegment, Themes
│   ├── Obstacles/      # Obstacle types and spawning
│   ├── Collectibles/   # Coins, PowerUps, Effects
│   ├── Characters/     # Character data and unlocking
│   ├── UI/             # All UI controllers
│   ├── Audio/          # Audio management
│   ├── Effects/        # Visual effects and particles
│   ├── Services/       # Backend services (Leaderboard, CloudSave, Achievements)
│   ├── Quality/        # QA tools and performance monitoring
│   ├── Config/         # Configuration ScriptableObjects
│   └── Editor/         # Unity Editor tools
├── Tests/
│   ├── EditMode/       # Unit tests (12 test files)
│   └── PlayMode/       # Integration tests (10 test files)
├── Prefabs/
├── Scenes/
├── Resources/
└── Art/
```

## 🧪 Testing

### Run Tests in Unity
1. Open **Window > General > Test Runner**
2. Select **EditMode** or **PlayMode** tab
3. Click **Run All**

### Test Coverage
- **EditMode Tests**: 12 test files covering all systems
- **PlayMode Tests**: 10 test files for integration testing
- **Performance Tests**: FPS, memory, latency benchmarks

## 🔧 Quality Assurance

Access QA tools via Unity menu: **Escape Train Run > Quality**

- **Quality Checker** - Runs pre-launch checklist
- **Project Structure Checker** - Validates project setup
- **Pre-Build Validator** - Automatic validation before builds

### Performance Targets
| Metric | Mobile | Desktop |
|--------|--------|---------|
| FPS | 60 stable | 60 stable |
| Memory | < 500 MB | < 1 GB |
| Load Time | < 3 sec | < 2 sec |
| Input Latency | < 100 ms | < 100 ms |

## 👶 Kids Safety (COPPA Compliance)

- ✅ No personal data collection from children
- ✅ No behavioral advertising
- ✅ Parental gate for sensitive features
- ✅ No open chat or social features
- ✅ Privacy policy included
- ✅ Age-appropriate content only

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3 LTS or later
- Visual Studio 2022 or VS Code with C# extension

### Setup
1. Clone the repository
2. Open project in Unity Hub
3. Wait for package imports to complete
4. Open `Assets/Scenes/MainMenu.unity`
5. Press Play!

### Build
1. Go to **File > Build Settings**
2. Select target platform (iOS/Android/Windows)
3. Configure Player Settings
4. Click **Build**

## 📊 Implementation Status

All 12 phases complete! ✅

| Phase | Description | Status |
|-------|-------------|--------|
| 1 | Core Architecture | ✅ Complete |
| 2 | Player System | ✅ Complete |
| 3 | Level Generation | ✅ Complete |
| 4 | Power-Up System | ✅ Complete |
| 5 | Character System | ✅ Complete |
| 6 | UI System | ✅ Complete |
| 7 | Audio System | ✅ Complete |
| 8 | Effects System | ✅ Complete |
| 9 | Obstacle System | ✅ Complete |
| 10 | Backend Services | ✅ Complete |
| 11 | Testing Strategy | ✅ Complete |
| 12 | Quality Checklist | ✅ Complete |

## 📄 Documentation

- [Implementation Plan](docs/IMPLEMENTATION_PLAN.md) - Detailed development roadmap
- [Privacy Policy](Assets/Resources/Legal/PrivacyPolicy.txt) - COPPA-compliant policy
- [Terms of Service](Assets/Resources/Legal/TermsOfService.txt) - Usage terms

## 🎯 Controls

### Mobile (Touch)
| Gesture | Action |
|---------|--------|
| Swipe Left | Move to left lane |
| Swipe Right | Move to right lane |
| Swipe Up | Jump |
| Swipe Down | Slide |

### Desktop (Keyboard)
| Key | Action |
|-----|--------|
| A / ← | Move left |
| D / → | Move right |
| W / ↑ / Space | Jump |
| S / ↓ | Slide |
| ESC | Pause |

## 📈 Version History

- **v1.0.0** (Feb 2026) - Initial release with all features

## 👥 Credits

Developed as an AI-assisted game development project.

## 📜 License

Proprietary - All rights reserved.

---

*Made with ❤️ for kids who love endless runners!*
