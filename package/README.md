# NoTerrainPenalty

**Stop losing melee damage when your weapon clips the ground, a wall or a rock.**

In vanilla Valheim, a melee swing that hits more than one target deals less damage. The game counts everything the swing touches, so one enemy plus the ground or a wall counts as 2 targets and takes only about 67% damage. This mod counts only creatures, so a clean hit on one enemy does full damage again.

## What it does

- Only creatures (players, mobs and bosses) count toward the multi-hit penalty. Hitting several enemies in one swing keeps the vanilla penalty (about 67% each for 2, 44% for 3).
- Works on every player melee attack that has the penalty, including weapons from other mods.
- Everything else stays vanilla. Hitting 2 trees in one swing is still reduced, and fists, bows, magic and monster attacks are untouched.

## Config

The file is `BepInEx/config/onononoki.NoTerrainPenalty.cfg`. Settings are synced from the server and only admins can change them in multiplayer. Changes apply right away.

| Setting | Default | What it does |
|---|---|---|
| `Enabled` | true | Turns the mod on or off |
| `PrimaryAttack` | true | Apply to primary attacks |
| `SecondaryAttack` | true | Apply to secondary attacks |
| `ExcludedSkills` | (empty) | Weapon skill types to leave vanilla, for example `Axes, Pickaxes` |
| `ExcludedItems` | (empty) | Item prefab names to leave vanilla, for example `THSwordKrom` |

## Installation and compatibility

- Use a mod manager, or put `NoTerrainPenalty.dll` into `BepInEx/plugins`. Requires BepInExPack Valheim and Jotunn.
- For multiplayer, install it on the server and on every player. Players without the mod cannot join a server that has it, and the other way around.
- Safe to add or remove on an existing world. Nothing is saved.
- If a mod such as WackysDatabase turns the penalty off for a weapon (`Lower_Dmg_Per_Hit`), it stays off.
- If another mod rewrites `Attack.DoMeleeAttack`, the log shows `could not find the multi-hit penalty` and this mod stays inactive.

## Feedback and bug reports

Found a bug or have an idea? Open an issue on [GitHub](https://github.com/Ononononoki/NoTerrainPenalty/issues). The source code is in the [GitHub repository](https://github.com/Ononononoki/NoTerrainPenalty).
