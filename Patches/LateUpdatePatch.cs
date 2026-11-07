using Comfort.Common;
using EFT;
using EFT.CameraControl;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace hazelify.RaccoonCam.Patches
{
    // here's our patching of the first person pov
    // camera state script, where we patch
    // ManualLateUpdate so that we can override it
    // with the camera controller;

    // we want to ensure it doesn't do any fvckery
    // with our camera

    public class CameraStatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(FirstPersonCameraState), nameof(FirstPersonCameraState.ManualLateUpdate));
        }

        [PatchPrefix]
        static bool PatchPrefix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null) return true;

            return !(Plugin.IsCameraActive || Plugin.IsHelmetActive);
        }
    }
}
