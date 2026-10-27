# Solaler

<p align="center">
  <img src="Documentation/banner.png" alt="Solaler Banner" width="800" />
</p>

A modular, high-performance 2D top-down action game framework built in **Unity 6**, following strict **SOLID principles**, **OOP design patterns**, and inspired by the deliberate movement and shooting mechanics of *The Binding of Isaac*.

---

## Key Highlights

- **Top-Down 2D Physics**: 8-directional movement with normalized diagonals, snappy acceleration, and zero platformer gravity.
- **Isaac-Style Shooting**: 360-degree projectile shooting toward the cursor via direct pooling / dynamic instantiation. Projectiles spawn offset from the player body and reliably impact walls/enemies.
- **True Death Sequence**: Death animations (shattering skeleton/dust) trigger strictly upon health depletion (0 HP), keeping attack actions smooth and non-destructive.
- **Zero-Allocation Architecture**: Animator parameters use pre-cached `Animator.StringToHash` integers; physics queries and state switching allocate zero garbage during update loops.
- **SOLID & State Pattern**: Clean decoupling between input, movement, animation, combat, and health subsystems.
- **Isaac-Style Interconnected Rooms**: Discrete room-to-room transitions through cardinal doorways with smooth camera panning, boundary locks, and safe teleport thresholds.
- **Extensible Puzzle Mechanics**: Decoupled puzzle elements (`IPuzzleElement`) including pressure plates and pushable blocks, orchestrated by `PuzzleController` to unlock sealed doors upon completion.

---

## Controls

| Action | Control | Description |
|---|---|---|
| **Move** | `W` `A` `S` `D` / Arrow Keys | 8-directional top-down walking with responsive friction |
| **Shoot** | `Left Mouse Button` (Hold / Click) | Fires tear/orb projectiles toward the mouse cursor |
| **Dodge Roll / Dash** | `Spacebar` / `Shift` | Burst evasion dash in the current movement direction |

---

## Architectural Breakdown (SOLID Principles)

```
                         +--------------------------+
                         |     PlayerController     | (Facade Orchestrator)
                         +------------+-------------+
                                      |
         +-----------------+----------+----------+-----------------+
         |                 |                     |                 |
  +------v-------+  +------v-------+      +------v-------+  +------v-------+
  | IPlayerInput |  | IMovement    |      | PlayerCombat |  | PlayerHealth |
  | (InputReader)|  | (TopDown 2D) |      | (Projectiles)|  |  (HP & Death)|
  +--------------+  +--------------+      +--------------+  +------+-------+
                                                                   |
                                                            Triggers Death Only
                                                            When Health <= 0
```

1. **Single Responsibility Principle (SRP)**:
   - `PlayerInputReader`: Translates raw keyboard, mouse, and New Input System events into game intents.
   - `PlayerMovement`: Manages Rigidbody2D velocity, top-down acceleration/deceleration, and sprite flipping.
   - `PlayerCombat`: Manages fire rate cooldowns, projectile trajectory calculation, and projectile instantiation.
   - `PlayerProjectile`: Handles projectile travel, lifetime decay, and impact collisions.
   - `PlayerHealth`: Manages hit points, invincibility frames, and death events.
   - `PlayerAnimationController`: Synchronizes visual states via pre-cached parameter hashes.
2. **Open/Closed Principle (OCP)**:
   - Behavior is structured around `PlayerBaseState`. New character actions (e.g., Charge Shot, Shield, Teleport) can be added as new states without altering existing states.
   - Movement and combat variables are exposed in `PlayerMovementData` (ScriptableObject) for runtime tuning without code changes.
3. **Liskov Substitution Principle (LSP)**:
   - All states (`PlayerIdleState`, `PlayerMoveState`, `PlayerDashState`, `PlayerDeadState`) derive cleanly from `PlayerBaseState`.
   - Any input provider (AI, replay, virtual gamepad) can substitute for `IPlayerInput`.
4. **Interface Segregation Principle (ISP)**:
   - Lean interfaces: `IPlayerInput`, `IMovementController`, `IPlayerState`.
5. **Dependency Inversion Principle (DIP)**:
   - High-level coordinator (`PlayerController`) and state classes depend on abstractions rather than concrete hardware drivers.

---

## Project Structure

```
Assets/
├── Animations/
│   ├── Player_Idle.anim                  # Breathing idle loop
│   ├── Player_Move.anim                  # 6-frame top-down walk cycle
│   ├── Player_Dash.anim                  # Dash posture
│   ├── Player_Die.anim                   # Shattering death explosion
│   └── PlayerAnimatorController.controller
├── Prefabs/
│   ├── Player.prefab                     # Wired player character
│   └── PlayerProjectile.prefab           # Tear/orb projectile
├── Scenes/
│   └── SampleScene.unity                 # 3-room connected dungeon with puzzle room
├── Scripts/
│   ├── Animation/                        # PlayerAnimationController.cs
│   ├── Combat/                           # PlayerCombat.cs, PlayerProjectile.cs, PlayerHealth.cs
│   ├── Core/Interfaces/                  # IPlayerInput.cs, IMovementController.cs, IPlayerState.cs
│   ├── Dungeon/                          # Isaac-style room, doorway, and puzzle system
│   │   ├── Core/                         # IDoor.cs (IDoor, IPuzzleElement, Direction)
│   │   ├── Puzzles/                      # PressurePlate.cs, PushableBlock.cs, PuzzleController.cs
│   │   ├── Doorway.cs                    # Doorway transitions & barriers
│   │   ├── DungeonManager.cs             # Camera panning & player teleportation
│   │   └── Room.cs                       # Room boundaries & puzzle listeners
│   ├── Editor/                           # PlayerAssetGenerator.cs
│   ├── Input/                            # PlayerInputReader.cs
│   ├── Movement/                         # PlayerMovement.cs, PlayerMovementData.cs
│   ├── StateMachine/                     # PlayerStateMachine.cs, PlayerBaseState.cs
│   │   └── States/                       # Idle, Move, Dash, Dead states
│   └── PlayerController.cs
├── Settings/
│   └── DefaultPlayerMovementData.asset   # Tunable movement & combat parameters
└── Sprites/
    ├── PlayerSpriteSheet.png             # Character & effect sprite sheet
    └── playerHUD.png
```

---

## Getting Started

1. Clone repository:
   ```bash
   git clone https://github.com/mohalbyt/Solaler.git
   ```
2. Open project in **Unity 6 (6000.0.33f1 or newer)**.
3. Open `Assets/Scenes/SampleScene.unity`.
4. Press **Play** in the Unity Editor to test movement and combat.
