# Plan: Solo Mercenary Game (Godot 4, 3D low-poly third-person)

## Context
The user wants a hitman/mercenary game in Godot: an open city, an upgradable home base (it starts as a rented basement and later becomes the whole building), driving, gunplay, close-quarters combat, and contracts gated by street cred. The contract types are assassination, destruction and shakedowns. The user has no Godot or programming experience, so Claude builds everything and the user plays, tests and makes design calls. Godot is not installed yet. The first deliverable is a refined concept doc plus a small playable vertical slice.

---

## Part 1: Concept improvements (design tips)

### 1. A core loop that ties everything together
**Contract board â†’ Prep (gear, car, intel) â†’ Mission â†’ Escape â†’ Payout and cred â†’ Spend on base and gear â†’ better contracts.**
Every base upgrade should change how missions play, not just add decoration. Some examples:
- **Armory**: unlocks weapon mods and lets you carry a loadout of more than two guns.
- **Gun range**: lets you test weapons and raises a skill stat (recoil control, reload speed).
- **Storage**: keeps loot and stolen items to fence later, and holds evidence you need to get rid of.
- **Garage** (new): stores and tunes cars, and lets you swap plates to lower heat on stolen cars.
- **Intel room** (new): buy target dossiers that show schedules, guard counts and entry points before a mission.
- **Med bay** (new): heals injuries between missions (see Injuries below).
- **Renovations** are the "whole building" progression: basement â†’ ground floor â†’ upper floors â†’ roof (sniper practice or a helipad late in the game).

### 2. Three currencies instead of one
- **Money**: buys things.
- **Street cred**: unlocks contract tiers and better fixers. You gain it from clean, stylish or fast jobs.
- **Heat**: police and gang attention. It rises with witnesses, civilian kills and stolen cars, and fades over in-game days. High heat brings checkpoints, patrols near your base, and eventually a raid on the base.
Cred against heat is the main tension: loud jobs pay fast but bring heat.

### 3. Contract design: give each mission type its own mechanic
- **Assassination**: the target follows a daily schedule (home â†’ cafÃ© â†’ office). Choose where and when to strike. Bonus objectives such as "make it look like an accident" or "use only a knife" earn more cred.
- **Destruction**: plant charges at weak points, cut the power, or cause a gas leak. Bonus for zero casualties or for being out of range when it goes off.
- **Shakedown/threaten**: a niche intimidation mechanic. A fear meter rises when you aim at the target, break their things, grab them, or bring backup. Too much fear and they panic, run or call the cops. Too little and they refuse. Results can be cash, intel, or the target becoming a recurring informant.
- **Later additions**: extraction/escort, retrieving an item, sabotage, tailing someone, defending your base.

### 4. Fixers and reputation
Instead of a generic board, 2â€“3 **fixers** (handlers) each prefer a style: a quiet one, a loud one, a gang contact. Your relationship with each fixer unlocks their unique gear and story contracts. This gives the world characters and gives the game a light story without cutscenes.

### 5. Niche mechanics that make the game stand out
- **Injuries**: wounds such as a shot arm (more sway) or a leg (slower) persist until treated at the med bay or a back-alley doctor.
- **Disguises / blending in**: a work uniform lets you into restricted areas.
- **Evidence**: shell casings, a car seen at the scene, and body cams raise heat unless you clean up.
- **Car chases with police**: break line of sight, then swap or ditch the car.
- **Day/night cycle**: targets' schedules and guard alertness change with the time of day.
- **Melee and takedowns**: grabs, human shields, non-lethal knockouts (they matter for shakedowns).

### 6. The "John Wick craving": gun-fu and an underworld society (original take)
The goal is that feel without copying the film's names or lore.
- **Gun-fu combat** is the core of the game, not a side feature:
  - **Close-range gun-fu**: at melee range, a shot becomes a contextual move (grab â†’ shoot point-blank, a judo throw followed by a headshot, a disarm that ends with a shot). Mixamo has free grapple, throw and takedown animations.
  - **Combo flow**: chaining kills (a takedown into a headshot into a throw) builds **Focus**. Focus lets you trigger brief slow-motion, makes reloads faster, or auto-targets the next headshot.
  - **Tactical reloads that matter**: limited magazines, and a short, deliberate reload animation with a quick-swap to a sidearm. Picking up enemy weapons mid-fight is encouraged.
  - **Armored suits**: tailored clothing upgrades act as body armor. Enemies in heavy armor require headshots or melee finishers.
  - **Environment kills**: tables, glass, railings, car doors, and books or objects you can throw.
  - **Hit reactions and ragdolls**: enemies stagger and react, using Godot's PhysicalBone3D for ragdolls.
- **An underworld society (your own invention)**: the world has a secret criminal society with rules.
  - **Blood Marks / Tokens**: a separate underworld currency, gold-ish tokens earned from contracts. Use them to pay for services: body disposal (removes heat), safehouses, doctors, the tailor, the armorer, and intel.
  - **Neutral ground**: a hidden safehouse hotel or club where violence is forbidden. You use it to rest, buy services and meet fixers. Breaking the rule is possible, but it gets you **branded**.
  - **Branded (outcast) state**: breaking underworld rules, or betraying a fixer, puts a bounty on you. Every hitman in the city hunts you, services are closed to you, and you must survive to clear your name. This works as an endgame or story arc.
  - **Markers / oaths**: favors owed between characters. You can call in a marker for help, or be forced to honor one, which leads to story contracts.
- **Vibe**: neon-soaked night city, rain, reflective streets, a synth/techno soundtrack, and clean suits. Godot 4's SDFGI, volumetric fog and glow fit this well, even with low-poly assets.
- **Your homebase arc fits this**: a grieving or retired merc who is rebuilding from a rented basement. That's a natural "pulled back in" story hook.

