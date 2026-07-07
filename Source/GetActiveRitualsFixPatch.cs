using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

/*
 * GetActiveRitualsFixPatch
 *
 * Problem:
 * Vanilla IdeoManager.GetActiveRituals reuses a single instance list (activeRitualsTmp).
 * After rituals, nested calls from gizmo drawing (Precept_Ritual.ShouldShowGizmo),
 * alerts (Alert_AnimaLinkingReady), and optionally Performance Optimizer's GetGizmosFast
 * can re-enter or race on that list. List.Clear() then throws IndexOutOfRangeException
 * and InspectGizmoGrid stops drawing buttons for all buildings.
 *
 * Solution:
 * Replace GetActiveRituals with a version that always returns a fresh list, so callers
 * cannot corrupt shared scratch state. Works with or without Performance Optimizer.
 */
namespace HSK.GetActiveRitualsFixPatch
{
    public static class ModCompatibility
    {
        private const string PerformanceOptimizerPackageId = "Taranchuk.PerformanceOptimizer";

        public static bool IsPerformanceOptimizerLoaded()
        {
            return IsPackageActive(PerformanceOptimizerPackageId);
        }

        private static bool IsPackageActive(string packageId)
        {
            if (ModsConfig.IsActive(packageId))
            {
                return true;
            }

            return LoadedModManager.RunningModsListForReading.Exists(
                mod => string.Equals(mod.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class GetActiveRitualsFixPatchMod : Mod
    {
        private const string HarmonyId = "kebabebak.get.active.rituals.fix.patch";

        public GetActiveRitualsFixPatchMod(ModContentPack content)
            : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(ApplyPatches);
        }

        private static void ApplyPatches()
        {
            if (!ModsConfig.IdeologyActive)
            {
                return;
            }

            try
            {
                MethodBase target = AccessTools.Method(typeof(IdeoManager), nameof(IdeoManager.GetActiveRituals));
                if (target == null)
                {
                    Log.Warning("[GetActiveRitualsFixPatch] IdeoManager.GetActiveRituals not found; patch skipped.");
                    return;
                }

                new Harmony(HarmonyId).Patch(
                    target,
                    prefix: new HarmonyMethod(
                        typeof(IdeoManager_GetActiveRituals_Patch),
                        nameof(IdeoManager_GetActiveRituals_Patch.Prefix)));

                Log.Message(
                    "[GetActiveRitualsFixPatch] Loaded " +
                    $"(Performance Optimizer={(ModCompatibility.IsPerformanceOptimizerLoaded() ? "ON" : "OFF")}). " +
                    "GetActiveRituals now returns an isolated list (fixes missing building gizmos after rituals).");
            }
            catch (Exception ex)
            {
                Log.Error("[GetActiveRitualsFixPatch] Failed to apply patches: " + ex);
            }
        }
    }

    internal static class IdeoManager_GetActiveRituals_Patch
    {
        public static bool Prefix(Map map, ref List<LordJob_Ritual> __result)
        {
            __result = ActiveRitualCollector.Collect(map);
            return false;
        }
    }

    internal static class ActiveRitualCollector
    {
        public static List<LordJob_Ritual> Collect(Map map)
        {
            var result = new List<LordJob_Ritual>();
            if (map == null)
            {
                return result;
            }

            LordManager lordManager = map.lordManager;
            if (lordManager == null)
            {
                return result;
            }

            List<Lord> lords = lordManager.lords;
            for (int i = 0; i < lords.Count; i++)
            {
                Lord lord = lords[i];
                if (lord?.LordJob is LordJob_Ritual ritual)
                {
                    result.Add(ritual);
                }
            }

            return result;
        }
    }
}
