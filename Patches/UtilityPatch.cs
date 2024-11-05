using HarmonyLib;
using System.Reflection.Emit;

namespace SafeLightningUpdated.Patches;

static class UtilityPatch {

    static bool GetStrikeStatus() {
        bool allow = ModEntry.Config.ModEnabled ? ModEntry.Config.DisableStrikes : false;
        return allow;
    }

    static bool GetModStatus() {
        return ModEntry.Config.ModEnabled;
    }

    public static IEnumerable<CodeInstruction> PerformLightningUpdate_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
        var strikeStatusMethod = AccessTools.Method(typeof(UtilityPatch), nameof(GetStrikeStatus));
        var modStatusMethod = AccessTools.Method(typeof(UtilityPatch), nameof(GetModStatus));

        var matcher = new CodeMatcher(instructions, generator);

        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldloc_1),
            new CodeMatch(OpCodes.Ldfld),
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Callvirt),
            new CodeMatch(OpCodes.Ldfld),
            new CodeMatch(OpCodes.Ldstr, "(O)787")
        ).ThrowIfNotMatch("Couldn't find match for lightning rod loop start");

        matcher.MatchStartForward(new CodeMatch(OpCodes.Ret)
        ).ThrowIfNotMatch("Couldn't find match for lightning rod loop return");

        matcher.CreateLabel(out Label rodReturnLabel);

        matcher.MatchStartBackwards(new CodeMatch(OpCodes.Ldloc_1)
        ).ThrowIfNotMatch("Couldn't find match for lightning rod strike event fire");

        matcher.InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, strikeStatusMethod),
            new CodeInstruction(OpCodes.Brtrue, rodReturnLabel)
        );

        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Ldc_I4_1),
            new CodeMatch(OpCodes.Callvirt),
            new CodeMatch(OpCodes.Ldloc_2)
        ).ThrowIfNotMatch("Couldn't find match for skip fruit tree struck countdown location");

        matcher.CreateLabel(out Label skipStruckLabel);

        matcher.MatchStartBackwards(
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Ldfld),
            new CodeMatch(OpCodes.Ldc_I4_4),
            new CodeMatch(OpCodes.Callvirt)
        ).ThrowIfNotMatch("Couldn't find match for fruit tree struck countdown");

        matcher.InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, modStatusMethod),
            new CodeInstruction(OpCodes.Brtrue, skipStruckLabel)
        );

        matcher.MatchEndForward(
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Ldnull),
            new CodeMatch(OpCodes.Ldc_I4_S),
            new CodeMatch(OpCodes.Ldloc_S)
        ).ThrowIfNotMatch("Couldn't find match for after terrain damage location");

        matcher.CreateLabel(out Label afterDamageLabel);

        matcher.MatchStartBackwards(new CodeMatch(OpCodes.Ldc_I4_S)
        ).ThrowIfNotMatch("Couldn't find match for terrain damage amount");

        matcher.InsertAndAdvance(
            new CodeMatch(OpCodes.Ldc_I4_0),
            new CodeInstruction(OpCodes.Call, modStatusMethod),
            new CodeInstruction(OpCodes.Brtrue, afterDamageLabel),
            new CodeMatch(OpCodes.Pop)
        );

        matcher.MatchEndForward(
            new CodeMatch(OpCodes.Ldloc_S),
            new CodeMatch(OpCodes.Callvirt),
            new CodeMatch(OpCodes.Pop),
            new CodeMatch(OpCodes.Ldloc_2)
        ).ThrowIfNotMatch("Couldn't find match for remove terrain feature skip location");

        matcher.CreateLabel(out Label terrainSkipLabel);

        matcher.MatchStartBackwards(new CodeMatch(OpCodes.Ldloc_1)
        ).ThrowIfNotMatch("Couldn't find match for remove terrain feature");

        matcher.InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, modStatusMethod),
            new CodeInstruction(OpCodes.Brtrue, terrainSkipLabel)
        );

        matcher.MatchEndForward(
            new CodeMatch(OpCodes.Ret)
        ).ThrowIfNotMatch("Couldn't find match for skip terrain feature strike event fire location");

        matcher.CreateLabel(out Label featureReturnLabel);

        matcher.MatchStartBackwards(new CodeMatch(OpCodes.Ldloc_1)
        ).ThrowIfNotMatch("Couldn't find match for terrain feature strike event start");

        matcher.InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, strikeStatusMethod).MoveLabelsFrom(matcher.Instruction),
            new CodeInstruction(OpCodes.Brtrue, featureReturnLabel)
        );

        matcher.MatchEndForward(
            new CodeMatch(OpCodes.Ret)
        ).ThrowIfNotMatch("Couldn't find match for skip small flash strike event location");

        matcher.CreateLabel(out Label smallReturnLabel);

        matcher.MatchStartBackwards(new CodeMatch(OpCodes.Ldloc_1)
        ).ThrowIfNotMatch("Couldn't find match for small flash strike event");

        matcher.InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, strikeStatusMethod),
            new CodeInstruction(OpCodes.Brtrue, smallReturnLabel)
        );

        return matcher.InstructionEnumeration();
    }

}