<template>
    <div>
        <ul>
            <li v-for="entry in jsonData.data">
                <v-link :href="'/docu/' + encodeURIComponent(jsonData.name) + 's/' + encodeURIComponent(entry)">{{ entry }}</v-link>
            </li>
        </ul>
    </div>
</template>

<script>
    export default {
        props: ['jsonData']
    }
</script>