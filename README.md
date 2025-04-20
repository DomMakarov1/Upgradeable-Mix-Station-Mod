Dom's Upgradeable Mix Station mod adds a fully-featured upgrade and enhancement system to the Mixing Station Mk2 in Schedule I, giving players more control and progression in their drug manufacturing empire.

## Requirements
- The game requires your mod to target the net6 framework, this is defined in the ExampleMod.csproj file TargetFramework property and net6 must be installed before building the library. Visual Studio will prompt you to install it if not present.
- Then install the 2 required packages, Visual Studio toolbar -> Project -> Manage NuGet Packages
  - Search for LavaGang.MelonLoader and install it
  - Search for Lib.Harmony and install it

- Now you must additionally have MelonLoader installed for the game, if it is not installed do it now.
- Start your game once and let MelonLoader build the il2cpp assemblies. After this is done the game will start and then close the game.
  - Then you must navigate to the following directory: C:\Program Files (x86)\Steam\steamapps\common\Schedule I\MelonLoader\Il2CppAssemblies
  - From here you will need two files: Assembly-CSharp.dll and UnityEngine.CoreModule.dll, move these two files into the libs folder. (Also specified in the .csproj file)
      - NOTE: Now if you inspect that that Assembly-CSharp.dll file with dnSpy, you will find that the namespace for ScheduleOne has become Il2CppScheduleOne
      - NOTE: You will need to use that specific namespace when referring to the game namespace related things by: using Il2CppScheduleOne.Player;

- After these steps are done, you are ready to code your own logics. See the Template MainMod.cs file for the basic requirements for Harmony and MelonLoader.

## NOT COMPATIBLE WITH MONO, ONLY IL2CPP

## Features
-Upgradeable Mixing Station
Upgrade your Mixing Station up to Level 5, reducing mixing times with each level.
-Progressive Costs
Each upgrade costs more, starting at $500 and doubling until Level 5. Once maxed, you’ll unlock the ability to enhance the station.
-Enhancement Mode
At Level 5, the upgrade button transforms into an “Enhance” option for $30,000. Enhancing instantly reduces mixing time to 1 second.

## Installation
Drop the compiled .dll into your MelonLoader Mods/folder.
Launch the game — the mod activates automatically when a Mixing Station Mk2 is used.
