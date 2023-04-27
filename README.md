# Game Scene Manager

Tool for defining collections of scenes to be loaded together, as well as some scene management-related utilities.

Please note: this package is a WORK IN PROGRESS, I am making it for my own projects. Its structure is bound to change at any time as I find out what works and what doesn't.

Most of this README's contents will eventually be moved to a documentation site.

## Contents

- [Dependencies](#dependencies)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Features](#features)


## Dependencies
These should be handled automatically by the Unity Package Manager.
- `com.unity.addressables`: Because the Scene assets are in the UnityEditor namespace, references to them can't be serialized easily. To avoid dealing with this, the [Unity Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@latest/index.html) system is being used.

## Installation

### Option A: Scoped Registry

All my public/open source packages are hosted on [my registry](https://registry.bazzagibbs.com).

1. Install a Scoped Registry: `Project Settings > Package Manager > Scoped Registries`
```
Name:       BazzaGibbs
URL:        https://registry.bazzagibbs.com
Scope(s):   com.bazzagibbs
```
2. In the Package Manager, select "Add package by name", `com.bazzagibbs.gamescenemanager`

### Option B: Git URL

Adding packages by Git URL has the downside of not properly displaying when there is a package update available.

1. In the Package Manager, select "Add package from git URL", `https://github.com/Bazzagibbs/GameSceneManager.git`


## Quick Start

### Recommended File Structure

```
Directories and files marked with *** are recommended to be included in your file structure. Other files are for example purposes.


- Assets
|
|   - SC_Auxiliary            ***
|   |   - Player UI
|   |   ...
|
|   - SC_Core                 ***
|   |   - Save System
|   |   ...     
|
|   - SC_Levels               ***
|   |   - Main Menu
|   |   - Level 01
|   |   ...
|
|   - Scene Collections       ***
|   |   ...
|
|   - Scenes                  ***
|   |   - _EntryPoint         ***
|   |   - PlayerUIScene
|   |   - SaveSystemScene
|   |   - MainMenuScene
|   |   - Level 01
|   |   |   - Level01_01
|   |   |   - Level01_02
|   |   |   ...
|   |   
|   |   - Level 02
|   |   |   ...

```

### Entry Point

Your game needs an entry point - the scene that gets loaded in a build. This scene will serve one purpose, and that is to start the Game Scene Management system.

1. Create a Scene in your project. I like to call it something that sorts it to the top of its directory, such as "\_EntryPoint".
2. Keep the Camera, but remove the directional light.
3. Add an Event System ("UI > Event System").
4. Add an empty GameObject called "Game Scene Manager", and add the `GameSceneManager` component to it.
5. Go to "File > Build Settings", and make sure __only__ your "_EntryPoint" scene is in the build.

At this point, the \_EntryPoint scene should have three gameObjects in it; the Game Scene Manager, a camera and an Event System.

### Scene Collections

Before we can load up a main menu, let's talk about how scenes are managed by the Game Scene Manager.

Scene Collection assets are used to define groups of Scenes that are loaded together. There are three types of Scene Collection:
- __Auxiliary Scene Collection__: Scenes that contain systems that can be loaded in and out at any time, such as a player HUD or pause menu. Other scenes should not depend on any Auxiliary scenes.
- __Game Level__: One and only one Game Level is active at any moment. Changing the Game Level will unload any Auxiliary Scene Collections that are currently loaded. The Main Menu also should be considered a Game Level, and is usually the first one we want to load.
- __Core Scene Collection__: Core scenes can be loaded at any time, but may have other scenes dependent on them. They will not be unloaded when the Game Level is changed. These may be useful for Save systems, network managers, etc.

The Game Scene Manager will immediately load whichever Game Level it is provided with, in our case we want to start at the Main Menu.

### Creating Game Level assets

1. Create a scene called "MainMenuScene" and a scene called "Level1Scene".
   -  Make sure these scenes don't have a camera or event system in them. These will be loaded separately in a Core Scene Collection.
2. Create a Game Level asset: "Create > Game Scene Manager > Game Level", named "Main Menu"
3. In the asset's inspector, expand the "Scene Refs" dropdown and add a new element.
4. Add a new element, and drag the "MainMenuScene" asset onto it.
5. Repeat from step 2 for the Level1Scene.

### Loading the Start Level

1. Return to the "\_EntryPoint" scene and select the Game Scene Manager object.
2. Set the Game Scene Manager's "Start Level" field to your "Main Menu" Level asset.

At this point, if you run the game from the "\_EntryPoint" scene, the Main Menu scene should also be loaded. You may want to add some gameObjects to each scene to see what is currently loaded.

### Changing Levels

Changing scenes from C# scripts can be simple, see the following snippet:
```csharp
using UnityEngine;
using BazzaGibbs.GameSceneManager;

// A trigger zone to load the next level in a linear game.
public class MyLevelLoadTrigger : MonoBehaviour {
  public GameLevel nextLevel;

  private void OnTriggerEnter(Collider other){
    Debug.Log($"Loading Level: {nextLevel.name}");

    GameSceneManager.SetLevel(nextLevel);
  }
}

```

To achieve a similar effect from the inspector, such as UI buttons or UnityEvents, we can use a GameSceneManagerHook asset. "Create > Game Scene Manager > Game Scene Manager Hook"

This asset can be used to call the GameSceneManager's static methods.

1. In the Main Menu scene, add a UI button labelled "Level 1".
2. In the button's "On Click" event, add an entry:
   - Target Object: Game Scene Manager Hook
   - Function: "SetLevel"
   - Parameter: your Level 1 Game Level asset

Repeat this in your Level 1 scene to add a way back to the main menu.

### Optional: Play Mode Launch Settings

If you find yourself frustrated at having to unload the scenes you're working in every time you want to enter play mode, there's a solution!

1. Create a "Game Scene Manager > Play Mode Launch Settings" asset.
2. Select your "\_EntryPoint" scene.

Now whenever you press the Play mode button, only your \_EntryPoint scene will be launched, allowing you to set up dependencies properly.

If you instead want to immediately launch play mode in your currently open scenes, simply leave the "Play Mode Scene" field empty.

### Wrapping Up

That's it, the system should be ready to use! The game should launch into the Main Menu level, and clicking the buttons should change them.

---
## Features

### Runtime
- __Game Scene Manager__ (GameObject): Singleton GameObject that controls the state of loaded scenes.
- __Game Scene Manager Hook__ (ScriptableObject): Utility asset that can be used to access the Game Scene Manager's functions from UnityEvents, such as UI Buttons.
  - "Create > Game Scene Manager > Game Scene Manager Hook"
  - Reference this asset from UnityEvents to call Game Scene Manager static functions from the inspector.
- __Game Scene Collections__ (ScriptableObject): A method for organizing additive scenes based on their intended lifetimes. Listed from shortest lifetime to longest:
  - __Auxiliary Scene Collection__: Includes scenes that can be loaded and unloaded at will. Game Levels and Core scenes should not depend on Auxiliary scenes.
  - __Game Level__: One and only one Game Level collection is active at any time in the game. This may include Main Menus, lobbies and gameplay stages. Changing the Game Level will unload all active Auxiliary Scene Collections.
  - __Core Scene Collection__: Includes scenes that may have dependencies in other scenes, such as an Event System or Save Manager. Unlike Auxiliary Scene collections, Core scenes are not unloaded whenever the Game Level is changed.
  
### Editor

- __Play Mode Launch Helper__ (ScriptableObject): Launch Play Mode from a different scene to those currently edited.
  - `Create > Game Scene Manager > Play Mode Launch Settings`
  - If a Scene asset is provided, it will be launched when Play mode is entered. and the specified scene will be launched. These closed scenes will be reopened upon exiting Play Mode.
  - Otherwise, the currently open scenes will be launched as normal.

## WIP

### Runtime
- Implement Core scenes properly
- "Wait for loading screen" toggle on Level assets
- Scene Collection dependency counters? (probably not needed if other scene decoupling strategies are used)

### Editor

- Build verification - make sure all referenced Addressables are valid, etc.
- Unit tests
  