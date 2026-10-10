# NoTerrainPenalty

**Tired of your weapon clipping the ground, a wall or a rock and losing a big chunk of its damage in dungeons or on uneven terrain? This mod solves it.**

In vanilla Valheim, many melee attacks lower their damage when one swing hits more than one target. The game counts everything the swing touches, not just enemies. So one enemy plus the ground, a dungeon wall, a rock or a tree counts as 2 targets, and the enemy takes only about 67% of the damage.

This mod makes only creatures count. A clean hit on one enemy does full damage again, wherever you fight.

## What it changes

- Only creatures (players, mobs and bosses) count toward the multi-hit damage penalty.
- The ground, walls, rocks, trees and building pieces no longer lower your damage to enemies.
- Hitting several enemies in one swing still uses the vanilla penalty (about 67% each for 2 enemies, 44% for 3), so the balance stays vanilla.
- Works on every player melee weapon attack that has the penalty, including weapons added by other mods. No per-weapon setup.

## What it does not change

- Damage to objects. Chopping and mining work exactly as in vanilla, so hitting 2 trees in one swing is still reduced.
- Which targets a swing can reach, hit effects and stagger.
- Damage values, stamina and attack speed.
- Attacks that never had the penalty in vanilla.
- Fists, torches, bows, crossbows, magic and monster attacks.

## Config

The file is `BepInEx/config/onononoki.NoTerrainPenalty.cfg`. Settings are synced from the server to every player, and only admins can change them in multiplayer.

| Setting | Default | What it does |
|---|---|---|
| `Enabled` | true | Turns the mod on or off |
| `PrimaryAttack` | true | Apply to primary attacks |
| `SecondaryAttack` | true | Apply to secondary attacks |
| `ExcludedSkills` | (empty) | Weapon skill types to leave vanilla, for example `Axes, Pickaxes` |
| `ExcludedItems` | (empty) | Item prefab names to leave vanilla, for example `THSwordKrom` |

Changes apply right away. No restart is needed.

## Installation

- Use a mod manager (r2modman or Thunderstore Mod Manager), or put `NoTerrainPenalty.dll` into `BepInEx/plugins`.
- For multiplayer, install it on the server and on every player. Players without the mod cannot join a server that has it, and players with the mod cannot join a server without it.
- Requires BepInExPack Valheim and Jotunn.

## Compatibility

- Safe to add to or remove from an existing world. Nothing is saved to the world or character. Removing the mod brings back vanilla behavior.
- Works with weapons added by other mods, such as ValheimArmory.
- Respects per-weapon settings from other mods. If a mod such as WackysDatabase turns the penalty off for a weapon (`Lower_Dmg_Per_Hit`), it stays off.
- If another mod rewrites the same part of the game code, the log shows `could not find the multi-hit penalty` and this mod stays inactive. The game then plays as vanilla.

## How it works

A Harmony transpiler on `Attack.DoMeleeAttack` replaces the target count used in the multi-hit penalty. When the hit target is a creature, the count is the number of creatures in the swing. When it is an object, the vanilla count is kept.
