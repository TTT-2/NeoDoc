<template>
    <nav class="navbar flex items-center justify-between flex-wrap bg-brand p-6">
        <div :class="{ 'active' : !isNavBarOpen }" class="title">
            <slot name="title"></slot>
        </div>

        <div :class="{ 'active' : isNavBarOpen }" class="content">
            <slot name="content"></slot>
        </div>

        <div class="block lg:hidden">
            <button @click="toggleNavBar" class="flex items-center px-3 py-2 border rounded text-on-brand border-brand hover:text-on-brand-hover hover:border-on-brand-hover">
                <svg class="fill-current h-3 w-3" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg"><title>Menu</title><path d="M0 3h20v2H0V3zm0 6h20v2H0V9zm0 6h20v2H0v-2z" /></svg>
            </button>
        </div>
    </nav>
</template>

<script>
    import { store, mutations } from '../../store.js'

    export default {
        computed: {
            isNavBarOpen() {
                return store.isNavBarOpen
            }
        },
        methods: {
            toggleNavBar() {
                mutations.toggleNavBar()
            }
        }
    }
</script>

<style scoped>
    nav.navbar > .content {
        display: none;
    }
    nav.navbar > .title {
        display: none;
    }

    /* width: lg */
    @media (min-width: 1024px) {
        nav.navbar > .content {
            display: block;
        }
    }
    @media (min-width: 1024px) {
        nav.navbar > .title {
            display: block;
        }
    }

    nav.navbar > .content.active {
        display: block;
    }
    nav.navbar > .title.active {
        display: block;
    }
</style>