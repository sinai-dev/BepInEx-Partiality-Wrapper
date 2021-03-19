# BepInEx-Partiality-Wrapper

[![Version](https://img.shields.io/badge/BepInEx-5.X-green.svg)](https://github.com/BepInEx/BepInEx)
[![Version](https://img.shields.io/badge/Partiality-0.3.1-green.svg)](https://github.com/PartialityModding/Partiality)

The **BepInEx-Partiality-Wrapper** allows Partiality mods to run in BepInEx (5.X).

## Install

Installation is pretty straight-forward:

* Install [BepInEx 5.X](https://github.com/BepInEx/BepInEx/releases)
* [Download the latest PartialityWrapper release](https://github.com/sinai-dev/BepInEx-Partiality-Wrapper/releases/latest)
* Extract the zip file and put the contents into your Game folder, making sure you merge with the existing BepInEx folder.
* It should look like `[game]\BepInEx\patchers\PartialityWrapper\...` and `[game]\BepInEx\plugins\PartialityWrapper\...`.
* **Run the game once** to generate the necessary files and folders.
* If you see the file `BepInEx\plugins\PartialityWrapper\HOOKS-Assembly-CSharp.dll` then you did it correctly.

## Adding Partiality Mods

PartialityWrapper supports two folders where you can place your Partiality mods:

* `[game]\BepInEx\plugins\partiality-mods\` - generated after first launch, the recommended folder to use.
* `[game]\Mods\` - the legacy Partiality folder for mods, in case the mod **requires** this structure.

Both folders allow you to place the mods inside sub-folder(s).

## Credits
Written by Sinai, with help from:

* <b>Laymain</b>, for help with this rewrite.
* <b>notfood</b> for the BepInEx 5.0 rewrite
* <b>Ashnal</b> for the original PartialityWrapper

## License
[MIT](https://choosealicense.com/licenses/mit/)
