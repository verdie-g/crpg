# Export items

To export item thumbnails and the items.json file, follow the instructions:

- Set the `MB_CLIENT_PATH` environment variable to your Bannerlord path (e.g. C:/Program Files/steam/steamapps/common/Mount & Blade 2: Bannerlord)
- Enable the `Export Items` option by removing these lines in the file CrpgSubModule.cs
```diff
@@ -91,10 +91,8 @@ namespace Crpg.GameMod
   Module.CurrentModule.AddInitialStateOption(new InitialStateOption("DefendTheVirgin", new TextObject("{=4gpGhbeJ}Defend The Virgin"),
       4567, () => MBGameManager.StartNewGame(new DefendTheVirginGameManager()), () => false));

-            #if false
   Module.CurrentModule.AddInitialStateOption(new InitialStateOption("ExportData",
-                new TextObject("Export Data"), 4578, ExportData, () => false));
-            #endif
+                new TextObject("Export Data"), 4578, ExportData, () => false));

   // Uncomment to start watching UI changes.
   // UIResourceManager.UIResourceDepot.StartWatchingChangesInDepot();
```
- Build Crpg.GameMod assembly
- Launch Bannerlord
- Enable cRPG mod
- Max out the quality of your game as the quality of the images depend on it
- Click on the `Export Items` button in the menu
- Wait for the "Done" message
- Items are exported in `$MB_CLIENT_PATH/Items`
