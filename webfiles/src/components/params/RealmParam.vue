<template>
    <div :class="realm" class="realm px-2 mr-2 rounded"></div>
</template>

<script>
    export default {
        props: ['realm']
    }
</script>

<style scoped>
    div.realm.client {
        background-color: rgb(var(--color-client));
    }

    div.realm.server {
        background-color: rgb(var(--color-server));
    }

    div.realm.shared {
        background-color: rgb(var(--color-shared));
    }
</style>