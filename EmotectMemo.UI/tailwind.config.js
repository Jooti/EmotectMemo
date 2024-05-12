/** @type {import('tailwindcss').Config} */
const colors = require('tailwindcss/colors')

module.exports = {
  content: ["./src/**/*.{html,js}"],
  theme: {
    extend: {
      colors: {
        primary: '#111111',
        secondary: '#ffffff',
        neutral: {
          180: '#e8e8e8',
        },
      }
    },
    fontFamily:{
      body: ['Montserrat']
    }
  },
  plugins: [],
}

