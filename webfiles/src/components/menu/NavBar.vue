<template>
    <nav class="navbar flex items-center justify-between flex-wrap bg-brand p-6">
        <div :class="{ 'active' : !isNavBarOpen }">
            <div v-if="isMobile">
                <button @click="toggleNavBar" class="title">
                    <slot name="title"></slot>
                </button>

                <burger v-if="isMobile" class="fixed right-0 top-0 mt-5 mr-5" />
            </div>
            <slot name="title" v-else></slot>
        </div>

        <div :class="{ 'active' : isNavBarOpen }" class="content">
            <slot name="content"></slot>
        </div>
    </nav>
</template>

<script>
    import { store, mutations } from '../../store.js'
    import Burger from './Burger.vue';

    export default {
        computed: {
            isNavBarOpen() {
                return store.isNavBarOpen
            },
            isMobile() {
                return this.$globalMethods.IsMobile()
            }
        },
        methods: {
            toggleNavBar() {
                mutations.toggleNavBar()
            }
        },
        components: {
            Burger
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