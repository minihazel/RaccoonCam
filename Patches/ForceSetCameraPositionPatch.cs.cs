using System.Reflection;
using EFT.CameraControl;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace hazelify.RaccoonCam.Patches
{
    // overriding the CameraManager's ForceSetPosition
    // function with a patch depending on whether
    // our camera controller script is active;

    // we want to ensure it doesn't do any fvckery
    // with our camera

    public class ForceSetCameraPositionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(CameraManager), nameof(CameraManager.ForceSetPosition));
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            if (Plugin.ControllerInstance == null)
            {
                return true;
            }

            return !(Plugin.ControllerInstance.IsCameraActive || Plugin.ControllerInstance.IsHelmetActive);
        }
    }
}
