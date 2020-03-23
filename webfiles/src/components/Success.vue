<template>
    <div class="flex bg-success-primary mb-4 rounded">
        <div class="w-16 flex justify-center bg-success-secondary rounded-l">
            <div class="p-4">
                <font-awesome-icon icon="check" size="2x" class="text-on-error-secondary" />
            </div>
        </div>
        <div class="w-auto text-on-success-primary items-center p-4">
            <slot></slot>
        </div>
    </div>
</template>