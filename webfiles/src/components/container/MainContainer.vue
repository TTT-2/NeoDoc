<template>
    <div class="m-6 p-2 rounded bg-main text-on-main transition duration-500">
         <slot></slot>
    </div>
</template>

<script>
    import { store, mutations } from '../../store.js'

    export default {
        computed: {
            isBurgerActive() {
                return store.isNavOpen
            }
        },
        methods: {
            toggle() {
                mutations.toggleNav()
            }
        }
    }
</script>