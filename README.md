# BepInEx-Partiality-Wrapper

[![Version](https://img.shields.io/badge/BepInEx-5.0-green.svg)](https://github.com/BepInEx/BepInEx)
[![Version](https://img.shields.io/badge/Partiality-0.1-green.svg)](https://github.com/PartialityModding/Partiality)

A <b>BepInEx-Partiality Mod Wrapper</b> for Unity Games (this fork is specifically targeted at Outward). This allows Partiality mods to run in BepInEx.

The zip release in this repository also includes a release of <b>BepInEx 5.0</b> for convenience. All credit goes to bbepis for BepInEx.

## Installation

1. Take the <b>BepInEx 5.0 + PartialityWrapper.zip</b> file and place it in your Outward installation folder.
2. It should look like `"...Steam\steamapps\common\Outward\BepInEx 5.0 + PartialityWrapper.zip"`
3. Right-click the file and choose <b>"Extract here</b>"
4. Run the game once, and then close it.
5. Done!

## Adding Partiality Mods

Drag and drop any **Partiality** mod .dll files onto **BepInEx\plugins** folder.

Some mods may use the `Outward\Mods` folder for assets or configuration files. You <b>still need this folder in these cases</b>. Only the actual mod .dll file should be placed in BepInEx\plugins\.

For example for SideLoader, `SideLoader.dll` goes in `Outward\BepInEx\plugins`, but all SL Packs should be in `Outward\Mods\SideLoader\`.

## Credits
* <b>Ashnal</b> for the original PartialityWrapper
* <b>notfood</b> for the Bep 5.0 rewrite
* <b>bbepis</b> for BepInEx
* <b>Zandra<b> for Partiality
* <b>0x0ade</b> for MonoMod

## License
[MIT](https://choosealicense.com/licenses/mit/)
