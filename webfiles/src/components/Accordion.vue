<template>
    <div class="">
        <div class="tab__header">
            <a href="#" class="tab__link p-4 block bg-blue-dark hover:bg-blue-darker no-underline text-white border-b-2 border-white flex justify-between" @click.prevent="active = !active">
                <slot name="title" />
                <span class="down-Arrow" v-show="!isActive">&#9660;</span>
                <span class="up-Arrow" v-show="isActive">&#9650;</span>
            </a>
        </div>

        <div class="tab__content p-2" v-show="isActive">
            <slot name="content" />
        </div>
    </div>
</template>

<script>
    export default {
        props: [
            'forceActive'
        ],
        data() {
            return {
                active: false,
            }
        },
        computed: {
            isActive() {
                return this.forceActive || this.active;
            }
        }
    }
</script>