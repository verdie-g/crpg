module.exports = {
  ignorePatterns: ['node_modules', 'dist', '.eslintrc.js'],
  root: true,
  env: {
    node: true,
  },
  extends: [
    'plugin:vue/vue3-recommended',
    'plugin:vue-pug/vue3-recommended',
    '@vue/prettier',
    '@vue/typescript',
    'plugin:prettier/recommended',
  ],
  parserOptions: {
    parser: '@typescript-eslint/parser',
  },
  plugins: ['@typescript-eslint', 'prettier'],
  rules: {
    'vue/multi-word-component-names': 'off',
  },
  globals: {
    env: 'readable',
  },
};
