# Cataclysm Protocol

**Turn-Based Tactical Survival Game** | Unity 2D

A post-apocalyptic survival game featuring turn-based tactical combat, dynamic AI systems, and deep character progression.

## 🎮 Game Overview

- **Genre**: Turn-Based Tactical RPG / Survival
- **Engine**: Unity 2022.3 LTS
- **Platform**: PC (Windows)
- **Status**: In Development (Demo Available)

## 🌍 Setting

Set in Vermont, USA after a dual catastrophe:
- **Biotech Collapse**: Gene-modified creatures roam the wilderness
- **Solar EMP**: Periodic electromagnetic pulses disable electronics

Players take on the role of a scientist working to rebuild civilization while surviving against mutated wildlife.

## ⚔️ Core Systems

### 1. Initiative-Based Combat
Turn order determined by initiative rolls, creating dynamic tactical situations.

### 2. Grid Movement System
- Manhattan distance calculations
- Pathfinding with obstacle avoidance
- Attack range validation (min/max)

### 3. AI State Machines
Enemies feature complex behavior patterns:

**Mantis Grapple System** (See: `Mantis.cs`, `MonsterBase.cs`)
```
NORMAL → [Grapple Hit] → GRAPPLING → [Release] → NORMAL
                              ↓
                         Bite Attack
                         (turns tracked)
```
- Grapple success → Player loses DEX bonus to AC
- Release probability increases: 30% → 50% → 70%
- Dual attack modes: Blade (2d6+2) vs Grapple → Bite (4d6+2)

### 4. Weapon & Stance System
- Multiple weapon types with unique properties
- Stance modifiers affecting combat stats
- Action Point resource management

## 📁 Project Structure

```
Scripts/
├── Combat/
│   ├── BattleManager.cs      # Core combat orchestration
│   ├── BattleMoveSystem.cs   # Movement during combat
│   ├── TargetSelector.cs     # Target selection UI
│   └── CombatSystem.cs       # Damage calculations
├── AI/
│   ├── MonsterBase.cs        # Base class for all enemies
│   ├── Mantis.cs             # Mantis with grapple mechanics
│   ├── Beaver.cs             # Beaver enemy
│   ├── PorcupineBoss.cs      # Boss encounter
│   └── MonsterPatrol.cs      # Patrol behavior
├── Player/
│   ├── Player.cs             # Player controller
│   ├── PlayerCombatData.cs   # Combat statistics
│   ├── PlayerInventoryData.cs# Inventory system
│   └── PlayerVision.cs       # Fog of war
├── Systems/
│   ├── GridManager2D.cs      # Grid-based movement
│   ├── SaveManager1.cs       # Save/Load system
│   ├── StanceSystem.cs       # Combat stances
│   └── WeaponManager.cs      # Weapon handling
└── UI/
    ├── BattleUI.cs           # Combat interface
    ├── WeaponInventoryUI.cs  # Inventory display
    └── DamagePopupManager.cs # Floating damage numbers
```

## 🎯 Key Features

- **D&D-inspired combat**: Attack rolls, armor class, dice-based damage
- **Fog of War**: Limited visibility exploration
- **Save System**: Full game state persistence
- **Modular AI**: Behavior tree-based enemy logic
- **Scene Management**: Seamless area transitions

## 🔧 Technical Highlights

- Interface-driven design (`ICombatTarget`, `IMobAction`)
- Event-based UI updates
- Grid-based pathfinding
- State machine AI with memory (grapple turn tracking)

## 📜 License

Personal portfolio project. Code samples for educational reference.

## 👤 Author

LNilLea - Game Designer & Programmer

---

*Part of my game design portfolio showcasing systems design and Unity development skills.*
