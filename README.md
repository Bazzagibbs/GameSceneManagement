# Game Scene Manager

Tool for defining collections of scenes to be loaded together, as well as some scene management-related utilities.

## Features

### Editor

- __Play Mode Launch Helper__ - Launch Play Mode from a different scene to those currently edited.
  - Create a settings asset: "Create/Game Scene Manager/Play Mode Launch Settings"
  - If a Scene asset is provided and Play Mode is entered, the currently open scenes will be closed and the specified scene will be launched. These closed scenes will be reopened upon exiting Play Mode.
  - Otherwise, the currently open scenes will be launched as normal.

## WIP

### Runtime

- Scene collections
  - Possible types: Lobby, Menu/UI, Level, Player
- Scene dependencies
- "Constant" scenes

### Editor

- Build verification
 