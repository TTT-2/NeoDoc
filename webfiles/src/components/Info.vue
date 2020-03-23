<template>
    <div class="flex bg-info-primary mb-4 rounded">
        <div class="w-16 flex justify-center bg-info-secondary rounded-l">
            <div class="p-4">
                <font-awesome-icon icon="info" size="2x" class="text-on-error-secondary" />
            </div>
        </div>
        <div class="w-auto text-on-info-primary items-center p-4">
            <slot></slot>
        </div>
    </div>
</template>