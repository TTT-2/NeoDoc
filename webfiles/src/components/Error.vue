<template>
    <div class="flex bg-error-primary mb-4 rounded">
        <div class="w-16 flex justify-center bg-error-secondary rounded-l">
            <div class="p-4">
                <font-awesome-icon icon="times" size="2x" class="text-on-error-secondary" />
            </div>
        </div>
        <div class="w-auto text-on-error-primary opacity-75 items-center p-4">
            <slot></slot>
        </div>
    </div>
</template>