### 7. Free assets and tools (allowed)
- **Models**: Kenney.nl, Quaternius, KayKit (city, cars, characters, guns, all CC0), Poly Pizza, Sketchfab (filter for CC0/CC-BY).
- **Animations**: Mixamo (free, with gun, melee and takedown animations), the Quaternius Universal Animation Library.
- **Audio**: freesound.org, Sonniss GDC free bundles, OpenGameArt.
- **Textures/HDRI**: Poly Haven, ambientCG (CC0).
- **Godot addons**: Phantom Camera (camera), Godot Jolt or the built-in Jolt physics (better vehicles and ragdolls), Dialogue Manager (fixer conversations).
- Claude keeps an `assets/CREDITS.md` with licenses for any CC-BY assets.

### 8. Scope warning (important)
In 3D, an open city + driving + gunplay + CQC + base building is a large project, even for a team. To keep it achievable:
- Use a **small, dense city** (about 6â€“10 blocks), not a big one. GTA-style size is out of reach.
- Use **low-poly free/CC0 assets** (Kenney, Quaternius, KayKit) and **Mixamo animations**.
- Build in **milestones**, each one playable. Don't build systems in isolation.

---

## Part 2: Implementation roadmap

### Milestone 0: Setup
- **Project location**: `C:\Users\veron\OneDrive\Skrivbord\hurman\`. The folder is currently empty or missing, so Claude creates it. All planning goes in `hurman/docs/`: this plan becomes `docs/PLAN.md`, the concept becomes `docs/GAME_DESIGN.md`, and future plans go in `docs/plans/`.
- **Version control**: `git init`, add a Godot `.gitignore` (excluding `.godot/` and `*.import` caches) and `.gitattributes` with Git LFS for `.glb`, `.fbx`, `.png`, `.wav` and `.ogg`, so large assets don't bloat the repo. Set the remote to `https://github.com/Rufsing/hurman`.
- **Uploading versions**: commit after each completed step and push to `main`. Tag milestones (`v0.1-slice`, â€¦). The first push is the docs only. `gh` 2.103 and git 2.51 are installed. If `gh auth status` shows no login, the user runs `! gh auth login` once.
- **OneDrive caveat**: OneDrive syncing a `.git` folder and the `.godot/` cache can cause conflicts or locks. If problems appear, pause OneDrive sync for that folder. GitHub is the real backup anyway.
- **User action**: install **Godot 4.5 stable (standard, GDScript)** from godotengine.org. Claude will give exact steps and can then call the Godot executable for headless checks.

### Milestone 1: Vertical slice (first deliverable)
1. **Concept doc** written to `docs/GAME_DESIGN.md` (Part 1, expanded).
2. **Project structure**
   ```
   scenes/{player,vehicles,npc,world,base,ui,missions}
   scripts/autoload/  GameState.gd (money, cred, heat, upgrades, save/load)
                       ContractManager.gd, EventBus.gd (global signals)
   resources/         WeaponData, ContractData, UpgradeData (.tres custom Resources)
   assets/            imported low-poly models and animations
   ```
3. **Player controller**: a third-person CharacterBody3D with a SpringArm3D camera, walk, sprint, crouch, and an over-the-shoulder aim mode.
4. **Weapons**: hitscan pistol and SMG driven by `WeaponData` resources (damage, fire rate, recoil, magazine size), ammo, reloading.
5. **Gun-fu CQC (the slice's "wow" feature)**: a melee strike, a stealth takedown from behind, and one close-range gun-fu move (grab â†’ point-blank shot) using Mixamo animations. Include a Focus meter with a short slow-motion effect and enemy ragdolls.
6. **NPCs**: civilians who wander and flee; guards with a state machine (patrol â†’ suspicious â†’ alert â†’ combat) using NavigationAgent3D.
7. **Vehicle**: VehicleBody3D car with enter/exit; NPC-parked cars can be stolen (adds heat).
8. **World**: one small city block built from modular low-poly kit pieces.
9. **Home base**: a run-down basement interior with a laptop/contract board, a bed (save game and advance the day), and one upgrade station (Armory tier 1) to prove the upgrade loop.
10. **One assassination contract**: the target has a simple 2-location schedule, then payout plus cred, and heat if there were witnesses.
11. **HUD**: health, ammo, money, cred, heat stars, mission objective.
12. **Save/load**: GameState serialized to JSON in `user://`.

### Later milestones (outline)
- M2: Destruction and shakedown contracts (fear meter), heat and police response, Blood Marks currency, and the neutral-ground safehouse hotel with services.
- M2.5: Expanded gun-fu (throws, disarms, environment kills, armored enemies), the tailor (armored suits).
- M3: Base expansion (buy floors, gun range, garage, storage, med bay), car purchase and tuning.
- M4: More city districts, fixers, day/night cycle, target schedules, injuries.
- M5: Polish: audio, VFX, menus, balancing.

### Key architecture choices
- **Autoload singletons** for global state, so systems talk through `EventBus` signals and stay decoupled.
- **Data-driven Resources** for weapons, contracts and upgrades, so new content means a new `.tres` file instead of new code.
- **Component nodes** (HealthComponent, Interactable) that are reused by player, NPCs, cars and base stations.

---

## Verification
- After each milestone step, Claude runs the project headless (`godot --headless --check-only` / script parse checks), and the user launches the game to playtest.
- Vertical slice "done" test: start in the basement â†’ accept the contract on the laptop â†’ walk out â†’ steal or use a car â†’ drive to the target â†’ eliminate the target (gun or takedown) â†’ escape â†’ return home â†’ see money and cred go up â†’ buy Armory tier 1 â†’ save, quit, reload, and confirm progress is kept.
