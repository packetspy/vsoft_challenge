<template>
  <TransitionGroup name="toast">
    <div
      v-for="toast in activeToasts"
      :key="toast.id"
      :class="['fixed top-4 right-4 z-50 max-w-sm w-full bg-white rounded-lg shadow-lg border-l-4 p-4 mb-2 transform transition-all duration-300', toastBorderClass(toast.type)]"
      @click="handleToastClick(toast)"
    >
      <div class="flex justify-between items-start mb-2">
        <div class="font-semibold text-gray-800 flex items-center gap-2">
          <span class="text-lg">🔔</span>
          {{ toast.title }}
        </div>
        <button @click.stop="removeToast(toast.id)" class="text-gray-400 hover:text-gray-600 text-xl font-bold leading-none">×</button>
      </div>
      <div class="text-gray-600 text-sm mb-1">{{ toast.message }}</div>
      <div class="text-gray-400 text-xs">{{ formatTime(toast.timestamp) }}</div>
    </div>
  </TransitionGroup>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import type { Toast } from '@/types/notification/notification';

const router = useRouter();
const activeToasts = ref<Toast[]>([]);

const toastBorderClass = (type: Toast['type']): string => {
  const borderClasses = {
    info: 'border-l-blue-500',
    success: 'border-l-green-500',
    warning: 'border-l-yellow-500',
    error: 'border-l-red-500',
  };
  return borderClasses[type] || borderClasses.info;
};

const showToast = (notification: any): void => {
  const toast: Toast = {
    id: `toast-${Date.now()}-${Math.random()}`,
    title: notification.title || 'New Notification',
    message: notification.message || 'Blank message',
    type: 'info',
    timestamp: new Date(notification.createdAt || Date.now()),
    taskItemId: notification.taskItemId,
    duration: 5000,
  };

  activeToasts.value.push(toast);

  if (toast.duration) {
    setTimeout(() => {
      removeToast(toast.id);
    }, toast.duration);
  }
};

const removeToast = (toastId: string): void => {
  activeToasts.value = activeToasts.value.filter((toast) => toast.id !== toastId);
};

const handleToastClick = (toast: Toast): void => {
  if (toast.taskItemId) router.push(`/tasks/${toast.taskItemId}`);
  removeToast(toast.id);
};

const formatTime = (date: Date): string => {
  return new Date(date).toLocaleTimeString('pt-BR', {
    hour: '2-digit',
    minute: '2-digit',
  });
};

const handleNewNotification = (event: CustomEvent): void => {
  showToast(event.detail);
};

onMounted(() => {
  window.addEventListener('new-notification', handleNewNotification as EventListener);
});

onUnmounted(() => {
  window.removeEventListener('new-notification', handleNewNotification as EventListener);
});
</script>

<style scoped>
.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateX(100%);
}

.toast-enter-to,
.toast-leave-from {
  opacity: 1;
  transform: translateX(0);
}

.toast-leave-active {
  position: absolute;
}
</style>
