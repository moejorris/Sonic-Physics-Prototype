# Unity Momentum Platformer Prototype

A Unity (C#) prototype implementing a deterministic 2D momentum-based movement system using a publicly available physics guide as a behavioral specification rather than a complete implementation blueprint.

The focus of this project is **deterministic movement, slope-aware kinematic physics, readable system design, and faithful animation behavior**, with visuals serving primarily as validation tools.

---

## Project Overview

This project explores how to implement high-speed, ground-relative movement that remains stable across slopes, walls, and ceilings.

The *Sonic Physics Guide* was used as a **technical reference describing expected outcomes** (movement behavior, edge cases, animation rules), but it does not provide full implementation details.  
Numerous edge cases—particularly around collision resolution, state transitions, and snapping behavior—were independently designed and solved during development.

All simulation and gameplay code is original and written from scratch in Unity using C#.

## Technical Highlights

- Implemented a custom kinematic character controller without relying on Unity Rigidbody physics
- Designed a fixed-step simulation loop with frame-independent movement behavior
- Built custom collision sensing using raycast surface queries
- Developed state-driven animation logic decoupled from gameplay systems
- Created editor debugging tools to visualize collision data and surface normals

## Technical Challenges

- Maintaining stable movement across arbitrary surface angles while preserving player-relative velocity
- Implementing robust collision resolution without relying on Unity's built-in physics engine
- Separating simulation state from presentation state to prevent animation logic from affecting gameplay behavior
- Debugging edge cases involving slopes, transitions between grounded and airborne states, and high-speed movement

---

## Simulation Model

- Physics simulation runs at a **fixed 60 ticks per second**
- Rendering framerate is fully decoupled from simulation
- Movement behavior is deterministic and frame-rate independent
- Visual interpolation is used for smooth rendering
- Movement does **not** rely on Unity Rigidbody forces

This approach prioritizes:
- Consistent movement behavior
- Stable collision handling at high velocity
- Predictable debugging and iteration

---

## Architecture Overview

This project intentionally favors **cohesive, readable systems** over extreme fragmentation.

Rather than splitting tightly coupled movement behavior across many small scripts, responsibilities are grouped to preserve **local reasoning**—the ability to understand, debug, and modify behavior without jumping across dozens of files.

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

## Asset & IP Notes

Character visuals are temporary placeholder assets used solely for technical validation of movement and animation systems. They are not part of the original implementation and are not intended for redistribution or commercial use. The focus of this repository is the engineering behind the movement, collision, and animation systems.

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

## Disclaimer

This project is an original implementation based on publicly documented behavior described in the Sonic Physics Guide. All gameplay and simulation code was written from scratch. No original game source code was used.

---
