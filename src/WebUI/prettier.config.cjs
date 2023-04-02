// @ts-check
/** @type {import('prettier').Options} */
module.exports = {
  semi: true,
  singleQuote: true,
  bracketSpacing: true,
  printWidth: 100,
  tabWidth: 2,
  htmlWhitespaceSensitivity: 'ignore',
  arrowParens: 'avoid',
  trailingComma: 'es5',
  plugins: [require('prettier-plugin-tailwindcss')],
};
