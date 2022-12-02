// @ts-check
/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['index.html', './src/**/*.{js,jsx,ts,tsx,vue,html}'],
  theme: {
    container: {
      center: true,
      padding: {
        DEFAULT: '1rem',
      },
    },

    // TODO:
    screens: {
      sm: '640px',
      md: '768px',
      lg: '1024px',
      xl: '1280px',
      '2xl': '1536px',
    },

    // TODO:
    colors: {
      transparent: 'transparent',
      current: 'currentColor',
      inherit: 'inherit',

      white: '#ffffff',
      black: '#000000',

      primary: {
        DEFAULT: '#D2BB8A',
        hover: '#AE9257',
        // disabled =  DEFAULT + 30% opacity
      },

      bg: {
        main: '#0F0F0E',
      },

      content: {
        100: '#FFFFFF',
        200: '#B6B6B6',
        300: '#8F8F8F',
        400: '#626262',
        500: '#3F3F3F',
        600: '#0D0D0D',

        link: {
          DEFAULT: '#D2BB8A',
          hover: '#AE9257',
        },
      },

      base: {
        100: '#0F0F0E',
        200: '#171716',
        300: '#272723',
        400: '#3C3B36',
        500: '#555552',
        600: '#FFFFFF',
      },

      border: {
        200: '#232527',
        300: '#404040',
      },

      status: {
        success: '#53825A',
        danger: '#BF3838',
        warning: '#C29F47',
      },

      more: {
        support: '#C99E34',
      },
    },

    fontFamily: {
      sans: [
        'Merriweather',
        'ui-sans-serif',
        'system-ui',
        '-apple-system',
        'BlinkMacSystemFont',
        '"Segoe UI"',
        'Roboto',
        '"Helvetica Neue"',
        'Arial',
        '"Noto Sans"',
        'sans-serif',
        '"Apple Color Emoji"',
        '"Segoe UI Emoji"',
        '"Segoe UI Symbol"',
        '"Noto Color Emoji"',
      ],
    },

    extend: {
      fontSize: {
        'title-hero': ['42px', '48px'],
        'title-lg': ['21px', '27px'],
        'title-md': ['15px', '21px'],
        '3xl': ['42px', '48px'],
        '2xl': ['36px', '42px'],
        xl: ['32px', '38px'],
        lg: ['24px', '28px'],
        sm: ['18px', '23px'],
        xs: ['15px', '21px'],
        '2xs': ['12px', '18px'],
      },

      minWidth: {
        initial: 'initial',
        9: '2.25rem' /* 36px */,
        36: '9rem' /* 144px */,
        48: '12rem' /* 192px */,
        60: '15rem' /* 240px */,
      },

      minHeight: {
        // screen: 'calc(100vh + 1px)',
        screen: '100vh',
      },

      spacing: {
        4.5: '1.125rem' /* 18px */,
        10.5: '2.625rem' /* 42px */,
        13.5: '3.375rem' /* 54px */,
      },

      zIndex: {
        100: 100,
      },

      opacity: {
        15: 0.15,
      },

      borderRadius: {
        sm: '0.188rem', // 2px
      },

      // ref: https://github.com/tailwindlabs/tailwindcss-typography/blob/master/src/styles.js
      typography: ({ theme }) => {
        // console.log(theme('spacing'));
        return {
          DEFAULT: {
            css: {
              fontSize: theme('fontSize.xs[0]'),
              lineHeight: theme('fontSize.xs[1]'),
              '--tw-prose-invert-counters': theme('colors.content.100'),
              li: {
                marginTop: theme('spacing')['2.5'],
                marginBottom: theme('spacing')['2.5'],
              },
            },
          },

          invert: {
            css: {
              '--tw-prose-body': theme('colors.content.200'),
              // TODO:!
              '--tw-prose-headings': 'var(--tw-prose-invert-headings)',
              '--tw-prose-lead': 'var(--tw-prose-invert-lead)',
              '--tw-prose-links': 'var(--tw-prose-invert-links)',
              '--tw-prose-bold': 'var(--tw-prose-invert-bold)',
              '--tw-prose-counters': 'var(--tw-prose-invert-counters)',
              '--tw-prose-bullets': 'var(--tw-prose-invert-bullets)',
              '--tw-prose-hr': 'var(--tw-prose-invert-hr)',
              '--tw-prose-quotes': 'var(--tw-prose-invert-quotes)',
              '--tw-prose-quote-borders': 'var(--tw-prose-invert-quote-borders)',
              '--tw-prose-captions': 'var(--tw-prose-invert-captions)',
              '--tw-prose-code': 'var(--tw-prose-invert-code)',
              '--tw-prose-pre-code': 'var(--tw-prose-invert-pre-code)',
              '--tw-prose-pre-bg': 'var(--tw-prose-invert-pre-bg)',
              '--tw-prose-th-borders': 'var(--tw-prose-invert-th-borders)',
              '--tw-prose-td-borders': 'var(--tw-prose-invert-td-borders)',
            },
          },
        };
      },
    },
  },

  plugins: [
    require('@tailwindcss/typography'),
    require('./src/assets/themes/oruga-tailwind/forms'), // TODO:
  ],
};
