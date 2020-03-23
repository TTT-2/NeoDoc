<template>
    <div>
        <ul>
            <li v-for="(entry, name) in jsonData.data">
                <div v-if="entry.sections">
                    <v-link :href="'./' + transformURI(name)">{{ name }}</v-link>

                    <ul class="ml-8">
                        <li v-for="(sectionEntry, sectionName) in entry.sections">
                            <h2>{{ sectionName }}</h2>

                            <ul class="ml-8">
                                <li v-for="(dsList, ds) in sectionEntry">
                                    <v-link :href="'./' + transformURI(name) + '/' + transformURI(ds)">{{ ds }}</v-link>

                                    <ul class="ml-8">
                                        <li v-for="dsEntry in dsList">
                                            <v-link :href="'./' + transformURI(name) + '/' + transformURI(ds) + '/' + transformURI(dsEntry)">{{ dsEntry }}</v-link>
                                        </li>
                                    </ul>
                                </li>
                            </ul>
                        </li>
                    </ul>
                </div>
                <v-link v-else :href="'./' + transformURI(jsonData.name) + 's/' + transformURI(entry)">{{ entry }}</v-link>
            </li>
        </ul>
    </div>
</template>

<script>
    export default {
        props: ['jsonData'],
        methods: {
            transformURI(uri) {
                return this.$globalMethods.transformURI(uri)
            }
        }
    }
</script>