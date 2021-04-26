# Export items

To export item thumbnails and the items.json file, follow the instructions:

- Set the `MBPATH` environment variable to your Bannerlord path (e.g. C:/Program Files/steam/steamapps/common/Mount & Blade 2: Bannerlord)
- Enable the `Export Items` option by removing these lines in the file CrpgSubModule.cs
```diff
line: 94
- #if false
line: 97
- #endif
```
- Build Crpg.GameMod assembly
- Launch Bannerlord
- Enable cRPG mod
- Click on the `Export Items` button in the menu (The quality of the images depends on the level of the graphics of the game)
- Wait for the "Done" message
- Items are exported in `$MBPATH/Items`
