# Phigros UI Sample
UI development exercise: a recreation of the song selection menu from Phigros.

## Notes
- No music is included or played, and there is no audio system to play sounds provided
  - This is because royalty-free music sites do not allow redistributing the raw audio files, and since this is an open-source repo I can't include music without also distributing the raw files
  - Since there is no audio system, there are also no sound effects
- Features from the Phigros song select that are not implemented: filter/sort options, random song button, open settings menu button, user profile information, and scrolling cover art/blurred cover art as the background

Change the data displayed:
- A list of `MusicData` scriptable objects can be found on the `SongSelectScreen` prefab. Edit this list, or the information on the scriptable objects, to change the songs displayed
  - The scriptable objects can be found in the `_Main/Data/MusicData` assets folder
  - On the `MusicData` scriptable objects, the song name, artist, cover art, difficulty level and score/accuracy/ranking for each difficulty can be edited for each song
  - Currently none of songs have any cover art attached (since there's no actual songs)

Change how the data is displayed:
- `SongSelectScreen` prefab: change the **initial difficulty** (`Current Difficulty` variable) and if **mirror mode** is active by default (`Mirror Mode Active` variable)
  - (The `Current Song Data` variable is purely runtime data for debugging, and cannot be set beforehand. It will always change to the first song on the list)
- `ScrollingSongList` prefab: all variables under the `Settings` header **(list item size and spacing, size of the list, snap settings)**
- `DifficultySelection` prefab: change **colors** and **movement settings** for difficulty selection

## Known Issues
- There's really bad aliasing on all the UI shapes
  - Because of how the shapes scale and are angled/sized in the design, scaling or tiling a single sprite does not work, unless you wanted to use a unique sprite for every aspect ratio of parallelogram used
  - The best solution is to use a plugin like Freya's [Shapes](https://assetstore.unity.com/packages/tools/particles-effects/shapes-173167) plugin instead of Unity's built-in VertexHelper, since there is no way to draw angled lines without aliasing using the VertexHelper (to my knowledge)
  - Since this is an open source repo, we can't include a paid plugin like that here, so we're stuck with the aliasing for now
- On rare occasions, the list will snap a few pixels above or below (usually below) where it should stop

## Resources
Phigros is the property of PigeonGames, I am not associated with them in any way. This repo is just a programming exercise, recreating an existing game's design. Please support the official release.

The font used is Saira, licensed under the [SIL Open Font License 1.1](https://openfontlicense.org/open-font-license-official-text/)

Icons provided by Google Icons, licensed under the [Apache 2.0 license](https://www.apache.org/licenses/LICENSE-2.0.html)
