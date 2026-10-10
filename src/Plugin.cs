using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;

namespace NoTerrainPenalty
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency("com.jotunn.jotunn", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "onononoki.NoTerrainPenalty";
        public const string ModName = "NoTerrainPenalty";
        public const string ModVersion = "1.0.1";

        internal static ManualLogSource Log;

        private static ConfigEntry<bool> _enabled;
        private static ConfigEntry<bool> _primary;
        private static ConfigEntry<bool> _secondary;
        private static ConfigEntry<string> _excludedSkills;
        private static ConfigEntry<string> _excludedItems;

        // parsed filter lists, rebuilt on every config change
        private static HashSet<string> _skipSkills = new HashSet<string>();
        private static HashSet<string> _skipItems = new HashSet<string>();

        private static readonly HashSet<ItemDrop.ItemData.ItemType> WeaponTypes = new HashSet<ItemDrop.ItemData.ItemType>
        {
            ItemDrop.ItemData.ItemType.OneHandedWeapon,
            ItemDrop.ItemData.ItemType.TwoHandedWeapon,
            ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft
        };

        // private game fields
        private static readonly AccessTools.FieldRef<Attack, Humanoid> AttackCharacter =
            AccessTools.FieldRefAccess<Attack, Humanoid>("m_character");
        private static readonly AccessTools.FieldRef<Attack, ItemDrop.ItemData> AttackWeapon =
            AccessTools.FieldRefAccess<Attack, ItemDrop.ItemData>("m_weapon");
        private static readonly AccessTools.FieldRef<Humanoid, bool> CurrentAttackIsSecondary =
            AccessTools.FieldRefAccess<Humanoid, bool>("m_currentAttackIsSecondary");

        private void Awake()
        {
            Log = Logger;
            var sync = new ConfigurationManagerAttributes { IsAdminOnly = true };

            _enabled = Config.Bind("General", "Enabled", true,
                new ConfigDescription("Only creatures (players, mobs, bosses) count toward the melee multi-hit damage penalty. Ground, walls, rocks, trees and building pieces no longer lower damage to creatures.", null, sync));
            _primary = Config.Bind("General", "PrimaryAttack", true,
                new ConfigDescription("Apply to primary attacks.", null, sync));
            _secondary = Config.Bind("General", "SecondaryAttack", true,
                new ConfigDescription("Apply to secondary attacks.", null, sync));
            _excludedSkills = Config.Bind("Filters", "ExcludedSkills", "",
                new ConfigDescription("Comma separated weapon skill types to leave vanilla (Swords, Knives, Clubs, Polearms, Spears, Axes, Pickaxes).", null, sync));
            _excludedItems = Config.Bind("Filters", "ExcludedItems", "",
                new ConfigDescription("Comma separated item prefab names to leave vanilla (example: THSwordKrom, SwordIron).", null, sync));

            // rebuild the filter lists when any setting changes, including server sync from jotunn
            ParseFilters();
            Config.SettingChanged += (s, e) => ParseFilters();

            Harmony.CreateAndPatchAll(typeof(MeleePatch), ModGuid);

            Logger.LogInfo($"{ModName} {ModVersion} loaded");
        }

        private static void ParseFilters()
        {
            _skipSkills = ParseList(_excludedSkills.Value);
            _skipItems = ParseList(_excludedItems.Value);
        }

        private static HashSet<string> ParseList(string value) =>
            new HashSet<string>((value ?? "").Split(',').Select(s => s.Trim()).Where(s => s.Length > 0),
                StringComparer.OrdinalIgnoreCase);

        // true when this attack should use the creatures-only count
        internal static bool Applies(Attack attack)
        {
            if (!_enabled.Value) return false;
            if (!(AttackCharacter(attack) is Player player)) return false;

            var weapon = AttackWeapon(attack);
            if (weapon?.m_shared == null || !WeaponTypes.Contains(weapon.m_shared.m_itemType)) return false;
            if (!(CurrentAttackIsSecondary(player) ? _secondary.Value : _primary.Value)) return false;
            if (_skipSkills.Contains(weapon.m_shared.m_skillType.ToString())) return false;
            if (_skipItems.Count == 0) return true;

            var prefab = weapon.m_dropPrefab;
            return prefab == null || !_skipItems.Contains(prefab.name);
        }
    }
}
