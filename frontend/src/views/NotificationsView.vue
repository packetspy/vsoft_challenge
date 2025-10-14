<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <div class="flex justify-between items-center mb-8">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Notifications</h1>
          <p class="text-gray-600 mt-2">Manage your notifications</p>
        </div>

        <div class="flex items-center gap-4">
          <div class="text-sm text-gray-500">{{ unreadCount }} unread</div>
          <button v-if="unreadCount > 0" @click="markAllAsRead" :disabled="isLoading" class="bg-emerald-600 text-white px-4 py-2 rounded-md hover:bg-emerald-700 disabled:opacity-50">
            Mark all as read
          </button>
        </div>
      </div>

      <div v-if="isLoading" class="text-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-emerald-600 mx-auto"></div>
        <p class="text-gray-600 mt-4">Loading notifications...</p>
      </div>

      <div v-else-if="notifications.length === 0" class="text-center py-12">
        <div class="text-gray-400 text-6xl mb-4">🔔</div>
        <h3 class="text-xl font-semibold text-gray-600 mb-2">No notifications</h3>
        <p class="text-gray-500">You currently have no notifications.</p>
      </div>

      <div v-else class="bg-white rounded-lg shadow divide-y divide-gray-200">
        <div
          v-for="notification in notifications"
          :key="notification.id"
          :class="['p-6 hover:bg-gray-50 transition-colors cursor-pointer', { 'bg-blue-50': !notification.isRead }]"
          @click="handleNotificationClick(notification)"
        >
          <div class="flex justify-between items-start">
            <div class="flex-1">
              <div class="flex items-center gap-3 mb-2">
                <h3 class="font-semibold text-gray-900">{{ notification.title }}</h3>
                <span v-if="!notification.isRead" class="inline-block w-2 h-2 bg-blue-600 rounded-full"></span>
              </div>
              <p class="text-gray-600 mb-2">{{ notification.message }}</p>
              <div class="flex items-center gap-4 text-sm text-gray-500">
                <span>{{ formatTime(notification.createdAt) }}</span>
                <span v-if="notification.taskItemId" class="flex items-center gap-1">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
                    />
                  </svg>
                  See Task
                </span>
              </div>
            </div>
            <button v-if="!notification.isRead" @click.stop="markAsRead(notification.id)" class="ml-4 text-blue-600 hover:text-blue-800 p-2" title="Marcar como lida">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import { useNotificationStore } from '@/stores/notificationStore';
import { storeToRefs } from 'pinia';

const router = useRouter();
const notificationStore = useNotificationStore();

const { notifications, unreadCount, isLoading } = storeToRefs(notificationStore);

const handleNotificationClick = (notification: any): void => {
  if (!notification.isRead) {
    notificationStore.markAsRead(notification.id);
  }

  if (notification.taskItemId) {
    router.push(`/tasks/${notification.taskItemId}`);
  }
};

const markAsRead = async (notificationId: string): Promise<void> => {
  await notificationStore.markAsRead(notificationId);
};

const markAllAsRead = async (): Promise<void> => {
  await notificationStore.markAllAsRead();
};

const formatTime = (dateString: string): string => {
  const date = new Date(dateString);
  const now = new Date();
  const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));

  if (diffInMinutes < 1) return 'Now';
  if (diffInMinutes < 60) return `${diffInMinutes} minutes ago`;
  if (diffInMinutes < 1440) return `${Math.floor(diffInMinutes / 60)} hours ago`;

  return date.toLocaleDateString('en-US');
};

onMounted(() => {
  notificationStore.loadNotifications();
});
</script>
