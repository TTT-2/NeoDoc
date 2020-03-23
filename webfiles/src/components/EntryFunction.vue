<template>
    <div>
        <div>
            <realm-param :realm="jsonData.data.realm" class="inline realm px-2 mr-2 client" />
            <span class="inline">{{ jsonData.name }}</span>
        </div>

        <div v-if="jsonData.data.param">
            <h2>Params:</h2>

            <ul>
                <li v-for="entry in jsonData.data.param">
                    <docu-param :name="entry.name" :type="entry.type" :description="entry.description" />
                </li>
            </ul>
        </div>

        <div v-if="jsonData.data.return">
            <h2>Returns:</h2>

            <ul>
                <li v-for="entry in jsonData.data.return">
                    <docu-param :name="entry.name" :type="entry.type" :description="entry.description" />
                </li>
            </ul>
        </div>

        <div v-if="jsonData.data.desc && jsonData.data.desc[0] != ''">
            <h2>Description:</h2>

            {{ jsonData.data.desc[0] }}
        </div>
    </div>
</template>

<script>
    export default {
        props: ['jsonData']
    }
</script>