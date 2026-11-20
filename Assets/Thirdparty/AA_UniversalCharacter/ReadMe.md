# Universal Character

A modular low poly character base, male and female, with armour parts, a helmet, a shield and an axe. Mix the parts or randomize a new character with one click. Part of the Codewart modular character series.

## New in version 2.0
Version 2.0: updated for Unity 6.3 LTS. Universal Render Pipeline by default, Built-in and HDRP materials packages included. New showroom scene that shows every character with its own weapons and shields, new metal material, build error in the inspector script fixed, publisher tools removed from the package. Asset paths and GUIDs are unchanged, so existing scenes keep working.

## Quick start
1. Open `Showroom/Showroom_AA.unity` and press Play to browse the characters.
2. Drag any prefab from `Prefabs` into your scene. Weapons and shields are separate prefabs in `Prefabs/Props`.
3. `DemoScene/DemoScene.unity` shows the characters together.

## Render pipelines
The pack is set up for the Universal Render Pipeline. For the Built-in Render Pipeline, import `BuiltIn/AA_BuiltIn_Materials.unitypackage`: it replaces the materials and nothing else. The showroom shaders work in both. For the High Definition Render Pipeline, import `HDRP/AA_HDRP_Materials.unitypackage` the same way; the showroom scene needs URP or the Built-in Render Pipeline.

## Matte and metal materials
All prefabs use the matte `Materials/UniversalCharacterMaterial`. `Materials/AA_Material_Metal` uses the same atlas with the specular map `Textures/AA_Metal.png`, so metal parts catch the light while cloth stays matte. Assign it to the renderers if you want this look.

## Showroom controls
| Key | Action |
|---|---|
| Up, Down | Previous or next character |
| Space | Mix parts between characters of the same body type |
| Alt or Backspace | Back to the character's own set |
| Ctrl | Rotate the set by 90 degrees |
| W, S | Zoom in and back out, three steps |
| H | Show or hide the helmet |
| T | Show or hide the on-screen text |

The Left and Right keys switch animations when you fill in the Clips list on the Showroom object. The pack itself contains no animations.

Weapons and shields from `Prefabs/Props` float beside the character whose set number they carry, for example `Axe_AA_001` beside both characters of set 001. They have no mount bone, so the showroom hides them while an animation plays.

## Characters and animation
All characters use one Humanoid skeleton, so any Humanoid animation can be retargeted to them.

## Building your own characters
The `ModulesShaker` component on the prefabs sets parts by preset number or randomizes them: choose a preset number and press **Set all elements by numeration**, or use the randomize buttons. Details are in `Scripts/HowToUse - Modules Shaker.pdf`.

Modules work with the other Codewart modular packs on the same skeleton: Gladiators, Knights, Fantasy Heroes and Knights of the Round Table. A part taken from another pack uses the material of the character it is placed on.

## Contents
| Folder | What is inside |
|---|---|
| `Prefabs` | Character prefabs, `Props` holds one prefab for each prop |
| `Meshes` | `UniversalCharacter.fbx` with all modules, `Props` with one file for each prop |
| `Materials`, `Textures` | Matte and metal material, texture atlas and specular map |
| `Showroom` | Character viewer scene with its scripts and shaders |
| `DemoScene` | The demo scene |
| `Scripts` | `ModulesShaker`, its inspector and the guide |
| `BuiltIn` | Optional materials for the Built-in Render Pipeline |
| `HDRP` | Optional materials for the High Definition Render Pipeline |

## Support
support@codewart.com
https://assetstore.unity.com/publishers/49258
