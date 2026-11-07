using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Bootstrap;
using UnityEngine;
using hazelify.RaccoonCam.Patches;
using JetBrains.Annotations;
using System.ComponentModel;
using hazelify.RaccoonCam.Scripts;

namespace hazelify.RaccoonCam
{
    [BepInPlugin("hazelify.RaccoonCam", "RaccoonCam", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool isSOInstalled = Chainloader.PluginInfos.ContainsKey("com.fontaine.spt-stance-overhaul");

        // list for ropdown for helmetcam presets
        public enum HelmetCamTypes
        {
            [Description("Left side")]
            Left,
            [Description("Right side")]
            Right,
            [Description("Helmet top")]
            Top,
            [Description("Above eyes")]
            AboveEyes
        }

        // list for dropdown for the crosshair styles
        public enum Crosshairs
        {
            Square,
            Plus,
            Circle,
            Traditional
        }

        // list for dropdown for the crosshair color
        public enum CrosshairColors
        {
            Red,
            Green,
            Blue,
            Purple,
            Sky,
            White,
            Black
        }

        private static string categoryBodycam = "Bodycam";
        private static string categoryHelmetcam = "Helmet cam";
        private static string categoryCrosshair = "Crosshair Settings";
        private static string categoryBodycamValues = "Bodycam Values";
        private static string categoryPresetValues = "Helmetcam Preset Values";
        private static string categoryPresetValuesTop = "Helmetcam Preset Values - Top";
        private static string categoryPresetValuesLeft = "Helmetcam Preset Values - Left";
        private static string categoryPresetValuesRight = "Helmetcam Preset Values - Right";
        private static string categoryPresetValuesAboveEyes = "Helmetcam Preset Values - Above Eyes";
        private static string categoryHotkeys = "Hotkeys";

        internal new static ManualLogSource Logger { get; private set; }

        // standard settings
        public static ConfigEntry<bool> useHotkeys;

        // helmetcam settings
        public static ConfigEntry<bool> helmetCam;
        public static ConfigEntry<KeyboardShortcut> helmetCamSwitch;
        public static ConfigEntry<bool> useHelmetcamSmoothing;
        public static ConfigEntry<bool> useHelmetcamCrosshair;
        public static ConfigEntry<HelmetCamTypes> helmetCamType;

        // bodycam settings
        public static ConfigEntry<bool> bodyCam;
        public static ConfigEntry<KeyboardShortcut> bodyCamSwitch;
        public static ConfigEntry<bool> useBodycamSmoothen;
        public static ConfigEntry<bool> useBodycamCrosshair;

        // crosshair settings
        public static ConfigEntry<Crosshairs> GlobalCrosshairStyle;
        public static ConfigEntry<CrosshairColors> GlobalCrosshairColor;

        // advanced bodycam stuff (customization)
        public static ConfigEntry<bool> UseBodycamValues;
        public static ConfigEntry<float> bodyCamX;
        public static ConfigEntry<float> bodyCamY;
        public static ConfigEntry<float> bodyCamZ;

        // advanced helmetcam stuff (preset customization)
        public static ConfigEntry<bool> UseHelmetcamValues;
        public static ConfigEntry<float> helmetPresetTop;
        public static ConfigEntry<float> helmetPresetLeft;
        public static ConfigEntry<float> helmetPresetRight;
        public static ConfigEntry<float> helmetPresetAboveEyes;

        public static ConfigEntry<float> helmetCamTopX;
        public static ConfigEntry<float> helmetCamTopY;
        public static ConfigEntry<float> helmetCamTopZ;

        public static ConfigEntry<float> helmetCamLeftX;
        public static ConfigEntry<float> helmetCamLeftY;
        public static ConfigEntry<float> helmetCamLeftZ;

        public static ConfigEntry<float> helmetCamRightX;
        public static ConfigEntry<float> helmetCamRightY;
        public static ConfigEntry<float> helmetCamRightZ;

        public static ConfigEntry<float> helmetCamAboveEyesX;
        public static ConfigEntry<float> helmetCamAboveEyesY;
        public static ConfigEntry<float> helmetCamAboveEyesZ;

        // internal variables
        public static CameraController ControllerInstance;
        public static bool IsCameraActive => ControllerInstance != null && ControllerInstance.IsCameraActive;
        public static bool IsHelmetActive => ControllerInstance != null && ControllerInstance.IsHelmetActive;

        [UsedImplicitly]
        internal void Start()
        {
            Logger = base.Logger;
            Logger.LogInfo("[ActualBodyCam] Loaded!");

            new RaidStartPatch().Enable();
            new ForceSetCameraPositionPatch().Enable();
            new CameraStatePatch().Enable();
        }

        private void Awake()
        {
            // hotkeys
            useHotkeys = Config.Bind(
                categoryHotkeys,
                "Use hotkeys?",
                false,
                new ConfigDescription("Whether to allow hotkeys or not for the mod",
                null,
                new ConfigurationManagerAttributes { Order = 5 })
            );

            // masterswitches
            helmetCam = Config.Bind(
                categoryHelmetcam,
                "Helmet camera",
                false,
                new ConfigDescription("Toggle between helmet worn camera and the normal camera",
                null,
                new ConfigurationManagerAttributes { Order = 4 })
            );

            helmetCamType = Config.Bind(
                categoryHelmetcam,
                "Helmet camera placement",
                HelmetCamTypes.Top,
                new ConfigDescription("Where the helmet camera is placed on the helmet.",
                null,
                new ConfigurationManagerAttributes { Order = 3 })
            );

            helmetCamSwitch = Config.Bind(
                categoryHelmetcam,
                "Helmet camera hotkey",
                new KeyboardShortcut(KeyCode.F7),
                new ConfigDescription("What hotkey to use for the helmet worn camera toggle.",
                null,
                new ConfigurationManagerAttributes { Order = 2 })
            );

            useHelmetcamSmoothing = Config.Bind(
                categoryHelmetcam,
                "Smoothen Helmetcam motion?",
                true,
                new ConfigDescription("RaccoonCam utilizes a motion smoothening to ensure that running with the helmetcam on isn't dizzying.\n\nEnable this if you want the chaotic default motion.",
                null,
                new ConfigurationManagerAttributes { Order = 2 })
            );

            useHelmetcamCrosshair = Config.Bind(
                categoryHelmetcam,
                "Enable crosshair for the Helmetcam?",
                true,
                new ConfigDescription("Whether to display an assisting crosshair for gameplay purposes.",
                null,
                new ConfigurationManagerAttributes { Order = 1 })
            );


            // bodycam category
            bodyCam = Config.Bind(
                categoryBodycam,
                "Body camera",
                false,
                new ConfigDescription("Toggle between body worn camera and the normal camera",
                null,
                new ConfigurationManagerAttributes { Order = 4 })
            );

            bodyCamSwitch = Config.Bind(
                categoryBodycam,
                "Body camera hotkey",
                new KeyboardShortcut(KeyCode.F8),
                new ConfigDescription("What hotkey to use for the body worn camera toggle.",
                null,
                new ConfigurationManagerAttributes { Order = 3 })
            );

            useBodycamSmoothen = Config.Bind(
                categoryBodycam,
                "Smoothen Bodycam motion?",
                true,
                new ConfigDescription("RaccoonCam utilizes a motion smoothening to ensure that running with the bodycam on isn't dizzying.\n\nEnable this if you want the chaotic default motion.",
                null,
                new ConfigurationManagerAttributes { Order = 2 })
            );

            useBodycamCrosshair = Config.Bind(
                categoryBodycam,
                "Enable crosshair for the Bodycam?",
                true,
                new ConfigDescription("Whether to display an assisting crosshair for gameplay purposes.",
                null,
                new ConfigurationManagerAttributes { Order = 1 })
            );

            GlobalCrosshairStyle = Config.Bind(
                categoryCrosshair,
                "Crosshair style",
                Crosshairs.Traditional,
                new ConfigDescription("What crosshair style are we feeling up to today?",
                null,
                new ConfigurationManagerAttributes { Order = 2 })
            );

            GlobalCrosshairColor = Config.Bind(
                categoryCrosshair,
                "Crosshair color",
                CrosshairColors.Red,
                new ConfigDescription("What crosshair color are we feeling up to today?",
                null,
                new ConfigurationManagerAttributes { Order = 1 })
            );



            // helmet cam values
            UseHelmetcamValues = Config.Bind(
                categoryPresetValues,
                "Use Helmetcam Values?",
                false,
                new ConfigDescription(
                    "Whether to use your own custom values for each preset. Disable if you want to use the default values that come with the mod.",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 13 }
                )
            );

            helmetCamTopX = Config.Bind(
                categoryPresetValuesTop,
                "Helmet Top X",
                0.04f,                    // Default Value
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 12 })
            );

