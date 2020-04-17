# BepInEx-Partiality-Wrapper

[![Version](https://img.shields.io/badge/BepInEx-5.0-green.svg)](https://github.com/BepInEx/BepInEx)
[![Version](https://img.shields.io/badge/Partiality-0.3.1-green.svg)](https://github.com/PartialityModding/Partiality)

A <b>BepInEx-Partiality Mod Wrapper</b> for Unity Games. This allows Partiality mods to run in BepInEx.

This version is essentially Partiality packaged <b>as</b> a BepInEx mod. It serves as a dummy reference to Partliality.dll for Partiality Mods, but otherwise behaves as a BepInEx mod.

The HOOKS file included with the release is the current latest one for Outward. If you're using this for a different game you will need to delete that and run the mod once to generate the hooks for your game.

## Install

Installation instructions and the download link can be found here:

<b>[Download Here](https://github.com/sinaioutlander/BepInEx-Partiality-Wrapper/releases)</b>

## Adding Partiality Mods

Partiality Mod DLL files can be placed either in the `BepInEx\plugins\` folder, or the Partiality `Mods\` folder. Both locations work fine.

However, no matter which folder you use, the DLL files need to be directly inside this folder (not in a sub-folder).

If you use the `BepInEx\plugins\` folder, just note that other non-DLL files in the mod (if any) should be placed wherever the author instructs you to, likely not in the plugins folder.

## Credits
* <b>Laymain</b>, for help with this rewrite.
* <b>Ashnal</b> for the original PartialityWrapper
* <b>notfood</b> for the Bep 5.0 rewrite
* <b>bbepis</b> for BepInEx
* <b>Zandra</b> for Partiality
* <b>0x0ade</b> for MonoMod

## License
[MIT](https://choosealicense.com/licenses/mit/)
