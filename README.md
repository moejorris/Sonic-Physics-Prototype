# Sonic Physics Prototype in Unity

A Unity (C#) prototype implementing a 2D momentum-based movement system using the [Sonic Physics Guide](https://info.sonicretro.org/Sonic_Physics_Guide) as a technical specification rather than a complete implementation blueprint.

The focus of this project is **deterministic movement, slope-aware kinematic physics, readable system design, and faithful animation behavior**, with visuals serving primarily as validation tools.

<img width="478" height="478" alt="physics_hero" src="https://github.com/user-attachments/assets/4d322f78-6ad1-482d-b5f6-f5471032589d" />

---

## Web Build

You can play the demo on [Unity Play](https://play.unity.com/en/games/ee2f8051-b255-4582-96fc-4667fd9b665d/sonic-physics-prototype)

Controls listed in the description.

---

## Project Overview

This project explores how to implement high-speed, ground-relative movement that remains stable across slopes, walls, and ceilings.

The *Sonic Physics Guide* was used as a **technical reference describing expected outcomes** (movement behavior, edge cases, animation rules), but it does not provide full implementation details.  
Numerous edge cases—particularly around collision resolution, state transitions, and snapping behavior—were independently designed and solved during development.

All simulation and gameplay code is original and written from scratch in Unity using C#.

This repository is a curated showcase of an earlier project, refactored, polished, and streamlined into a focused portfolio piece.

## Technical Highlights

<img width="476" height="486" alt="physics_360_loop" src="https://github.com/user-attachments/assets/37d81215-26b1-4536-880e-9cffcdca3239" />


- Implemented a custom kinematic character controller without relying on Unity Rigidbody physics
- Designed a fixed-step simulation loop with frame-independent movement behavior
- Built custom collision sensing using raycast surface queries
- Developed state-driven animation logic decoupled from gameplay systems
- Created editor debugging tools to visualize collision data and surface normals

## Technical Challenges

<img width="476" height="486" alt="physics_arbitgeo" src="https://github.com/user-attachments/assets/060a13ce-674d-426d-ac1e-12a140f9ce2d" />

- Maintaining stable movement across arbitrary surface angles while preserving player-relative velocity
- Implementing robust collision resolution without relying on Unity's built-in physics engine
- Separating simulation state from presentation state to prevent animation logic from affecting gameplay behavior
- Debugging edge cases involving slopes, transitions between grounded and airborne states, and high-speed movement

---

## Movement Model

The controller represents movement using two coordinate systems:

### World Velocity
Used for airborne movement and external forces.

### Ground Relative Speed
Used while grounded, allowing movement to follow arbitrary surfaces.

Ground speed is converted into world velocity using the current surface angle:

```
velocity.x = groundSpeed * cos(surfaceAngle)
velocity.y = groundSpeed * sin(surfaceAngle)
```

This allows the same movement rules to function across floors, walls, and ceilings.

---

## Simulation Model

- Physics simulation runs at a **fixed 60 ticks per second**
- Rendering framerate is fully decoupled from simulation
- Movement behavior is deterministic and frame-rate independent
- Visual interpolation is used for smooth rendering
- Movement does **not** rely on Unity Rigidbody or Character Controller components

<img width="476" height="486" alt="physics_steppedoneway" src="https://github.com/user-attachments/assets/45b8317c-bd69-454e-a24a-fe05f64d3d44" />

This approach prioritizes:
- Consistent movement behavior
- Predictable collision behavior during high-speed movement
- Easy debugging and iteration

---

## Architecture Overview

This project intentionally favors **cohesive, readable systems** over extreme fragmentation.

Systems are separated by responsibility boundaries rather than arbitrary file size. Closely coupled movement logic remains together to preserve local reasoning and simplify debugging.

During development, I found that maintaining clear ownership of movement responsibilities was important for debugging complex interactions between collision, physics, and animation systems. This architecture prioritizes traceable execution flow while preserving separation between major responsibilities.

### Core Classes

#### `PlayerMovement`
- Owns all movement decision-making per physics step
- Reads input and current state to compute velocity updates
- Handles gravity, acceleration, jumping, rolling, braking, and ground snapping
- Maintains two state representations:
  - **Movement / Physics State** (e.g. Grounded, Airborne, Rolling)
  - **Descriptive / Action State** (e.g. Standing, Running, Jumping, Crouching)

This separation allows physics rules and animation intent to remain distinct while still being evaluated together.

#### `PlayerSensors`
- Performs all collision and surface detection
- Implements custom ground and collision sensors to gather surface normals, contact distances, and collision data for movement resolution.
- Provides collision information to movement systems without directly controlling player behavior

(Some sensor logic currently applies limited velocity correction; this is a known refactor target and is documented in code.)

#### `GenesisAnimator` (Custom Player Animation System)
- Fully custom, state-driven animation system
- Reads movement and action state but **never modifies gameplay**
- Implements frame-accurate animation timing and transition rules
- Implements state-driven animation behavior independent of Unity’s Animator state machine
---

## Animation System

<img width="476" height="486" alt="physics_animations" src="https://github.com/user-attachments/assets/b9d3981d-26f6-4317-ba45-005d925b2e82" />

The animation system was implemented based on documented behavioral requirements, including:

- Explicit animation transition rules
- Speed-based playback rates
- Frame-accurate timing
- Strict separation between animation and gameplay logic

Animation behavior is treated as a first-class system rather than a visual afterthought.

---

## Design Philosophy

This prototype prioritizes **clarity and correctness over maximal abstraction**.

This project keeps tightly coupled systems logically grouped to:

- Reduce cognitive overhead while debugging
- Preserve clear execution flow
- Improve iteration speed during behavior validation
- Keep the full movement model understandable

The architecture reflects the needs of a **specialized, high-complexity movement system**, rather than a generalized framework.

---

## Engineering Decisions

### Why a custom controller instead of Unity physics?

Unity's built-in physics systems are designed around general-purpose rigid body simulation. This project required direct control over acceleration, slope interaction, and ground-relative velocity, so a custom kinematic approach was used.

### Why sensor-based collision?

The reference behavior relies heavily on sampling the environment rather than relying solely on physical collision responses. A sensor-driven approach allows explicit control over grounding, slope traversal, and wall/ceiling movement.

---

## Asset & IP Notes

Character visuals are temporary placeholder assets used solely for technical validation of movement and animation systems. They are not intended for redistribution or commercial use. The focus of this repository is the engineering behind the movement, collision, and animation systems.

---

## What This Project Demonstrates

- Implementing systems from an incomplete external specification
- Translating behavioral specifications into working gameplay systems
- Solving undocumented edge cases in movement and collision behavior
- Designing deterministic, fixed-step gameplay systems
- Building custom animation logic independent of engine abstractions
- Making intentional architectural tradeoffs based on project scope

---

## Tools & Technologies

- **Engine:** Unity
- **Language:** C#
- **Simulation:** Fixed-step kinematic movement
- **Animation:** Custom state-driven system
- **Debugging:** In-editor visualization tools

---

## Scope & Intent

This is a **technical prototype**, not a finished game.

Visual polish and content scope are intentionally limited to keep the focus on:
- Movement correctness
- System readability
- Debuggability
- Engineering decision-making

---

## Known Limitations

- Extremely shallow airborne slope landings may occasionally fail due to the two-sensor collision model.
- Certain high-speed edge cases can require additional collision refinement.
- The controller prioritizes deterministic behavior and readability over reproducing every undocumented behavior of the original games.
---

## Disclaimer

This project is an original implementation based on publicly documented behavior described in the Sonic Physics Guide. All gameplay and simulation code was written from scratch. No original game source code was used.

Copyrighted Art Assets belong to their respective owners and were purely used for demonstration purposes.

---
