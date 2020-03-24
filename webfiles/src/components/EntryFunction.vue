<template>
    <div class="flex flex-grow flex-col">
        <div class="flex justify-center mb-2">
            <realm-param :realm="jsonData.params.realm[0].data || 'shared'" class="inline" />
            <span class="inline text-4xl">{{ jsonData.name }}</span>
        </div>

        <twod-param v-if="jsonData.params['2D']" />

        <threed-param v-if="jsonData.params['3D']" />

        <pretty-code>{{ jsonData.data[0] }} { ... }</pretty-code>

        <ul v-if="jsonData.params.param">
            <li v-for="entry in jsonData.params.param">
                <docu-param :name="entry.name" :type="entry.type" :description="entry.description" />
            </li>
        </ul>

        <div v-if="jsonData.params.return">
            <h2>Returns:</h2>

            <ul>
                <li v-for="entry in jsonData.params.return">
                    <docu-param :name="entry.name" :type="entry.type" :description="entry.description" />
                </li>
            </ul>
        </div>

        <div v-if="jsonData.params.desc && jsonData.params.desc[0].data != ''">
            <h2>Description:</h2>

            {{ jsonData.params.desc[0].data }}
        </div>
    </div>
</template>

<script>
    import TwodParam from '@/components/params/2DParam.vue';
    import ThreedParam from '@/components/params/3DParam.vue';

    export default {
        props: ['jsonData'],
        components: {
            TwodParam,
            ThreedParam
        }
    }
</script>