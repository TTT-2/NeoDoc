/*
 |--------------------------------------------------------------------------
 | Tailwind Theming
 |--------------------------------------------------------------------------
 |
 | This configuration file holds the theme settings of your
 | application. This value will be imported into your CSS as CSS 
 | variables so you can use a powerful theming management client-side.
 |
 | For more informations, see here: 
 | https://github.com/hawezo/tailwindcss-theming
 |
 */

const { ThemeBuilder, Theme } = require('tailwindcss-theming');

const lightTheme = new Theme()
    .name('light')
    .default()
    .assignable()
    .colors({
        // A transparent color, which alpha value will be detected.
        'transparent': 'transparent',

        // Brand colors
        'brand': '#2196f3',
        'on-brand': '#ffffff',
        'on-brand-hover': '#eeeeee',
        'highlight-on-brand': '#f66110',

        // Background colors, but not limited to `bg` utilities
        'background': '#f4f4f4',
        'on-background': '#585851',

        'main': '#eeeeee',
        'on-main': '#333333',

        // Event colors
        'error-primary': '#b00020',
        'on-error-primary': '#ffffff',
        'error-secondary': '#a00010',
        'on-error-secondary': '#dddddd',

        'success-primary': '#3ab577',
        'on-success-primary': '#ffffff',
        'success-secondary': '#2aa567',
        'on-success-secondary': '#dddddd',

        'warning-primary': '#e65100',
        'on-warning-primary': '#ffffff',
        'warning-secondary': '#d64100',
        'on-warning-secondary': '#dddddd',

        'info-primary': '#2481ea',
        'on-info-primary': '#ffffff',
        'info-secondary': '#1471da',
        'on-info-secondary': '#dddddd',

        'code': '#cccccc',
        'on-code': '#000000',

        'client': '#e3ab4b',
        'server': '#2488d4',
        'shared': '#5ae34b',
    })

    // Color variants
    .colorVariant('hover', 'white', ['on-navigation'])

    // Material variants
    .opacityVariant('high-emphasis', .87)
    .opacityVariant('medium-emphasis', .60)
    .opacityVariant('disabled', .38)
    .opacityVariant('helper-emphasized', .87)
    .opacityVariant('helper', .6)
    .opacityVariant('inactive', .6)

    // Arbitrary variants
    .opacityVariant('quote-border', .5)
    .opacityVariant('muted', .38)
    .opacityVariant('kinda-visible', .1)
    .opacityVariant('slightly-visible', .075)
;

const darkTheme = new Theme()
    .name('dark')
    .colors({
        // Background colors, but not limited to `bg` utilities
        'background': '#1f1f1f',
        'on-background': '#dddddd',

        'on-brand': '#dddddd',

        'main': '#333333',
        'on-main': '#dddddd',
    })

    // Arbitrary variants
    .opacityVariant('quote-border', .15)
    .opacityVariant('kinda-visible', .038)
    .opacityVariant('slightly-visible', .020)
;

module.exports = new ThemeBuilder()
    .asDataThemeAttribute()
    .default(lightTheme)
    .dark(darkTheme);