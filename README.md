# Unity Addressables Custom Boot-Strap Example

Companion article: [Bootstrapping your Unity game with Addressables](https://tomhalligan.substack.com/p/bootstrapping-your-unity-game-with)

This is a demonstration project indicating how developers might use Unity's Addressables system to implement a runtime boot-strapping mechanism.

This project contains a custom settings file in ProjectSettings, a custom Project Settings tab, and automatically configures the Addressables system to include a 'Runtime & Editor' and 'Editor Only' group.

The 'Runtime & Editor' Addressables group is always included in builds, whereas the 'Editor Only' group is not included in builds.

Both groups contain a reference to a single asset, which contains a simple list of prefabs which will be loaded whenever Playmode is entered (either via the Play button in-editor, or by launching a build).

Using this system, we can imagine various extensions and additions which would facilitate a great deal of flexibility and control over how the game is launched.
