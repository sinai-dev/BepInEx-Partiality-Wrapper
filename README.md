# BepInEx-Partiality-Wrapper

[![Version](https://img.shields.io/badge/BepInEx-5.0-green.svg)](https://github.com/BepInEx/BepInEx)
[![Version](https://img.shields.io/badge/Partiality-0.3.1-green.svg)](https://github.com/PartialityModding/Partiality)

A <b>BepInEx-Partiality Mod Wrapper</b> for Unity Games. This allows Partiality mods to run in BepInEx.

This version is essentially Partiality packaged <b>as</b> a BepInEx mod. It serves as a dummy reference to Partliality.dll for Partiality Mods, but otherwise behaves as a BepInEx mod.

The HOOKS file included with the release is the current latest one for Outward. If you're using this for a different game you will need to delete that and run the mod once to generate the hooks for your game.

## Release

<b>[Download Here](https://github.com/sinaioutlander/BepInEx-Partiality-Wrapper/releases)</b>

## Installation

Install <b>BepInEx</b>: (if you don't have it already)
1. Download the [latest BepInEx_x64 release](https://github.com/BepInEx/BepInEx/releases).
2. Place it in your game installation folder, so it looks like `...Steam\steamapps\common\{gamename}\BepInEx.zip`.
3. Right-click the file and choose <b>"Extract here</b>" to merge with the folder structure.
4. Run the game once, and then close it.

Install the <b>PartialityWrapper</b>:
1. Take the <b>BepInEx-Partiality-Wrapper.zip</b> file and place it in your Game installation folder.
2. It should look like `"...Steam\steamapps\common\{gamename}\BepInEx-Partiality-Wrapper.zip"`
3. Right-click the file and choose <b>"Extract here</b>" to merge with the folder structure.
4. Done, you can now add Partiality mods.

## Adding Partiality Mods

Drag and drop any **Partiality** mod .dll files onto **`BepInEx\plugins`** folder.

For Outward, some mods may use the `Outward\Mods` folder as well. Refer to the instructions of the mod for more details.

* For example, for the Outward SideLoader, `SideLoader.dll` goes in `Outward\BepInEx\plugins`, but all SL Packs should be placed in `Outward\Mods\SideLoader\`.

## Credits
* <b>Ashnal</b> for the original PartialityWrapper
* <b>notfood</b> for the Bep 5.0 rewrite
* <b>bbepis</b> for BepInEx
* <b>Zandra</b> for Partiality
* <b>0x0ade</b> for MonoMod

## License
[MIT](https://choosealicense.com/licenses/mit/)
