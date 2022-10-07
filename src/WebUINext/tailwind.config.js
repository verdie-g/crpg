// https://tailwindcss.com/docs/installation
const colors = require('tailwindcss/colors');

// @ts-check
/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['index.html', './src/**/*.{js,jsx,ts,tsx,vue,html}'],
  theme: {
    // TODO:
    screens: {
      sm: '640px',
      md: '768px',
      lg: '1024px',
      xl: '1280px',
      '2xl': '1536px',
    },

    // TODO:
    // colors: {
    //   transparent: 'transparent',
    //   current: 'currentColor',
    //   white: '#ffffff',
    // },

    ////////////////////////////////

    extend: {
      colors: {
        main: {
          DEFAULT: colors.gray[700],
          dark: '#ffffff',
        },
        primary: {
          DEFAULT: '#1fb6ff',
          dark: '#ffffff',
        },
        highlight: {
          DEFAULT: colors.red[700],
          dark: colors.violet[800],
        },
        'highlight-background': {
          DEFAULT: colors.yellow[400],
          dark: '#1fb6ff',
        },
      },
    },
  },
  plugins: [],
};
