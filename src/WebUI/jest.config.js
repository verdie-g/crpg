module.exports = {
  preset: '@vue/cli-plugin-unit-jest/presets/typescript',
  testEnvironment: 'jsdom',
  transformIgnorePatterns: ['/node_modules/'],
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/src/$1',
  },
  transform: {
    '^.+\\.json?$': '<rootDir>/src/tests/unit/json5-jest.js',
  },
  testMatch: ['**/*.spec.(js|jsx|ts|tsx)'],
  forceExit: !!process.env.CI,
};
