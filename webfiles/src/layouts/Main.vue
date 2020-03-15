<template>
    <div class="container">
        <ul>
            <li>
                <v-link href="/home">Home</v-link>
                <v-link href="/docu/hooks">Hooks</v-link>
                <v-link href="/docu/createconvars">ConVars</v-link>
                <v-link href="/docu/datastructures">Functions</v-link>
            </li>
        </ul>

        <slot></slot>
    </div>
</template>

<style scoped>
    .container {
        max-width: 600px;
        margin: 0 auto;
        padding: 15px 30px;
        background: #f9f7f5;
    }
</style>