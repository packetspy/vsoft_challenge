import type { ApiError } from '@/types/api/apiError';
import { apiService } from './apiService';
import type { Notification, NotificationCreate } from '@/types/notification/notification';

class NotificationService {
  async getNotifications(): Promise<Notification[]> {
    try {
      return await apiService.get<Notification[]>('/Notifications');
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to get notifications');
    }
  }

  async getNotification(id: string): Promise<Notification> {
    try {
      return await apiService.get<Notification>(`/Notifications/${id}`);
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to get notification');
    }
  }

  async markAsRead(notificationId: string): Promise<void> {
    try {
      await apiService.put(`/Notifications/${notificationId}/read`, {});
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to mark notification as read');
    }
  }

  async markAllAsRead(): Promise<void> {
    try {
      await apiService.put('/Notifications/mark-all-read', {});
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to mark all notifications as read');
    }
  }

  async createNotification(notification: NotificationCreate): Promise<Notification> {
    try {
      return await apiService.post<Notification>('/Notifications', notification);
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to create notification');
    }
  }

  async deleteNotification(id: string): Promise<void> {
    try {
      await apiService.delete(`/Notifications/${id}`);
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to delete notification');
    }
  }

  async getUnreadCount(): Promise<number> {
    try {
      const response = await apiService.get<{ count: number }>('/Notifications/unread-count');
      return response.count;
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to get unread notifications count');
    }
  }
}

export const notificationService = new NotificationService();
