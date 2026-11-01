# 2026-11-01: Names, C#, first greybox

## Decisions
- **Engine/language**: Godot **4.7.2 .NET**, coding in **C#** (.NET 8). Editor at `C:\Program Files (x86)\Godot\`.
- **Protagonist**: **Fredrick Hale**, called **Fred**.
- **Names**: Port Vesper (city), the Ledger (underworld society), Sovereigns (gold coins), the Halcyon (neutral-ground hotel), Struck (outcast state), Vows (favors). Fixers: Mother Ines, Dutch Varga, Saint. The full cast is in `GAME_DESIGN.md` §1b.

## Built (M1, steps 1–3 + part of 9–12)
- Project skeleton: `project.godot`, `Hurman.csproj`, Jolt physics, physics layers
- Autoloads: `EventBus` (signals + input map), `GameState` (money/Sovereigns/cred/heat/day/upgrades, JSON save), `ContractManager` (loads `data/contracts/*.tres`)
- Data resources: `ContractData`, `WeaponData`, `UpgradeData`; component `HealthComponent`
- `PlayerController`: third-person movement, sprint, crouch, jump, over-the-shoulder aim camera, gamepad support
- Interactables: laptop (Ledger contracts), mattress (sleep/save), armory upgrade station, contract target (placeholder takedown)
- `GreyboxCity`: Fred's basement + stairwell + ground floor, one Lowmarket street, the Neon Swan bar, neon/fog/wet-street lighting
- HUD: stats, objective, prompts, toasts, crosshair
- First contract: **"Last Call at the Neon Swan"** (target Viktor Lusk, fixer Mother Ines)

## Playable loop right now
Basement → laptop (accept contract) → up the stairs → across the street → take down Viktor (E) → +$2,000, +2 Sovereigns, +10 cred, +1 heat → back home → renovate the Armory ($1,500) → sleep (saves).

## Next
1. Weapons: pistol and SMG (`WeaponData` .tres), hitscan, recoil, reloading, ammo HUD
2. Gun-fu CQC: melee, stealth takedown, grab → point-blank shot, Focus meter + slow-mo
3. NPCs: guards (patrol/alert/combat), civilians who flee; Viktor gets bodyguards
4. Drivable car (VehicleBody3D), stealing parked cars
5. Swap greybox for low-poly assets (Kenney/KayKit/Quaternius) + Mixamo animations for Fred
