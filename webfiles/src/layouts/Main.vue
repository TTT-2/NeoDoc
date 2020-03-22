<template>
    <div class="container bg-background text-on-background transition duration-500">
        <header>
            <nav class="flex items-center justify-between flex-wrap bg-brand p-6">
                <div class="flex items-center flex-shrink-0 text-on-brand mr-6">
                    <span class="font-semibold text-xl tracking-tight">NeoDoc</span>
                    <Burger />
                </div>
                <div class="block lg:hidden">
                    <button class="flex items-center px-3 py-2 border rounded text-on-brand border-brand hover:text-on-brand-hover hover:border-on-brand-hover">
                        <svg class="fill-current h-3 w-3" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg"><title>Menu</title><path d="M0 3h20v2H0V3zm0 6h20v2H0V9zm0 6h20v2H0v-2z" /></svg>
                    </button>
                </div>
                <div class="w-full block flex-grow lg:flex lg:items-center lg:w-auto">
                    <div class="text-sm lg:flex-grow">
                        <v-link href="/home" class="block mt-4 lg:inline-block lg:mt-0 text-on-brand hover:text-on-brand-hover mr-4">Home</v-link>
                        <v-link href="/docu/hooks" class="block mt-4 lg:inline-block lg:mt-0 text-on-brand hover:text-on-brand-hover mr-4">Hooks</v-link>
                        <v-link href="/docu/createconvars" class="block mt-4 lg:inline-block lg:mt-0 text-on-brand hover:text-on-brand-hover mr-4">ConVars</v-link>
                        <v-link href="/docu/datastructures" class="block mt-4 lg:inline-block lg:mt-0 text-on-brand hover:text-on-brand-hover mr-4">Functions</v-link>
                    </div>
                    <div>
                        <a href="https://github.com/TTT-2/TTT2" class="inline-block text-sm px-4 py-2 leading-none border rounded text-on-brand-hover border-on-brand-hover hover:bg-on-brand hover:text-brand mt-4 lg:mt-0">GitHub</a>
                    </div>
                </div>
            </nav>

            <nav class="text-on-background font-bold m-2" aria-label="Breadcrumb">
                <ol class="list-none p-0 inline-flex">
                    <li class="flex items-center">
                        <a href="#">Home</a>
                        <svg class="fill-current w-3 h-3 mx-3" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 320 512"><path d="M285.476 272.971L91.132 467.314c-9.373 9.373-24.569 9.373-33.941 0l-22.667-22.667c-9.357-9.357-9.375-24.522-.04-33.901L188.505 256 34.484 101.255c-9.335-9.379-9.317-24.544.04-33.901l22.667-22.667c9.373-9.373 24.569-9.373 33.941 0L285.475 239.03c9.373 9.372 9.373 24.568.001 33.941z" /></svg>
                    </li>
                    <li class="flex items-center">
                        <a href="#">Second Level</a>
                        <svg class="fill-current w-3 h-3 mx-3" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 320 512"><path d="M285.476 272.971L91.132 467.314c-9.373 9.373-24.569 9.373-33.941 0l-22.667-22.667c-9.357-9.357-9.375-24.522-.04-33.901L188.505 256 34.484 101.255c-9.335-9.379-9.317-24.544.04-33.901l22.667-22.667c9.373-9.373 24.569-9.373 33.941 0L285.475 239.03c9.373 9.372 9.373 24.568.001 33.941z" /></svg>
                    </li>
                    <li>
                        <a href="#" class="text-brand" aria-current="page">Third Level</a>
                    </li>
                </ol>
            </nav>

            <Sidebar>
                <ul class="sidebar-panel-nav">
                    <li><a href="#home">Home</a></li>
                    <li><a href="#about">About</a></li>
                    <li><a href="#contact">Contact</a></li>
                </ul>
            </Sidebar>
        </header>

        <main-container>
            <slot></slot>
        </main-container>

        <footer class="flex justify-around bg-brand p-2 text-on-brand">
            <v-link href="/impressum" class="block pb-4 mt-4 lg:inline-block hover:text-on-brand-hover">Impressum</v-link>

            <div class="w-20 flex justify-around">
                <button @click="theme()" class="hover:text-on-brand-hover"><svg aria-hidden="true" focusable="false" data-prefix="fas" data-icon="adjust" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"><path fill="currentColor" d="M8 256c0 136.966 111.033 248 248 248s248-111.034 248-248S392.966 8 256 8 8 119.033 8 256zm248 184V72c101.705 0 184 82.311 184 184 0 101.705-82.311 184-184 184z"></path></svg></button>
                <button @click="theme('light')" class="hover:text-on-brand-hover"><svg aria-hidden="true" focusable="false" data-prefix="fas" data-icon="sun" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"><path fill="currentColor" d="M256 160c-52.9 0-96 43.1-96 96s43.1 96 96 96 96-43.1 96-96-43.1-96-96-96zm246.4 80.5l-94.7-47.3 33.5-100.4c4.5-13.6-8.4-26.5-21.9-21.9l-100.4 33.5-47.4-94.8c-6.4-12.8-24.6-12.8-31 0l-47.3 94.7L92.7 70.8c-13.6-4.5-26.5 8.4-21.9 21.9l33.5 100.4-94.7 47.4c-12.8 6.4-12.8 24.6 0 31l94.7 47.3-33.5 100.5c-4.5 13.6 8.4 26.5 21.9 21.9l100.4-33.5 47.3 94.7c6.4 12.8 24.6 12.8 31 0l47.3-94.7 100.4 33.5c13.6 4.5 26.5-8.4 21.9-21.9l-33.5-100.4 94.7-47.3c13-6.5 13-24.7.2-31.1zm-155.9 106c-49.9 49.9-131.1 49.9-181 0-49.9-49.9-49.9-131.1 0-181 49.9-49.9 131.1-49.9 181 0 49.9 49.9 49.9 131.1 0 181z"></path></svg></button>
                <button @click="theme('dark')" class="hover:text-on-brand-hover"><svg aria-hidden="true" focusable="false" data-prefix="fas" data-icon="moon" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"><path fill="currentColor" d="M283.211 512c78.962 0 151.079-35.925 198.857-94.792 7.068-8.708-.639-21.43-11.562-19.35-124.203 23.654-238.262-71.576-238.262-196.954 0-72.222 38.662-138.635 101.498-174.394 9.686-5.512 7.25-20.197-3.756-22.23A258.156 258.156 0 0 0 283.211 0c-141.309 0-256 114.511-256 256 0 141.309 114.511 256 256 256z"></path></svg></button>
            </div>
        </footer>
    </div>
