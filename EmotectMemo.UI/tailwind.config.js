/** @type {import('tailwindcss').Config} */
const colors = require('tailwindcss/colors')

module.exports = {
  content: ["./src/**/*.{html,js}"],
  darkMode: 'selector',
  theme: {
    extend: {
      colors: {
        primary: '#111111',
        secondary: '#ffffff',
        emotral: {
          0: '#ffffff',
          2: '#fafafa',
          5: '#f3f3f3',
          10: '#e8e8e8',
          20: '#cfcfcf',
          30: '#b8b8b8',
          40: '#a0a0a0',
          50: '#878787',
          60: '#707070',
          70: '#595959',
          80: '#414141',
          90: '#292929',
          100: '#111111'
        }
      }
    },
    fontFamily:{
      body: ['Montserrat'],
      mono: ['ui-monospace', 'SFMono-Regular', 'Consolas']
    }
  },
  plugins: [],
}

