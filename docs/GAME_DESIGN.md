# HURMAN: Game Design Document
*Working title. Living document, last updated 2026-11-01.*

## 1. Pitch
**Fredrick "Fred" Hale**, a retired mercenary, rebuilds his life from a rented basement in the rain-soaked neon harbor city of **Port Vesper**. He takes contracts from **the Ledger**, the city's hidden underworld society, fights with brutal close-range **gun-fu**, and slowly buys the whole building he lives in, turning it into a fortress. Break the Ledger's rules and you are **Struck**: struck from the Ledger, and the whole city comes for you.

- **Genre**: third-person action / open-world contract shooter
- **Engine**: Godot 4.7 .NET (C#), 3D low-poly with a stylized neon-noir look

## 1b. Names and cast
| Thing | Name | Notes |
|---|---|---|
| Protagonist | **Fredrick "Fred" Hale** | Everyone calls him Fred. Ex-Ledger operative who walked away. An old Vow pulls him back. (The surname "Hale" is a suggestion.) |
| City | **Port Vesper** | Rain-soaked harbor city, always night-ish |
| Districts | **Lowmarket** (home), **the Spires** (downtown), **Saltline Docks**, **Neon Mile** (nightlife), **the Kilnworks** (industrial) | The slice is set in Lowmarket |
| Underworld society | **The Ledger** | Every debt, contract and life is written down |
| Underworld currency | **Sovereigns** | Heavy gold coins stamped with a broken scale (replaces the "Sovereigns" placeholder) |
| Neutral ground | **The Halcyon** | Faded grand hotel on the harbor. No business on Halcyon grounds |
| Outcast state | **Struck** | "Struck from the Ledger": bounty, services closed (replaces "Struck") |
| Favors / markers | **Vows** | A card sealed with a bloody thumbprint. A Vow must be honored |
| Fixer: quiet jobs | **Mother Ines** | Runs a flower shop. Precise, patient, pays well for clean work |
| Fixer: loud jobs | **Dutch Varga** | Scrapyard owner. Wants things blown up |
| Fixer: street jobs | **Saint** | Young gang broker. Shakedowns and turf work |
| The Cleaner | **Mr. Pell** | Body and evidence disposal (−heat) |
| The Doctor | **Doc Ashby** | Patches injuries, no questions |
| The Tailor | **Ottoline Marsh** | Armored suits |
| The Armorer | **Kasimir** | Rare guns and mods |
| The Archivist | **Wren** | Sells target dossiers and intel |
| Landlord | **Mrs. Dobrev** | Owns Fred's building. You buy the floors from her |
| First target | **Viktor Lusk** | A loan shark working out of the Neon Swan bar, across the street from Fred
- **Inspirations (feel only)**: gun-fu action films, Hitman (contract planning), GTA 1/2-era open world (small dense city)
- **Player**: solo, single-player

## 2. Pillars
1. **Gun-fu is the core.** Fights are fast, close, physical and stylish. Shooting and melee blend together.
2. **Every job is a choice.** Plan quiet or go loud. Cred against heat is always the trade-off.
3. **Your home grows with you.** The base visibly goes from a run-down basement to an owned fortress, and each upgrade changes how you play.
4. **The underworld has rules.** Services, currency and consequences make the world feel alive and dangerous.

## 3. Core loop
```
Contract board / fixer call
        ↓
Prep: loadout · car · intel · disguise
        ↓
Mission: infiltrate · eliminate/destroy/shake down
        ↓
Escape: lose heat, ditch the car, dispose of evidence
        ↓
Payout: money + Sovereigns + street cred (− heat)
        ↓
Spend: base upgrades · weapons · suits · cars · services
        ↓
Higher cred → better contracts, new fixers
```

## 4. Currencies and meters
| Name | What it is | Gained by | Spent on / effect |
|---|---|---|---|
| **Money** | Regular cash | Contracts, fencing loot | Base renovations, cars, guns, ammo |
| **Sovereigns** | Underworld gold tokens | Contracts, bonus objectives | Services: cleanup, doctor, tailor, intel, safehouse rooms |
| **Street Cred** | Reputation | Clean, stylish, fast jobs | Unlocks contract tiers, fixers, vendors |
| **Heat** | Police and gang attention | Witnesses, civilian harm, stolen cars, evidence | Patrols, checkpoints, eventually a raid on your base. Fades over days or with cleanup services |

## 5. Combat: gun-fu
- **Shooting**: over-the-shoulder aim, hitscan weapons, meaningful recoil, limited magazines, deliberate reloads.
- **Close-range context moves**: when an enemy is within about 2 m, the fire button becomes a gun-fu move:
  - Grab → point-blank shot
  - Throw → shot on the downed enemy
  - Disarm → shoot with *their* gun
  - Human shield
- **Melee**: strikes, stealth takedowns from behind, non-lethal knockouts (needed for shakedowns).
- **Focus meter**: chained kills fill Focus. Spend it on a brief slow-motion, an instant reload, or a guaranteed next headshot.
- **Armor**: your tailored suits soak damage. Heavy-armor enemies need headshots or finishers.
- **Environment**: throwable objects, breakable glass, railings, car doors as cover or weapons.
- **Hit reactions**: staggers, limps, ragdolls.

## 6. Contracts
| Type | Core mechanic | Bonus objectives |
|---|---|---|
| **Assassination** | The target follows a daily schedule. Choose when and where | Accident kill, knife only, no witnesses, under a time limit |
| **Destruction** | Find weak points, plant charges, or sabotage the gas or power | Zero casualties, detonate from range, no alarms |
| **Shakedown** | Fear meter: aim, break things, grab, threaten. Too much and they panic, too little and they refuse | Turn the target into a recurring informant |
| *Later:* extraction, retrieval, tailing, sabotage, base defense | | |

Contracts come from **fixers** (2–3 handlers with different styles: quiet, loud, gang). Your relationship with each one unlocks unique gear and story jobs.

## 7. The underworld society (original lore, to be named)
- **Neutral ground**: a hidden hotel/club where violence is forbidden. You rest, buy services, and meet fixers there.
- **Services** (paid in Sovereigns): the **Cleaner** (body/evidence disposal, −heat), the **Doctor** (heals injuries), the **Tailor** (armored suits), the **Armorer** (rare guns and mods), the **Archivist** (target intel).
- **Markers**: favors owed between people. Call one in, or be forced to honor one, which triggers story contracts.
- **Struck**: break a rule (violence on neutral ground, betraying a fixer) and you get a bounty. Services close, hunters come after you, and you must survive or clear your name. This drives the endgame arc.

## 8. Home base progression
| Stage | Space | Unlocks |
|---|---|---|
| 1 | **Rented basement** (run-down) | Bed (save, sleep), laptop (contracts), weapon locker |
| 2 | Basement repaired | **Armory**, **Storage** |
| 3 | **Buy ground floor** | **Gun range** (skill training), **Garage** (store and tune cars, swap plates) |
| 4 | **Buy upper floors** | **Intel room**, **Med bay**, **Tailor corner** |
| 5 | **Own the building + roof** | Rooftop sniper range, defenses for base raids |
Repairs and renovations (lighting, walls, furniture) are visible and add small comfort buffs.

## 9. Vehicles
- Your own car can be bought, upgraded and stored. Stolen cars are free but add heat.
- Upgrades cover engine, armor, tires and plates (swapping lowers heat).
- Police chases: break line of sight, then ditch or swap the car.

## 10. World
- One **small, dense city** of about 6–10 blocks: home district, downtown, docks, nightlife strip, industrial area.
- **Day/night cycle**: target schedules and guard alertness change with the time of day.
- **Vibe**: night, rain, neon, wet reflective streets, volumetric fog, synth/techno music.

## 11. Niche mechanics
- **Injuries**: persist until treated (arm wound → more sway, leg wound → slower).
- **Disguises**: uniforms grant access to restricted areas.
- **Evidence**: shell casings, a car seen at the scene, and cameras raise heat unless cleaned up.

## 12. Story hook
A retired merc is pulled back into the life and starts over in a basement. Who forced them back, and why, unfolds through fixer contracts and markers.

## 13. Art and audio
- **Models**: Kenney, Quaternius, KayKit (CC0), Poly Pizza, Sketchfab (CC0/CC-BY)
- **Animation**: Mixamo, Quaternius animation library
- **Audio**: freesound.org, Sonniss GDC bundles, OpenGameArt
- **Textures/HDRI**: Poly Haven, ambientCG
- Licenses are tracked in `assets/CREDITS.md`.

## 14. Scope and milestones
See `docs/PLAN.md`. Each milestone is playable.
- **M1 Vertical slice**: basement, one city block, a car, one assassination with gun-fu basics
- **M2**: destruction and shakedown contracts, heat and police, Sovereigns, neutral-ground hotel
- **M2.5**: expanded gun-fu, tailor
- **M3**: base expansion, garage, car tuning
- **M4**: more districts, fixers, day/night, injuries
- **M5**: polish

## 15. Open questions
- Name of the city, the underworld society, the currency, and the protagonist
- Protagonist gender/appearance (customizable?)
- Controller support from the start? (recommended: yes)
