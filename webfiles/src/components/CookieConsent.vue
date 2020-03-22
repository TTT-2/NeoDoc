<template>
    <div v-if="show" class="cookie-consent fixed bg-brand w-full bottom-0 p-2 flex justify-around" role="dialog">
        <slot name="message">
            <span class="cookie-consent-message">
                {{ msg }}

                <slot name="link">
                    <a v-bind="{ href, target, rel }" class="cookie-consent-link text-highlight-on-brand underline" role="button">
                        {{ linkLabel }}
                    </a>
                </slot>
            </span>
        </slot>

        <section @click="dismiss">
            <slot name="button">
                <button class="cookie-consent-compliance bg-highlight-on-brand p-1 rounded whitespace-no-wrap flex-wrap" type="button">
                    {{ buttonLabel }}
                </button>
            </slot>
        </section>
    </div>
</template>

<script>
    export default {
        name: 'CookieConsent',
        props: {
            msg: {
                type: String,
                default: 'This website uses cookies to ensure you get the best experience on our website.'
            },
            // button
            linkLabel: {
                type: String,
                default: 'Learn more'
            },
            buttonLabel: {
                type: String,
                default: 'Got it!'
            },
            href: {
                type: String,
                default: 'http://cookiesandyou.com'
            },
            target: {
                type: String,
                default: '_blank'
            },
            rel: {
                type: String,
                default: 'noopener'
            },
            // cookie
            cookieName: {
                type: String,
                default: 'cookieconsent_status'
            },
            cookieExpirationTime: {
                type: String,
                default: '1Y'
            }
        },
        data() {
            return {
                show: undefined
            }
        },
        beforeMount() {
            this.show = !this.$cookie.get(this.cookieName)
        },
        methods: {
            dismiss() {
                this.show = false

                this.$cookie.set(this.cookieName, 1, { expires: this.cookieExpirationTime })
            }
        }
    }
</script>