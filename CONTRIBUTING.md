# How to contribute to cRPG

## Report a bug

- **Do not open GitHub issue if the bug is a security vulnerability in cRPG**, and instead follow
  the [security policy](https://github.com/verdie-g/crpg/blob/master/SECURITY.md).
- **Ensure the bug was not already reported** by searching on GitHub under [Issues](https://github.com/verdie-g/cRPG/issues?q=is%3Aissue)
- If you're unable to find an open issue addressing the problem, open a new one. Make sure to include
  a title and a clear description, as much relevant information as possible, and ideally a screenshot
  or a video pointing out the issue.

## Fix a bug

- Open a new GitHub pull request with the patch
- Ensure the PR description clearly describes the problem and solution. Include the relevant issue
  number if applicable.
- Make sure the changes are correctly unit-tested

## High Level Architecture

![crpg-architecture](https://user-images.githubusercontent.com/9092290/95020344-df71a880-066a-11eb-8439-f21f90cbc9c7.png)

## Technologies Used

### Web API (src/WebApi)

Server written in [.NET Core](https://dotnet.microsoft.com). Its design is inspired by
[jasontaylordev/NorthwindTraders](https://github.com/jasontaylordev/NorthwindTraders), see the
[conference on Youtube](www.youtube.com/watch?v=Zygw4UAxCdg). It uses the following technologies:
- [ASP.NET Core](https://dotnet.microsoft.com/learn/aspnet/what-is-aspnet-core) - Web-development framework
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef) - ORM
- [PostgreSQL](https://www.postgresql.org) - Relational database
- [Serilog](https://serilog.net) - Logger
- [Datadog](https://www.datadoghq.com) - Monitoring
- [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle) to generate API documentation, available at /swagger
- [StyleCop](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) to enforce coding style

### Web UI (src/WebUI)

The UI was bootstrapped using [Vue CLI](https://cli.vuejs.org). It uses the following technologies:
- [Vue.js](https://vuejs.org) - JavaScript framework
- [TypeScript](https://www.typescriptlang.org) for typing
- [Bulma](https://bulma.io) - CSS framework
- [Buefy](https://buefy.org) - Vue wrapper of Bulma
- [Sass](https://sass-lang.com) - CSS pre-processor
- [Font Awesome](https://fontawesome.com) for icons
- [Eslint](https://eslint.org) to enforce coding style and best practices. Use `yarn lint` to fix your code

## Run

### Web API (src/WebApi)

- Download your favorite IDE: [Visual Studio](https://visualstudio.microsoft.com/vs), [Visual Studio Code](https://code.visualstudio.com), [Rider](https://www.jetbrains.com/rider)...
- Download [.NET Core SDK](https://dotnet.microsoft.com/download)
- Set the `Steam:ApiKey` in [src/WebApi/appsettings.Development.json](https://github.com/verdie-g/cRPG/blob/master/src/WebApi/appsettings.Development.json),
  acquired by [filling out this form](https://steamcommunity.com/dev/apikey)
- Open the solution file Crpg.sln
- Run `dotnet dev-certs https --trust` to be able to launch the API with HTTPS. The Steam authentication creates a cookie
  with `SameSite=None` and recent version of Chrome requires HTTPS to do so
- Build and run (can be done without IDE using [dotnet cli](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-run))


### Web UI (src/WebUI)

- Go to src/WebUI
- Install [yarn](https://classic.yarnpkg.com/en/docs/install)
- Run `yarn install` to install dependencies
- Run `yarn serve` to launch the application

The client relies on the server so you have to run both.
