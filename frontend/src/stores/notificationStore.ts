import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { notificationService } from '@/services/notificationService';
import type { Notification } from '@/types/notification/notification';

export const useNotificationStore = defineStore('notifications', () => {
  const notifications = ref<Notification[]>([]);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  const unreadCount = computed(() => notifications.value.filter((notification) => !notification.isRead).length);

  const unreadNotifications = computed(() => notifications.value.filter((notification) => !notification.isRead));

  const recentNotifications = computed(() => notifications.value.slice(0, 10));

  const loadNotifications = async (): Promise<void> => {
    try {
      isLoading.value = true;
      error.value = null;
      notifications.value = await notificationService.getNotifications();
    } catch (err) {
      error.value = (err as Error).message || 'Failed to load notifications';
      console.error('Error loading notifications:', err);
    } finally {
      isLoading.value = false;
    }
  };

  const markAsRead = async (notificationId: string): Promise<void> => {
    try {
      await notificationService.markAsRead(notificationId);

      const notification = notifications.value.find((n) => n.id === notificationId);
      if (notification && !notification.isRead) {
        notification.isRead = true;
      }
    } catch (err) {
      console.error('Error marking notification as read:', err);
      throw err;
    }
  };

  const markAllAsRead = async (): Promise<void> => {
    try {
      await notificationService.markAllAsRead();
      notifications.value.forEach((notification) => {
        notification.isRead = true;
      });
    } catch (err) {
      console.error('Error marking all notifications as read:', err);
      throw err;
    }
  };

  const removeNotification = (notificationId: string): void => {
    notifications.value = notifications.value.filter((n) => n.id !== notificationId);
  };

  const clearError = (): void => {
    error.value = null;
  };

  const refreshUnreadCount = async (): Promise<number> => {
    try {
      return await notificationService.getUnreadCount();
    } catch (err) {
      return unreadCount.value;
    }
  };

  const addNotification = (notification: any): void => {
    notifications.value.unshift(notification);
  };

  return {
    notifications,
    isLoading,
    error,
    unreadCount,
    unreadNotifications,
    recentNotifications,
    loadNotifications,
    markAsRead,
    markAllAsRead,
    addNotification,
    removeNotification,
    clearError,
    refreshUnreadCount,
  };
});
