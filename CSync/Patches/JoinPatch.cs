using CSync.Lib;
using GameNetcodeStuff;
using HarmonyLib;

namespace CSync.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
internal class JoinPatch {
    [HarmonyPostfix]
    [HarmonyPatch("ConnectClientToPlayerObject")]
    private static void SyncOnJoin() {
        ConfigManager.SyncInstances();
    }
}
