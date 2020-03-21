<template>
    <div class="docu-param">
        <p><span class="underline">{{ name }}</span> (<span class="text-brand">{{ type }}</span>)</p>
        <p class="ml-6 my-2">{{ text }}</p>
    </div>
</template>

<script>
    export default {
        props: [
            'name',
            'type',
            'text'
        ]
    }
</script>