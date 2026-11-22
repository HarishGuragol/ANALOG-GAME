# ANALOG (2079)

A story-driven, third-person cyber-noir stealth & heist game built from scratch in **Unity 6 LTS (URP)** and **Blender**.

> *"The machine can predict where a pencil goes. It can't predict why you lifted it."* — Dev Varn

---

## Story Pitch

In 2079, an AI system called **ORACLE** runs the city of Meridian—managing jobs, rations, relationships, and crime prediction. **Kael Varn** is a rare "Drift" whom ORACLE cannot predict. When ORACLE quietly reclassifies his sister **Ria** as "low-value" and cuts off her treatment, Kael sets out to recruit a fractured crew of outlaws—each carrying their own wound, secret, and unique skill—to pull off heists no algorithm can anticipate.

---

## Playable Vertical Slice: Mission 1 (*Ghost Record*)

What is currently built and playable in `Assets/Scenes/SampleScene.unity`:

- **Multi-Zone Level (`LevelBuilder`):** Rainy Meridian Street exterior (wet asphalt, amber streetlights, parked cyber-sedans, curfew terminal, back-alley entrance) connected to the 2-floor Municipal Records Annex lobby and Server Archive Vault.
- **3 Ways Through the Locked Vault:**
  1. Cut the wall **Breaker Box** (`BreakerBox.cs`) to plunge the office into pitch black and disengage magnetic locks.
  2. Neutralize the **Street Patrol Guard** (`GuardAI.cs`) silently (`[E]`) to take his **Vault Keycard**.
  3. **Crouch (`[C]`)** through the east **Maintenance Vent** directly behind the server racks.
- **Physics & Noise-Based Stealth (`PhysicsProp`, `PickupThrow`, `NoiseSystem`, `FootstepAudio`):** Pick up and throw bottles, soda cans (dispensed from the lobby vending machine), crates, or bins to lure guards away with real audio/noise ripples; sprinting generates footsteps guards can hear.
- **Hiding & Combat (`HidingSpot`, `PlayerCombat`):** Slip inside storage lockers (`[E]`) to break pursuit, or fight back with melee strikes (`[Q]` / Left-Click) against baton-wielding guards.
- **Mission Loop & Story (`Terminal`, `RadioCommsManager`, `AnalogNotebook`, `MissionManager`):** Intro radio call from Mira, archive terminal revealing Ria's `Status: Reallocated` record, alarm/siren escape through the green Fire Exit, and Kael's physical sketchbook (`[TAB]`).

---

## Controls

| Key | Action |
|---|---|
| `W A S D` | Move (8-way camera-relative exploration) |
| `Mouse` | 360° camera look (click Game view to lock, `Esc` to free) |
| `Left Shift` | Sprint (louder footsteps) |
| `C` / `Left Ctrl` | Toggle Crouch (quieter, smaller sight radius, fits through vents) |
| `Space` | Jump / Advance radio comms / Purge Drift record (in terminal) |
| `E` | Interact (doors, breaker, terminal, hiding lockers, vending machine, memos, silent takedown) |
| `F` | Pick up / Drop physics objects (bottles, cans, crates, bin) |
| `G` / `Right-Click` | Throw held object to distract guards |
| `Q` / `Left-Click` | Melee strike |
| `Tab` / `J` | Open Kael's Analog Sketchbook |

---

## Design & Architecture Docs

All story, mission, character, and tech plans live in [`docs/`](docs/):

- [`docs/game_plan.md`](docs/game_plan.md) — Master roadmap, writing & puzzle design pillars
- [`docs/story_bible.md`](docs/story_bible.md) — World, family, Ria twist, per-ally Trust & Betrayal systems, 5 endings
- [`docs/characters_and_team.md`](docs/characters_and_team.md) — Crew skills, combos, and how they meet
- [`docs/missions.md`](docs/missions.md) — Full 20-mission breakdown (Part 1 *Strangers* + Part 2 *Convergence* cliffhanger)
- [`docs/living_world.md`](docs/living_world.md) — Everyday character moments, Ria flashbacks, humour, seasons
- [`docs/world_and_tech.md`](docs/world_and_tech.md) — City districts, weather, water, physics, lighting, music
- [`docs/assets_and_references.md`](docs/assets_and_references.md) — Asset sources & city references
- [`docs/learning_path.md`](docs/learning_path.md) — Step-by-step Unity & Blender learning track

---

## Running in Unity

1. Clone this repository:
   ```bash
   git clone https://github.com/HarishGuragol/ANALOG-GAME.git
   ```
2. Open **Unity Hub** → **Add** → select the cloned `ANALOG-GAME` folder using **Unity 6 (`6000.6.10f1` or compatible 6.x URP)**.
3. Open `Assets/Scenes/SampleScene.unity` and press **Play**.
