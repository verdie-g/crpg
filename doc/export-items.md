# Export items

To export item thumbnails and the items.json file, follow the instructions:

- Set the `MBPATH` environment variable to your Bannerlord path (e.g. C:/Program Files/steam/steamapps/common/Mount & Blade 2: Bannerlord)
- Enable the `Export Items` option by removing these lines in the file CrpgSubModule.cs
```diff
@@ -25,7 +25,6 @@ namespace Crpg.GameMod
- #if false
Module.CurrentModule.AddInitialStateOption(new InitialStateOption("ExportItems", new TextObject("Export Items"), 4578, () =>
{
@@ -33,7 +32,6 @@ namespace Crpg.GameMod
}, false));
- #endif
```
- Build Crpg.GameMod assembly
- Launch Bannerlord
- Enable cRPG mod
- Click on the `Export Items` button in the menu
- Wait for the "Done" message
- Items are exported in `$MBPATH/Items`