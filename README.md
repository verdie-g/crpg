# cRPG

cRPG is a mod for [Mount & Blade II: Bannerlord](https://store.steampowered.com/app/261550/Mount__Blade_II_Bannerlord)
that adds persistence (xp, gold, items, stats) to the multiplayer.

## Development

### Web API (src/Web)

Server written in [.NET Core](https://dotnet.microsoft.com). Its design is inspired by
[jasontaylordev/NorthwindTraders](https://github.com/jasontaylordev/NorthwindTraders), see the
[conference on Youtube](www.youtube.com/watch?v=Zygw4UAxCdg). It uses the following technologies:
- [ASP.NET Core](https://dotnet.microsoft.com/learn/aspnet/what-is-aspnet-core) - Web-development framework
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef) - ORM
- [PostgreSQL](https://www.postgresql.org) - Relational database
- [Serilog](https://serilog.net) - Logger
- [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle) - to generate API documentation, available at http://localhost:5000/swagger
- [StyleCop](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) - to enforce coding style

To run it:
- download your favorite IDE: Visual Studio, Visual Studio Code, Rider...
- set the `Steam:ApiKey` in [src/Web/appsettings.Development.json](https://github.com/verdie-g/cRPG/blob/master/src/Web/appsettings.Development.json),
  acquired by [filling out this form](https://steamcommunity.com/dev/apikey)
- open the solution file Crpg.sln
- build and run

### Web UI (src/Web/Client)

The UI was bootstrapped using [Vue CLI](https://cli.vuejs.org). It uses the following technologies:
- [Vue.js](https://vuejs.org) - JavaScript framework
- [TypeScript](https://www.typescriptlang.org) - for typing
- [Bulma](https://bulma.io) - CSS framework
- [Buefy](https://buefy.org) - Vue wrapper of Bulma
- [Sass](https://sass-lang.com) - CSS pre-processor to write maintainable CSS
- [Font Awesome](https://fontawesome.com) - for icons
- [Eslint](https://eslint.org) - to enforce coding style and best practices. Use `yarn lint` to fix your code

To run it:
- go to src/Web/Client
- install yarn
- `yarn install`
- `yarn serve`

The client relies on the server so you have to run both.
