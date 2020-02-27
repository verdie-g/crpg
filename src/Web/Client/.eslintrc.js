module.exports = {
  root: true,
  env: {
    node: true,
  },
  extends: [
    'plugin:vue/essential',
    '@vue/airbnb',
    '@vue/typescript',
  ],
  rules: {
    'no-console': process.env.NODE_ENV === 'production' ? 'error' : 'off',
    'no-debugger': process.env.NODE_ENV === 'production' ? 'error' : 'off',
    'max-len': ['warn', { code: 140 }],
    'class-methods-use-this': 'off',
    'import/prefer-default-export': 'off',
    'lines-between-class-members': 'off',
    'no-param-reassign': 'off',
    'vue/valid-v-for': 'off',
    'vue/require-v-for-key': 'off',
    'no-return-assign': 'off',
  },
  parserOptions: {
    parser: '@typescript-eslint/parser',
  },
};
