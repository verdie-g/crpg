// https://tailwindcss.com/docs/installation
// const colors = require('tailwindcss/colors');

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
        primary: '#D39A07',
        secondary: '#9E07D3',
        danger: '#E90202',
        neutral: '#FFF9E2',
        todo: '#675746', // need a name
        todo2: '#DECDBA', // need a name
        todo3: '#FFEDAF',
        //
        todo4: '#A69787', // checkbox border
        todo5: '#FEF2E6', // checkbox bg

        todo6: '#FAD4AC', // table td border
      },

      minWidth: {
        48: '12rem' /* 192px */,
        9: '2.25rem' /* 36px */,
      },
    },
  },

  plugins: [],
};
