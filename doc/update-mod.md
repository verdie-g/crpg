# Update Client Mod

## Generate Asset Package

`cRPG/AssetPackages/pack0.tpac` contains the cRPG sprite sheets (map images and stuff).
Here is the step to step guid how to update that file:

1. Build the client to generate some needed files
2. Put all asset sources in `%MB_CLIENT_PATH%/Modules/cRPG/AssetSources`
3. Create an empty folder `%MB_CLIENT_PATH%/Modules/cRPG/Assets`
4. Copy `%MB_CLIENT_PATH%/Modules/cRPG/Win64_Shipping_Client` to `%MB_CLIENT_PATH%/Modules/cRPG/Win64_Shipping_wEditor`
5. In `%MB_CLIENT_PATH%/Modules/cRPG/SubModule.xml` replace `<ModuleCategory value="Multiplayer"/>` by `<ModuleCategory value="SinglePlayer"/>`
6. Remove `%MB_CLIENT_PATH%/Modules/cRPG/GUI/*SpriteData.xml`
7. Launch `Mount & Blade II: Bannerlord - Modding Kit` and make sure to tick `cRPG` in the Singleplayer mods
8. Run the editor
9. Do `Window` / `Show Resource Browser`
10. Go to `Modules/cRPG/Assets`
11. Right click in the folder and `Import new asset`
12. Select all assets in `%MB_CLIENT_PATH%/Modules/cRPG/AssetSources/*`
13. In the editor do `File` / `Publish Module`
14. Select the `cRPG` Module
15. Tick `Packing Type` `Client`
16. Click `Publish` and select a random location such as `Desktop`
17. In the generated files, you'll get the `AssetPackages/pack0.tpac`. All the rest can be trashed

## Update client/server mod

1. Bump the mod version in `Submodule.xml` and `Directory.Build.props`
2. Run `git commit -m 'mod: bump version to W.X.Y.Z' && git push && git tag vZ && git push --tags`
   where `W.X.Y` is the Bannerlord version and `Z` is the cRPG version
3. Rebuild Module.Client and Module.Server in release mode
4. Make sure that in the client folder you have the `pack0.tpac` generated with the
   above instructions and the `pack1.tpac` containing the cRPG items. Only Meow knows how
   to generate the latter
5. Make sure that in both the client and server folders you have the SceneObj folder containing
   all maps
6. Create a file `WorkshopUpdate.xml` anywhere on your disk (e.g. Desktop) with the content
```xml
<Tasks>
    <GetItem>
        <ItemId Value="2878356589"/>
    </GetItem>
    <UpdateItem>
        <ModuleFolder Value="C:/PATH/TO/CLIENT/MODULE" />
        <ChangeNotes Value="W.X.Y.Z" />
        <Tags>
            <Tag Value="Multiplayer" />
            <Tag Value="vW.X.Y" />
        </Tags>
    </UpdateItem>
</Tasks>
```
7. Replace the ModuleFolder path with `%MB_CLIENT_PATH%/Modules/cRPG`
8. Update Bannerlorcd configuration files on the cRPG servers if needed
9. Replace manually the server module on all cRPG servers with `%MB_SERVER_PATH%/Modules/cRPG` and restart them
10. While it uploads you can write the patch note on Discord
11. Run `"%MB_CLIENT_PATH%/bin/Win64_Shipping_Client/TaleWorlds.MountAndBlade.SteamWorkshop.exe" %MB_CLIENT_PATH%/Modules/cRPG`

## Update WebApi/WebUI

1. In `deploy/playbook.yml` comment what you don't need
2. From WSL or a Linux machine, run `ANSIBLE_CONFIG=./ansible.cfg ansible-playbook playbook.yml`
