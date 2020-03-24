<template>
    <code class="prettyprint lang-lua mb-4 rounded p-2 bg-code text-on-code break-all">
        <slot></slot>
    </code>
</template>

<script>
    // code prettify 
    import PrettyPrint from 'code-prettify';

    export default {
        mounted() {
            this.$nextTick(function () {
                PrettyPrint.prettyPrint();
            });
        }
    }
</script>