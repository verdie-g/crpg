module.exports = {
  root: true,
  env: {
    node: true,
  },
  extends: ['plugin:vue/essential', '@vue/prettier', '@vue/typescript/recommended'],
  rules: {
    '@typescript-eslint/explicit-module-boundary-types': 'off',
    '@typescript-eslint/no-explicit-any': 'off',
    '@typescript-eslint/no-non-null-assertion': 'off',
    'arrow-parens': ['error', 'as-needed'],
    'class-methods-use-this': 'off',
    'import/prefer-default-export': 'off',
    'lines-between-class-members': 'off',
    'max-classes-per-file': 'off',
    'no-console': process.env.NODE_ENV === 'production' ? 'error' : 'off',
    'no-debugger': process.env.NODE_ENV === 'production' ? 'error' : 'off',
    'no-mixed-operators': [
      'error',
      {
        groups: [
          ['&', '|', '^', '~', '<<', '>>', '>>>'],
          ['==', '!=', '===', '!==', '>', '>=', '<', '<='],
          ['&&', '||'],
          ['in', 'instanceof'],
        ],
        allowSamePrecedence: true,
      },
    ],
    'no-param-reassign': 'off',
    'no-return-assign': 'off',
    'no-shadow': 'off',
    'vue/require-v-for-key': 'off',
    'vue/valid-v-for': 'off',
  },
};
