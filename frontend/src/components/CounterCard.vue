<template>
  <div class="bg-white rounded-lg shadow-md border border-slate-100 p-6">
    <div class="flex items-center">
      <div class="p-3 rounded-lg" :class="`bg-${color}-100`">
        <svg class="w-6 h-6" :class="`text-${color}-600`" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
      </div>
      <div class="ml-4">
        <p class="text-sm font-medium text-gray-600">{{ title }}</p>
        <p class="text-2xl font-semibold text-gray-900">{{ counter }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { watchEffect } from 'vue';

interface Props {
  title: string;
  counter: number;
  color: string;
}

const title = ref('');
const counter = ref(0);
const color = ref('');
const props = defineProps<Props>();

watchEffect(() => {
  title.value = props.title;
  counter.value = props.counter;
  color.value = props.color;
});
</script>
