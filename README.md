# RaccoonCam

> Somebody decided that a filter is not enough; maximum immersion is necessary.

**Author:** hazelify  
**Version:** 1.0.0  
**SPT Version:** 4.1.6  
**License:** MIT

---

## What This Mod Does

RaccoonCam is a relatively simple mod that provides two different camera types;
- Bodycam (similar to YouTube channels that provide "bodycam footage";
- Helmetcam (similar to YouTube war channels that provide war footage)

The bodycam feature is a simple camera that is attached to your chest/plate carrier/body armor, with smoothing to ensure running is still viable.

The helmetcam feature is a helmet-mounted camera that has four presets and can be adjusted depending on your needs: Left side, Right side, Top side, and Above Eyes.

Coordinates for camera placement for all features and presets can be adjusted and customized in realtime.

I've also implemented compatibility for [Stance Overhaul](https://sp-mod.com/mod/3136/spt-stance-overhaul) to ensure that the mod works as it should if both are installed.

---

## Requirements

- [SPT](https://sp-mod.com/) 4.1.2 or later 4.1.x installed (BepInEx is included)
- .NET SDK (`netstandard2.1`-compatible) for building from source

---

## Building

```sh
cd RaccoonCam
dotnet build -c Release
```

The PostBuild target automatically copies the compiled DLL to your SPT installation's `BepInEx\plugins\` folder.
The project also packages the mod into a distributable `RaccoonCam.zip` ready to be uploaded.

---

## Installation

1. Build the project (see above) **or** download the latest release DLL.
2. Copy `RaccoonCam.dll` to `<SPT install>\BepInEx\plugins\`.
3. Launch SPT as usual.

---

## Learning Resources

| Resource | URL |
|---|---|
| SPT Client Mod Migration (4.1) | https://wiki.sp-tushonka.com/en/modding/SPT_41_Modding/Client_40_to_41 |
| Harmony Patching Docs | https://harmony.pardeike.net/articles/patching.html |
| SPT Wiki Modding Resources | https://wiki.sp-tushonka.com/en/modding/Modding_Resources |

---
