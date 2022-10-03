module.exports = {
  preset: '@vue/cli-plugin-unit-jest/presets/typescript',
  //

  testEnvironment: 'jsdom',
  transformIgnorePatterns: ['/node_modules/'],
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/src/$1',
  },
  testMatch: ['**/*.spec.(js|jsx|ts|tsx)'],
  forceExit: !!process.env.CI,
};
