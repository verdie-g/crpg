# How to contribute to cRPG

Contributors should follow [Google Engineering Practices](https://google.github.io/eng-practices)! For example too large
PRs or PRs with no context might get ignored.

## Report a bug

- **Do not open GitHub issue if the bug is a security vulnerability in cRPG**, and instead follow
  the [security policy](https://github.com/verdie-g/crpg/blob/master/SECURITY.md).
- **Ensure the bug was not already reported** by searching on GitHub under [Issues](https://github.com/verdie-g/crpg/issues?q=is%3Aissue)
- If you're unable to find an open issue addressing the problem, open a new one. Make sure to include
  a title and a clear description, as much relevant information as possible, and ideally a screenshot
  or a video pointing out the issue

## Add a feature / Fix a bug

- Discuss first with the developers on the [cRPG Discord](https://discord.gg/c-rpg) if
  you're adding a new feature
- Open a new GitHub pull request with the patch
- Ensure the PR description clearly describes the problem and solution. Include the relevant issue
  number if applicable.
- Make sure the changes are correctly unit-tested. Tests can be skipped if the changes are
  on the mod itself since Bannerlord code is tedious to test

## High Level Architecture

![crpg-architecture](https://user-images.githubusercontent.com/9092290/95020344-df71a880-066a-11eb-8439-f21f90cbc9c7.png)

## Run

### VS Code Dev Container

- Install VS Code extension [Dev Containers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)
- Run the `Remote-Containers: Open Folder in Container` command from the Command Palette (F1)

[More info about dev containers](https://code.visualstudio.com/docs/remote/containers).

### Manually

#### Web API (src/WebApi)

- Download [.NET 7 SDK](https://dotnet.microsoft.com/download)
- Download your favorite IDE: [Visual Studio](https://visualstudio.microsoft.com/vs), [Visual Studio Code](https://code.visualstudio.com), [Rider](https://www.jetbrains.com/rider)...
- Open the solution file Crpg.sln
- Run `dotnet user-secrets set "Steam:ApiKey" "MY_API_KEY"` where `MY_API_KEY` is a Steam
  API key acquired by [filling out this form](https://steamcommunity.com/dev/apikey)
- Run `dotnet dev-certs https --trust` to be able to launch the API with HTTPS. The authentication creates a cookie
  with `SameSite=None` and recent version of Chrome requires HTTPS to do so
- Build and run (can be done without IDE using [dotnet cli](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-run))

By default, the Web API uses an in-memory database so no DBMS need to be downloaded.

#### Web UI (src/WebUI)

- Download [Node.js](https://nodejs.org)
- Go to `src/WebUI`
- Run `npm install` to install dependencies
- Run `npm run dev` to launch the application
- Use `npm run lint` to fix the coding style

The client relies on the server so you have to run both.

#### Mod (src/Module.Client + src/Module.Server)

- Follow TaleWorlds' guide "[Hosting a custom server](https://moddocs.bannerlord.com/multiplayer/hosting_server)"
- Set the environment variable `MB_CLIENT_PATH` to your Bannerlord client installation
  (e.g. C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II: Bannerlord)
- Set the environment variable `MB_SERVER_PATH` to your Bannerlord server installation
  (e.g. C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II: Dedicated Server)
- Build to generate the `Crpg.Module.dll` file in the Modules folder of both the client and the server

By default, the game server is not connected to the main cRPG server so you don't get
your characters when joining a server but a character is created with random items and
characteristics. To connect to the main cRPG server you need to set the environment
variable `CRPG_API_KEY`.

Custom cRPG items and maps are not stored in the git repository. If you need them for
development you can copy the AssetPackages folder from the Workshop into the client
module and also the SceneObj folder from the Workshop into both the client and server.
