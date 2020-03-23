<template>
    <div>
        <h1>{{ jsonData.name }}</h1>

        <ul>
            <li v-for="entry in jsonData.data">
                <v-link :href="'/docu/' + jsonData.name + '/' + entry">{{ entry }}</v-link>
            </li>
        </ul>
    </div>
</template>

<script>
    export default {
        props: ['jsonData']
    }
</script>