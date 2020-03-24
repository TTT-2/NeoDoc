<template>
    <div class="docu-param">
        <p><span class="underline">{{ name }}</span><span v-if="defaultval"> - default: {{ defaultval }}</span> (<span v-for="(typ, index) in type"><span class="text-brand">{{ typ }}</span>{{ index < type.length - 1 ? ' or ' : '' }}</span>)<span v-if="optional || optionalchain">: Optional</span></p>
        <p class="ml-6 my-2">{{ description }}</p>
    </div>
</template>

<script>
    export default {
        props: [
            'name',
            'type',
            'description',
            'optional',
            'optionalchain',
            'defaultval'
        ]
    }
</script>