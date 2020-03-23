<template>
    <div class="flex bg-warning-primary mb-4 rounded">
        <div class="w-16 flex justify-center bg-warning-secondary rounded-l">
            <div class="p-4">
                <font-awesome-icon icon="exclamation-triangle" size="2x" />
            </div>
        </div>
        <div class="w-auto text-on-warning-primary items-center p-4">
            <slot></slot>
        </div>
    </div>
</template>