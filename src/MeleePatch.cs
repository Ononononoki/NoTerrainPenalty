using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace NoTerrainPenalty
{
    // vanilla: if (m_multiHit && m_lowerDamagePerHit && hits.Count > 1) factor /= hits.Count * 0.75f
    // patched: both hits.Count reads become CountForPenalty(hits, hitObject, this)
    [HarmonyPatch(typeof(Attack), "DoMeleeAttack")]
    public static class MeleePatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            var goField = AccessTools.Field(typeof(Attack.HitPoint), nameof(Attack.HitPoint.go));
            var lowerField = AccessTools.Field(typeof(Attack), nameof(Attack.m_lowerDamagePerHit));
            var getCount = AccessTools.PropertyGetter(typeof(List<Attack.HitPoint>), nameof(List<Attack.HitPoint>.Count));
            var helper = AccessTools.Method(typeof(MeleePatch), nameof(CountForPenalty));

            // local that holds the object of the hit being processed, and the penalty check
            CodeInstruction goStore = null;
            int lowerAt = -1;
            for (int i = 0; i < code.Count - 1 && lowerAt < 0; i++)
            {
                if (code[i].LoadsField(goField) && code[i + 1].IsStloc()) goStore = code[i + 1];
                if (code[i].LoadsField(lowerField)) lowerAt = i;
            }

            // the two count reads right after the m_lowerDamagePerHit check
            var countAt = new List<int>();
            for (int i = lowerAt + 1; lowerAt >= 0 && i < code.Count && countAt.Count < 2; i++)
                if (code[i].Calls(getCount)) countAt.Add(i);

            if (goStore == null || countAt.Count != 2 || countAt[1] - lowerAt > 12)
            {
                Plugin.Log.LogError("could not find the multi-hit penalty in Attack.DoMeleeAttack (game update?), mod inactive");
                return code;
            }

            // later site first so the earlier index stays valid
            foreach (int i in new[] { countAt[1], countAt[0] })
            {
                var loadGo = LoadLocal(goStore).MoveLabelsFrom(code[i]);
                code[i] = new CodeInstruction(OpCodes.Call, helper);
                code.InsertRange(i, new[] { loadGo, new CodeInstruction(OpCodes.Ldarg_0) });
            }

            Plugin.Log.LogInfo("patched the multi-hit penalty in Attack.DoMeleeAttack");
            return code;
        }

        // creature hits divide by the number of creatures in the swing, everything else keeps the vanilla count
        public static int CountForPenalty(List<Attack.HitPoint> hits, GameObject hitObject, Attack attack)
        {
            if (!IsCreature(hitObject) || !Plugin.Applies(attack)) return hits.Count;
            return hits.Count(h => IsCreature(h.go));
        }

        private static bool IsCreature(GameObject go) => go != null && go.GetComponent<Character>() != null;

        // ldloc that reads the local a stloc writes
        private static CodeInstruction LoadLocal(CodeInstruction store)
        {
            if (store.opcode == OpCodes.Stloc_0) return new CodeInstruction(OpCodes.Ldloc_0);
            if (store.opcode == OpCodes.Stloc_1) return new CodeInstruction(OpCodes.Ldloc_1);
            if (store.opcode == OpCodes.Stloc_2) return new CodeInstruction(OpCodes.Ldloc_2);
            if (store.opcode == OpCodes.Stloc_3) return new CodeInstruction(OpCodes.Ldloc_3);
            return new CodeInstruction(OpCodes.Ldloc, store.operand);
        }
    }
}
