# ANALOG: Game Plan (Master)

**Engine:** Unity 6 LTS (URP) · **3D:** Blender · **Platform:** macOS first
**Genre:** Third-person, story-driven heist / immersive sim · **Tone:** dark, grounded cyber-noir (no neon)
**Approach:** everything built from scratch, learning-first, guided step by step.

---

## 1. Pitch
In 2079 the AI **ORACLE** runs the city of Meridian. **Kael Varn**, a "Drift" ORACLE can't predict, lost his sister Ria when ORACLE reclassified her as "low-value". He builds a crew of flawed people, each with a skill nobody else has, and together they pull off heists no algorithm can predict, until he discovers Ria's mind is inside ORACLE.

## 2. Design Pillars
1. **Common-sense puzzles:** observe, improvise, combine. No maths. 2-3 valid solutions per hurdle.
2. **Bonds and betrayal:** hidden per-character trust shapes dialogue, routes, betrayals and endings.
3. **Show, don't tell:** objects, silence and small habits carry the emotion.
4. **Life, not just trauma:** humour, routines, friendships, time passing.
5. **Grounded cyber-noir:** amber streetlights, rain, concrete, steel.

## 3. Scope
- **20 missions**, 25-30 minutes each → **~9-10 hours**.
- **Released in two parts:**
  - **Part 1 "Strangers"** (M1-M10, ~5h), ends on the Ria-voice cliffhanger.
  - **Part 2 "Convergence"** (M11-M20, ~5h), save import from Part 1.
- **5 endings:** Shutdown, Rewrite, Merge, Fracture, hidden Release.

## 4. Locked Decisions
| Decision | Choice |
|---|---|
| Camera | Third-person (Cinemachine) |
| Art and systems | From scratch: Blender models/rigs/animation, custom C# |
| Dialogue | Text-first with subtle audio; voice optional later |
| World | Hub district (Lowmarket) + handcrafted mission levels, not open world |
| City | Fictional Meridian, inspired by Kowloon, Hong Kong/Tokyo backstreets, brutalist concrete, old Mumbai/Kolkata, Singapore skyline |

## 5. Writing Rules
- Max 2-3 sentences per line; characters talk like tired real people.
- Objects carry memory: sketchbook, pencil, `Status: reallocated`.
- No trust UI or popups; trust shown through behaviour.
- Betrayals are foreshadowed fairly and can be prevented.
- One quiet emotional moment per mission.

## 6. Puzzle Rules
- Rule of 3 clues: every solution is hinted in the world (cables, notes, scratch marks).
- Predictable cause and effect (cutting power kills lights and cameras and locks magnetic doors).
- 2-3 routes: social/observe, physical/stealth, technical/improvise.
- Kael's notebook is the hint system.
- Greybox playtest before any art.

## 7. Core Systems
- `PlayerController`, `InteractionSystem` (inspect, grab, carry, use)
- `DialogueSystem` + `FlagManager`
- `TrustManager`, `BetrayalEvaluator`, `EndingResolver`
- `StealthSystem` (noise, light, vision cones, alert states)
- `WorldSystems` (linked power, doors, cameras, alarms)
- `NotebookUI`, `SaveSystem` (with Part 1 → Part 2 import)
- Ally skill system (Kael, Mira, Doc, Jun, Sera combos)

## 8. Build Order
1. **Mission 1 vertical slice** (Ghost Record): the template for everything.
2. **Part 1** (M1-M10) → shareable release.
3. **Part 2** (M11-M20).

Realistic timeline solo while learning: **Part 1 ≈ 3-5 months, full game ≈ 6-9 months.**

## 9. Document Index
| Doc | Contents |
|---|---|
| [story_bible.md](story_bible.md) | World, family, the truth about Ria, trust and betrayal systems, endings |
| [characters_and_team.md](characters_and_team.md) | Crew profiles, unique skills, combos, how they meet |
| [missions.md](missions.md) | All 20 missions in detail + Part 1/Part 2 cliffhanger |
| [living_world.md](living_world.md) | Ordinary lives, relationships, Ria flashbacks, humour, time passing |
| [world_and_tech.md](world_and_tech.md) | Map, districts, weather, water, physics, lighting, animation, music |
| [assets_and_references.md](assets_and_references.md) | Research on available city data, asset packs, reference cities |
| [learning_path.md](learning_path.md) | Step-by-step lessons and progress |