</template>

<script>
    import json from '../jsonList.json'
    import Burger from '../components/menu/Burger.vue';
    import Sidebar from '../components/menu/Sidebar.vue';

    export default {
        name: 'main-layout',
        data() {
            return {
                jsonList: json,
                themes: ["light", "dark"],
            }
        },
        methods: {
            theme(name) {
                document.body.removeAttribute('data-theme')
                this.$cookie.delete('theme')

                if (name) {
                    document.body.setAttribute('data-theme', name)
                    this.$cookie.set('theme', name, { expires: '1Y' }) // expires after 1 year
                }
            }
        },
        mounted() {
            let recaptchaScript = document.createElement('script')
            recaptchaScript.setAttribute('src', 'https://cdn.jsdelivr.net/gh/google/code-prettify@master/loader/run_prettify.js')

            document.head.appendChild(recaptchaScript)

            this.theme(this.$cookie.get('theme'))
        },
        components: {
            Burger,
            Sidebar
        }
    }
</script>

<style scoped>
    ul.sidebar-panel-nav {
        list-style-type: none;
    }

    ul.sidebar-panel-nav > li > a {
        color: rgb(var(--color-on-brand));
        text-decoration: none;
        font-size: 1.5rem;
        display: block;
        padding-bottom: 0.5em;
    }
</style>