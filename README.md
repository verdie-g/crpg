# cRPG

cRPG is a mod for [Mount & Blade II: Bannerlord](https://store.steampowered.com/app/261550/Mount__Blade_II_Bannerlord)
that adds persistence (xp, gold, items, stats) to the multiplayer.

## Development

### Web API (src/WebApi)

Server written in [.NET Core](https://dotnet.microsoft.com). Its design is inspired by
[jasontaylordev/NorthwindTraders](https://github.com/jasontaylordev/NorthwindTraders), see the
[conference on Youtube](www.youtube.com/watch?v=Zygw4UAxCdg). It uses the following technologies:
- [ASP.NET Core](https://dotnet.microsoft.com/learn/aspnet/what-is-aspnet-core) - Web-development framework
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef) - ORM
- [PostgreSQL](https://www.postgresql.org) - Relational database
- [Serilog](https://serilog.net) - Logger
- [Datadog](https://www.datadoghq.com) - Monitoring
- [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle) - to generate API documentation, available at http://localhost:5000/swagger
- [StyleCop](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) - to enforce coding style

To run it:
- download your favorite IDE: Visual Studio, Visual Studio Code, Rider...
- set the `Steam:ApiKey` in [src/WebApi/appsettings.Development.json](https://github.com/verdie-g/cRPG/blob/master/src/WebApi/appsettings.Development.json),
  acquired by [filling out this form](https://steamcommunity.com/dev/apikey)
- open the solution file Crpg.sln
- build and run

### Web UI (src/WebUI)

The UI was bootstrapped using [Vue CLI](https://cli.vuejs.org). It uses the following technologies:
- [Vue.js](https://vuejs.org) - JavaScript framework
- [TypeScript](https://www.typescriptlang.org) - for typing
- [Bulma](https://bulma.io) - CSS framework
- [Buefy](https://buefy.org) - Vue wrapper of Bulma
- [Sass](https://sass-lang.com) - CSS pre-processor to write maintainable CSS
- [Font Awesome](https://fontawesome.com) - for icons
- [Eslint](https://eslint.org) - to enforce coding style and best practices. Use `yarn lint` to fix your code

To run it:
- go to src/WebUI
- install yarn
- `yarn install`
- `yarn serve`

The client relies on the server so you have to run both.

## Deployment

### Web API Configuration

First, copy `appsettings.Development.json` to a new file `appsettings.Production.json` and update
the following keys:
- `Steam:ApiKey` [using this form ](https://steamcommunity.com/dev/apikey)
- `Jwt:Secret` with 64 random characters. Generated with [passwordsgenerator.net](https://passwordsgenerator.net)
  for instance. Note that changing this value will invalidate all JWT issued  with the old value.
- `Serilog:WriteTo:0` with 
```
{
  "Name": "File",
  "Args": {
    "Path": "/path/to/log/defined/in/datadog.yml",
    "Formatter": "Serilog.Formatting.Json.JsonFormatter",
    "Buffered": true
  }
}
```
- `ConnectionStrings:Crpg` with the PostgreSQL connection string

## PostgreSQL

Follow this guide: [First steps](https://wiki.postgresql.org/wiki/First_steps).

## Datadog

Follow this guides:
- [Installation](https://docs.datadoghq.com/videos/installation)
- [C# log collection](https://docs.datadoghq.com/logs/log_collection/csharp/?tab=serilog)

## Web API

The Web API is not meant to be used as a front facing server because [Kestrel](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel)
is not a full featured web server unlike [NGINX](https://www.nginx.com) or [IIS](https://www.iis.net). So,
use one of the latter and follow this guide to [host and deploy an ASP.NET Core app](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy).

## Web UI

TBD
