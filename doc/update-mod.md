# Update Client Mod

## Full update with assets

1. Bump the mod version in `Submodule.xml` and `Directory.Build.props`
2. Run `git commit -m 'mod: set version to W.X.Y.Z'` `git push` `git tag vZ` `git push --tags`
3. Rebuild Module.Client in release mode
4. Make sure you have all asset sources in `%MB_CLIENT_PATH%\Modules\cRPG\AssetSources`
5. Create the folder `%MB_CLIENT_PATH%\Modules\cRPG\Assets`
6. Copy `%MB_CLIENT_PATH%\Modules\cRPG\Win64_Shipping_Client` to `%MB_CLIENT_PATH%\Modules\cRPG\Win64_Shipping_wEditor`
7. In `%MB_CLIENT_PATH%\Modules\cRPG\SubModule.xml` replace `<ModuleCategory value="Multiplayer"/>` by `<ModuleCategory value="SinglePlayer"/>`
8. Remove `%MB_CLIENT_PATH%\Modules\cRPG\GUI\*SpriteData.xml`
9. Launch `Mount & Blade II: Bannerlord - Modding Kit` and make sure to tick `cRPG` in the Singleplayer mods
10. Run the editor
11. Do `Window` / `Show Resource Browser`
12. Go to `Modules\cRPG\Assets`
13. Right click in the folder and `Import new asset`
14. Select all assets in `%MB_CLIENT_PATH%\Modules\cRPG\AssetSources\*`
15. In the editor do `File` / `Publish Module`
16. Select the `cRPG` Module
17. Tick `Packing Type` `Client`
18. Click `Publish` and select a random location such as `Desktop`
19. Add back the `GUI\*SpriteData.xml` to the published mod
20. Change back the module category to Multiplayer in `SubModule.xml`
21. Remove the folder `bin\Win64_Shipping_wEditor` in the published mod
22. Create a file `WorkshopUpdate.xml` anywhere on your disk (e.g. Desktop) with the content
```xml
<Tasks>
	<GetItem>
		<ItemId Value="2878356589"/>
	</GetItem>
	<UpdateItem>
		<ModuleFolder Value="C:\PATH\TO\PUBLISHED\MODULE" />
                <ChangeNotes Value="W.X.Y.Z" />
		<Tags>
                        <Tag Value="Multiplayer" />
                        <Tag Value="vW.X.Y" />
		</Tags>
	</UpdateItem>
</Tasks>
```
21. Replace the ModuleFolder path with your published path
22. Run `"%MB_CLIENT_PATH%/bin/Win64_Shipping_Client/TaleWorlds.MountAndBlade.SteamWorkshop.exe" C:\PATH\TO\PUBLISHED\MODULE`

## Code/XML/Scenes update only

If you are not updating assets, you can do steps [1-3], then
copy the files from `%MB_CLIENT_PATH%` to the already existing published
folder and do step 22.
