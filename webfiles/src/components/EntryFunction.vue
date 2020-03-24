<template>
    <div class="flex flex-grow flex-col">
        <div class="flex break-all justify-center flex-wrap flex-row mb-2 break-normal">
            <realm-param :realm="(jsonData.params.realm && jsonData.params.realm[0].data) || 'shared'" class="inline" />
            <span class="inline text-2xl">{{ jsonData.name }}</span>
        </div>

        <deprecated-param v-if="jsonData.params.deprecated" />

        <twod-param v-if="jsonData.params['2D']" />

        <threed-param v-if="jsonData.params['3D']" />

        <pretty-code>{{ jsonData.data[0] }} { ... }</pretty-code>

        <param-param v-if="jsonData.params.param" :params="jsonData.params.param" title="Parameter" />

        <param-param v-if="jsonData.params.return" :params="jsonData.params.return" title="Returns" />

        <div v-if="jsonData.params.desc && jsonData.params.desc[0].data != ''" class="mt-4">
            <title-text>Description:</title-text>

            <div class="ml-8 mt-2">
                {{ jsonData.params.desc[0].data }}
            </div>
        </div>
    </div>
</template>

<script>
    import TwodParam from '@/components/params/2DParam.vue';
    import ThreedParam from '@/components/params/3DParam.vue';
    import DeprecatedParam from '@/components/params/DeprecatedParam.vue';
    import ParamParam from '@/components/params/ParamParam.vue';
    import RealmParam from '@/components/params/RealmParam.vue';

    export default {
        props: ['jsonData'],
        components: {
            TwodParam,
            ThreedParam,
            DeprecatedParam,
            ParamParam,
            RealmParam
        }
    }
</script>