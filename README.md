# ActualBodyCam

> Somebody decided that a filter is not enough; maximum immersion is necessary.

**Author:** hazelify  
**Version:** 1.0.0  
**SPT Version:** 4.1.6  
**License:** MIT

---

## What This Mod Does

Describe what your mod does here.

This template includes a Harmony patch example using SPT's `ModulePatch` system. The example is in `Patches/` and is disabled until you enable it in `Plugin.cs`.

### How Patches Work

1. Create a class that inherits `ModulePatch`.
2. Override `GetTargetMethod()` to specify which game method to patch.
3. Add `[PatchPrefix]` and/or `[PatchPostfix]` static methods.
4. Enable the example in `Plugin.Awake()` by uncommenting `new Patches.ExamplePatch().Enable()`.

---

## Requirements

- [SPT](https://sp-mod.com/) 4.1.2 or later 4.1.x installed (BepInEx is included)
- .NET SDK (`netstandard2.1`-compatible) for building from source

---

## Building

```sh
cd ActualBodyCam
dotnet build -c Release
```

The PostBuild target automatically copies the compiled DLL to your SPT installation's `BepInEx\plugins\` folder.
The project also packages the mod into a distributable `ActualBodyCam.zip` ready to be uploaded.

---

## Installation

1. Build the project (see above) **or** download the latest release DLL.
2. Copy `ActualBodyCam.dll` to `<SPT install>\BepInEx\plugins\`.
3. Launch SPT as usual.

---

## Project Structure

```
ActualBodyCam/
├── ActualBodyCam.csproj   <- project file with DLL references
├── Plugin.cs              <- BepInEx plugin entry point
├── Patches/
│   └── ExamplePatch.cs    <- Harmony patch example (Prefix/Postfix)
├── README.md
└── .gitignore
```

---

## Learning Resources

| Resource | URL |
|---|---|
| SPT Client Mod Migration (4.1) | https://wiki.sp-tushonka.com/en/modding/SPT_41_Modding/Client_40_to_41 |
| Harmony Patching Docs | https://harmony.pardeike.net/articles/patching.html |
| SPT Wiki Modding Resources | https://wiki.sp-tushonka.com/en/modding/Modding_Resources |

---
