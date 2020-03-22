const tailwind = require('tailwindcss/defaultTheme');

module.exports = {
    theme: {},
    variants: {
        backgroundColor: ['responsive', 'hover', 'focus', 'active'],
    },
    plugins: [
        require('./theme.config.js')
    ],
};