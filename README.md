# Cataclysm Protocol

Turn-Based Tactical Survival Game | Unity 2D

A survival RPG where you choose your apocalypse. Combine different disaster types to create unique survival scenarios, then navigate the aftermath as a scientist seeking long-term human survival.

## 🎮 Project Overview

- Genre: Turn-Based Tactical RPG / Survival
- Engine: Unity 2022.3 LTS
- Status: In Development

## 🌍 Core Concept: Choose Your Apocalypse

The game features a modular disaster system — players select which catastrophes shape their world before each playthrough:

| Category | Disaster Types |
|----------|---------------|
| Climate | Extreme Cold, Extreme Heat, Rising Seas, Acid Rain, Thunderstorms |
| Ecology | Biological Mutation, Weaponized Evolution, Mass Proliferation |
| Technology | AI Uprising, Self-replicating Nanobots, Global EMP Events |
| War | Nuclear Wasteland, Alien Invasion |
| Cosmic | Solar Anomalies, Asteroid Impact, Supervolcano |
| Supernatural | *(Easter egg category)* |

Disasters can be combined — face mutated creatures AND periodic electromagnetic storms, or survive extreme cold in a nuclear wasteland. Each combination creates different resource challenges, enemy types, and survival strategies.

### Demo Scenario
> Weaponized biological mutations + Periodic solar EMP storms
> 
> Engineered predators hunt with terrifying efficiency, exploiting human weaknesses by design. Meanwhile, electromagnetic pulses periodically disable technology, making recovery nearly impossible. As a scientist, you see what others don't: every threat is also a potential resource.

## ⚔️ Core Systems

### Combat System
- Initiative-based turns: Dynamic turn order based on stats
- Grid-based tactics: Positioning and terrain matter
- D&D-inspired mechanics: Attack rolls, armor class, dice-based damage
- Action Point management: Strategic resource allocation

### Creature AI
Enemies feature behavior tree-driven AI with unique abilities:

Example - Mantis Grapple State Machine:
```
NORMAL → [Grapple Success] → GRAPPLING → [Release Check] → NORMAL
                                  ↓
                            Bite Attack
                            (turn counter tracked)
```

### Survival Layer
- Needs system: Water (clean/dirty), Food (nutrients, contamination), Temperature
- Environmental hazards: Vary based on selected disasters
- Resource scarcity: Every threat is a potential resource

## 📁 Code Structure
```
Scripts/
├── Combat/      # Battle management, damage, targeting
├── AI/          # Creature behaviors, state machines, behavior trees
├── Player/      # Character control, stats, inventory
├── Weapons/     # Weapon data, ranged/melee systems, stances
├── UI/          # Combat interface, health bars, menus
├── Grid/        # Grid management, fog of war, range display
├── Scene/       # Scene transitions, portals, spawn points
└── Systems/     # Save system, camera, feats, utilities
```

## 🔧 Technical Highlights

- Interface-driven design (ICombatTarget, IMobAction)
- Behavior tree architecture for diverse enemy AI
- Modular disaster system affecting gameplay variables
- Event-based UI updates
- Grid-based pathfinding

## 🎯 Design Philosophy

> "Every threat is a resource. Every weakness can be exploited — including your own."

- Player agency: Choose your challenges before the game begins
- Replayability: Different disaster combinations = different experiences
- Tactical depth: Enemies are puzzles, not just obstacles
- Scientist fantasy: Knowledge and adaptation over brute force

## 👤 Author

LNilLea - Game Designer & Programmer

---

*This repository contains gameplay systems code. Art assets and full project files not included.*
