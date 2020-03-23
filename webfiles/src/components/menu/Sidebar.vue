<template>
    <div class="sidebar mobile" v-if="isMobile">
        <div class="sidebar-backdrop fixed left-0 top-0 w-full h-full" @click="closeSidebarPanel" v-if="isPanelOpen"></div>

        <transition name="slide">
            <div v-if="isPanelOpen" class="sidebar-panel bg-background text-on-background left-0 top-0 fixed h-full p-2 transition-colors duration-500 w-screen">
                <slot></slot>
            </div>
        </transition>
    </div>
    <div class="sidebar border-r-2 border-brand md:max-w-xs" v-else>
        <div class="sidebar-panel p-2">
            <slot></slot>
        </div>
    </div>
</template>

<script>
    import { store, mutations } from '../../store.js'

    export default {
        methods: {
            closeSidebarPanel: mutations.toggleSideNav
        },
        computed: {
            isPanelOpen() {
                return store.isSideNavOpen
            },
            isMobile() {
                return this.$globalMethods.IsMobile();
            }
        }
    }
</script>

<style scoped>
    .slide-enter-active,
    .slide-leave-active {
        transition: transform 0.2s ease;
    }

    .slide-enter,
    .slide-leave-to {
        transform: translateX(-100%);
        transition: all 150ms ease-in 0s
    }

    .sidebar-backdrop {
        background-color: rgba(0,0,0,.5);
        z-index: 9997;
        cursor: pointer;
    }

    .sidebar-panel {
        overflow-y: auto;
    }

    .sidebar-panel {
        z-index: 9998;
    }
</style>