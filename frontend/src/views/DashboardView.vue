<template>
  <div class="container mx-auto px-4 py-8">
    <div class="mb-8">
      <h2 class="text-3xl font-bold text-gray-900 mb-2">Dashboard</h2>
      <p class="text-gray-600">Welcome back, {{ authStore.user?.username }}!</p>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
      <CounterCard title="Pending Tasks" :counter="taskStore.pendingTasks.length" color="blue" />
      <CounterCard title="In Progress" :counter="taskStore.inProgressTasks.length" color="yellow" />
      <CounterCard title="Completed" :counter="taskStore.completedTasks.length" color="green" />
    </div>

    <div class="bg-white rounded-lg shadow-md border border-slate-200">
      <div class="px-6 py-4 border-b border-gray-200">
        <h3 class="text-xl font-semibold text-gray-800">Recent Tasks</h3>
      </div>
      <div class="p-6">
        <div v-if="taskStore.isLoading" class="text-center py-8">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-slate-600 mx-auto"></div>
        </div>

        <div v-else-if="recentTasks.length === 0" class="text-center py-8 text-gray-500">
          No tasks found. <router-link to="/tasks" class="text-slate-600 hover:text-slate-500">Create your first task</router-link>
        </div>

        <div v-else class="space-y-4">
          <div v-for="task in recentTasks" :key="task.publicId" class="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors">
            <div class="flex-1">
              <h3 class="font-medium text-gray-900">{{ task.title }}</h3>
              <p class="text-sm text-gray-600 mt-1">{{ task.description }}</p>
              <div class="flex items-center space-x-4 mt-2 text-xs text-gray-500">
                <span>Due: {{ formatDate(task.dueDate) }}</span>
                <span :class="statusClass(task.status.toString())">{{ task.status }}</span>
              </div>
            </div>
            <router-link :to="'/tasks'" class="text-slate-600 hover:text-slate-500 text-sm font-medium"> View </router-link>
          </div>
        </div>

        <div class="mt-6 text-center">
          <router-link
            to="/tasks"
            class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-slate-600 hover:bg-slate-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-slate-500"
          >
            View All Tasks
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import { useTaskStore } from '@/stores/taskStore';
import type { TaskItem } from '@/types';
import CounterCard from '@/components/CounterCard.vue';

const authStore = useAuthStore();
const taskStore = useTaskStore();

const recentTasks = computed(() => {
  return [...taskStore.tasks].sort((a: TaskItem, b: TaskItem) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()).slice(0, 5);
});

const statusClass = (status: string): string => {
  const classes = {
    Pending: 'text-yellow-600 bg-yellow-100 px-2 py-1 rounded',
    InProgress: 'text-blue-600 bg-blue-100 px-2 py-1 rounded',
    Completed: 'text-green-600 bg-green-100 px-2 py-1 rounded',
  };
  return classes[status as keyof typeof classes] || 'text-gray-600 bg-gray-100 px-2 py-1 rounded';
};

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
};

onMounted(() => {
  taskStore.fetchTasks();
});
</script>
