<template>
    <div class="overflow-auto flex flex-col flex-grow">
        <div class="flex flex-col flex-grow">
            <div class="m-6 p-2 flex flex-col flex-grow rounded bg-main text-on-main transition duration-500">
                <slot />
            </div>
        </div>
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