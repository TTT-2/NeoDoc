<template>
    <div>
        <title-text>{{ title }}:</title-text>

        <ul class="ml-8">
            <li v-for="entry in params" class="mt-4">
                <docu-param :name="entry.name" :type="entry.type" :description="entry.description" :optional="entry.optional" :optionalchain="entry.optionalchain" :defaultval="entry.default" />
            </li>
        </ul>
    </div>
</template>

<script>
    import DocuParam from './DocuParam.vue';

    export default {
        props: [
            'params',
            'title'
        ],
        components: {
            DocuParam
        }
    }
</script>