export interface Notification {
  id: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
  taskItemId: string;
  notificationType: string;
}

export interface NotificationCreate {
  title: string;
  message: string;
  taskItemId: string;
  notificationType: string;
}

export interface NotificationUpdate {
  isRead?: boolean;
}

export interface UnreadCountResponse {
  count: number;
}

export interface Toast {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  timestamp: Date;
  taskItemId?: string;
  duration?: number;
}
