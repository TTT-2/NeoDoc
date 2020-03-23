<template>
    <div>
        <h1>{{ jsonData.name }}</h1>

        <ul>
            <li v-for="entry in jsonData.data">
                <ul v-if="entry.sections" class="ml-8">
                    <li v-for="(sectionEntry, sectionName) in entry.sections">
                        <h2>{{ sectionName }}</h2>

                        <ul class="ml-8">
                            <li v-for="(dsList, ds) in sectionEntry">
                                <h3>{{ ds }}</h3>

                                <ul class="ml-8">
                                    <li v-for="dsEntry in dsList">
                                        {{ dsEntry }}
                                    </li>
                                </ul>
                            </li>
                        </ul>
                    </li>
                </ul>
                <v-link v-else :href="'/docu/' + encodeURIComponent(jsonData.name) + 's/' + encodeURIComponent(entry)">{{ entry }}</v-link>
            </li>
        </ul>
    </div>
</template>

<script>
    export default {
        props: ['jsonData']
    }
</script>