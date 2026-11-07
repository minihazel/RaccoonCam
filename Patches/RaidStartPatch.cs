using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using hazelify.RaccoonCam.Scripts;

namespace hazelify.RaccoonCam.Patches
{
    // here's the main raid start patch
    // where we assign the ControllerInstance
    // so that the CameraController can do its
    // work on the player's main camera

    public class RaidStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        static void Postfix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null) return;

            Plugin.ControllerInstance = gameWorld.gameObject.AddComponent<CameraController>();
        }
    }
}