            helmetCamTopY = Config.Bind(
                categoryPresetValuesTop,
                "Helmet Top Y",
                0.22f,                    // Default Value
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 11 })
            );

            helmetCamTopZ = Config.Bind(
                categoryPresetValuesTop,
                "Helmet Top Z",
                0.0f,                    // Default Value
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 10 })
            );

            helmetCamLeftX = Config.Bind(
                categoryPresetValuesLeft,
                "Helmet Left X",
                0.06f,                    // Default Value
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 9 })
            );

            helmetCamLeftY = Config.Bind(
                categoryPresetValuesLeft,
                "Helmet Left Y",
                0.1f,                    // Default Value
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 8 })
            );

            helmetCamLeftZ = Config.Bind(
                categoryPresetValuesLeft,
                "Helmet Left Z",
                0.16f,                    // Default Value
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 7 })
            );

            helmetCamRightX = Config.Bind(
                categoryPresetValuesRight,
                "Helmet Right X",
                0.06f,                    // Default Value
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 6 })
            );

            helmetCamRightY = Config.Bind(
                categoryPresetValuesRight,
                "Helmet Right Y",
                0.1f,                    // Default Value
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 5 })
            );

            helmetCamRightZ = Config.Bind(
                categoryPresetValuesRight,
                "Helmet Right Z",
                0.17f,                    // Default Value
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 })
            );

            helmetCamAboveEyesX = Config.Bind(
                categoryPresetValuesAboveEyes,
                "Helmet Above Eyes X",
                0.18f,                    // Default Value
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 })
            );

            helmetCamAboveEyesY = Config.Bind(
                categoryPresetValuesAboveEyes,
                "Helmet Above Eyes Y",
                0.08f,                    // Default Value
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 })
            );

            helmetCamAboveEyesZ = Config.Bind(
                categoryPresetValuesAboveEyes,
                "Helmet Above Eyes Z",
                0.0f,                    // Default Value
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 })
            );



            UseBodycamValues = Config.Bind(
                categoryBodycamValues,
                "Use Bodycam Values?",
                false,
                new ConfigDescription(
                    "Use custom values",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }
                )
            );

            bodyCamX = Config.Bind(
                categoryBodycamValues,
                "Bodycam X",
                0.23f,
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 })
            );

            bodyCamY = Config.Bind(
                categoryBodycamValues,
                "Bodycam Y",
                -0.02f,
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 })
            );

            bodyCamZ = Config.Bind(
                categoryBodycamValues,
                "Bodycam Z",
                0.11f,
                new ConfigDescription("Adjusts the master volume scale.",
                new AcceptableValueRange<float>(-10f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 })
            );
        }

        private void Update()
        {
            if (!Plugin.useHotkeys.Value) return;

            if (Plugin.helmetCamSwitch.Value.IsDown())
            {
                Plugin.helmetCam.Value = !Plugin.helmetCam.Value;
            }

            if (Plugin.bodyCamSwitch.Value.IsDown())
            {
                Plugin.bodyCam.Value = !Plugin.bodyCam.Value;
            }
        }
    }
}
