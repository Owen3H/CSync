using CSync.Lib;
using HarmonyLib;

namespace CSync.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class LeavePatch {
    [HarmonyPostfix]
    [HarmonyPatch("StartDisconnect")]
    private static void RevertOnDisconnect() {
        ConfigManager.RevertSyncedInstances();
    }
}