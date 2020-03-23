<template>
    <nav class="border-b-2 border-brand">
        <div class="text-on-background font-bold m-2" aria-label="Breadcrumb">
            <ol class="list-none p-0 inline-flex">
                <li v-for="(pathPart, index) in getPaths" :key="index" class="flex items-center">
                    <v-link :href="pathPart.fullPath" :class="{ 'text-brand' : index == (getPaths.length - 1) }">{{ pathPart.path }}</v-link>
                    <svg v-if="index != (getPaths.length - 1)" class="fill-current w-3 h-3 mx-3" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 320 512"><path d="M285.476 272.971L91.132 467.314c-9.373 9.373-24.569 9.373-33.941 0l-22.667-22.667c-9.357-9.357-9.375-24.522-.04-33.901L188.505 256 34.484 101.255c-9.335-9.379-9.317-24.544.04-33.901l22.667-22.667c9.373-9.373 24.569-9.373 33.941 0L285.475 239.03c9.373 9.372 9.373 24.568.001 33.941z" /></svg>
                </li>
            </ol>
        </div>
    </nav>
</template>

<script>
    export default {
        props: ['paths'],
        computed: {
            getPaths() {
                var fullPath = "/";
                var pathSplits = [];

                for (var i = 0; i < this.paths.length; i++) {
                    fullPath += this.paths[i]

                    pathSplits[i] = { path: this.paths[i], fullPath: fullPath }

                    if (i != this.paths.length) {
                        fullPath += "/"
                    }
                }

                return pathSplits
            }
        }
    }
</script>