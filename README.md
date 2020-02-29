# cRPG

cRPG is a mod for [Mount & Blade II: Bannerlord](https://store.steampowered.com/app/261550/Mount__Blade_II_Bannerlord)
that adds persistence (xp, gold, items, stats) to the multiplayer.

## Development

### Server (src/Web)

Server written in [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core).
Its design is inspired by [jasontaylordev/NorthwindTraders](https://github.com/jasontaylordev/NorthwindTraders),
see the [conference on Youtube](www.youtube.com/watch?v=Zygw4UAxCdg).

Use your favorite IDE to build, Visual Studio (Code) or Rider. A swagger UI is available on http://localhost:5000/swagger.

### Client (src/Web/Client)

Client written with [Vue.js](https://vuejs.org) and [TypeScript](https://www.typescriptlang.org).
The project was bootstraped using [Vue CLI](https://cli.vuejs.org).

To run it:
- install yarn
- `yarn install`
- `yarn serve`

The client relies on the server so you have to run both.

Use `yarn lint` to lint with [eslint](https://eslint.org).
