# Dear developer
## Env

### Using env variables in code
`import.meta.env.VITE_XXX_XXX_XXX`

[ref](https://vitejs.dev/guide/env-and-mode.html)


### Type declaration for user defined env variables

In the `src/types/vite-env.d.ts` add a field to the interface `ImportMetaEnv`

[ref](https://vitejs.dev/guide/env-and-mode.html#intellisense-for-typescript)

___

## Unit testing

[Vitest](https://vitest.dev/) was chosen as the framework for unit testing. Is very similar to Jest.

Vue components test with [VTU](https://test-utils.vuejs.org/).

Please do not forget to write tests, it is **very important**.
By writing a test, you can prove that your code **works correctly**
Also, it's a **self-documenting** tool.

Examples
- regular ts: `utils/color.spec.ts`
- service (with API call stubs): `services/crpg-client.spec.ts`, `services/example.spec.ts`
- vue 3 component: `components/Example.spec.ts`
- vue 3 page: TODO:
- vue 3 composable:  `composables/example.spec.ts`
- pinia store: `stores/example.spec.ts`
- router middleware (guard):  `middlewares/example.spec.ts`

TODO:
// mock global components `src/__test__/unit/setup.ts`
// testing a component, it is necessary to mock compostables, ref: https://github.com/vuejs/test-utils/issues/775#issuecomment-883189164
// testing a page with router & suspense (async setup), `src/__test__/unit/utils.ts` ref: https://github.com/vuejs/test-utils/issues/108#issuecomment-1124851726

### Tips

- there are [snapshots](https://vitest.dev/guide/snapshot.html) in some tests. To update snapshot, you must run `npm run test:unit -- -u`
- to check the [test-coverage](https://vitest.dev/guide/coverage.html), you can use the command `npm run test:unit-c8`
- dir for [mock-data](https://vitest.dev/guide/mocking.html) `src/__mocks__`
- global [setupfile](https://vitest.dev/config/#setupfiles) - `src/__test__/unit/index.ts`

___

## Functional testing

TODO: playwright/cypress

___

## HTML/CSS
We don't need to write CSS styles because we use [TailwindCSS framework](https://tailwindcss.com/)

Is a very handy tool for working in a large team. The `tailwind.config.js`

TODO: suggestions in js files (VS Code) - add comment /*@tw*/
"tailwindCSS.experimental.classRegex": [
    ["/\\*@tw\\*/ ([^;]*);", "'([^']*)'"]
]
### Tips
- [Tailwind config viewer](https://github.com/rogden/tailwind-config-viewer) - `npm run tailwind-config-viewer`
- we use automatic sorting of classes provied by [prettier-plugin-tailwindcss](https://github.com/tailwindlabs/prettier-plugin-tailwindcss)
- plugins for tooltips and autocompletes in the IDE: [VSCode](https://marketplace.visualstudio.com/items?itemName=bradlc.vscode-tailwindcss), [JetBrains](https://www.jetbrains.com/help/webstorm/tailwind-css.html)

___

## UI Library

**[Oruga](https://oruga.io/) + [TailwindCSS](https://tailwindcss.com/) = 😻**

We use the OrugaUI along with TailwindCSS.

Theme dir (CSS + icons): `src/assets/themes`

Available global components `src/boot/oruga-ui.ts`

Playground: `/playground/oruga`

### Icon
Just add an SVG icon as in the example: `src/assets/themes/oruga-tailwind/icons/arrow-up.ts`


### Image

TODO:

### Caveat

Waiting for these updates for a better DX

- https://github.com/oruga-ui/oruga/issues/263
- https://github.com/oruga-ui/oruga/pull/270
## CI/CD:

TODO: Lighthouse budget

___
