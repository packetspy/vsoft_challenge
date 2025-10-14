import { HubConnection, HubConnectionBuilder, LogLevel, HttpTransportType } from '@microsoft/signalr';
import { useAuthStore } from '@/stores/authStore';
import { useTaskStore } from '@/stores/taskStore';

class SignalRService {
  private connection: HubConnection | null = null;
  private isConnected = false;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;

  async startConnection(): Promise<void> {
    try {
      const authStore = useAuthStore();
      const token = authStore.token;

      if (!token) throw new Error('No authentication token');

      console.log('Starting SignalR connection...');
      const baseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:7000';
      this.connection = new HubConnectionBuilder()
        .withUrl(`${baseUrl}/notificationHub`, {
          accessTokenFactory: () => token,
          skipNegotiation: true,
          transport: HttpTransportType.WebSockets,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            this.reconnectAttempts = retryContext.previousRetryCount + 1;
            if (this.reconnectAttempts > this.maxReconnectAttempts) return null;
            return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
          },
        })
        .configureLogging(LogLevel.Information)
        .build();

      this.setupEventHandlers();

      await this.connection.start();
      this.isConnected = true;
      this.reconnectAttempts = 0;

      console.log('SignalR connection established');
    } catch (error) {
      console.error('SignalR connection failed:', error);
      this.isConnected = false;
      throw error;
    }
  }

  private setupEventHandlers(): void {
    if (!this.connection) return;

    this.connection.on('ReceiveNotification', async (notification) => {
      const { useNotificationStore } = await import('@/stores/notificationStore');
      const notificationStore = useNotificationStore();
      notificationStore.addNotification(notification);
      const taskStore = useTaskStore();
      await taskStore.fetchTasks();

      // Global event dispatch
      const event = new CustomEvent('new-notification', {
        detail: notification,
      });
      window.dispatchEvent(event);
      console.log('[SignalR] Evento disparado com sucesso');
    });

    this.connection.on('NotificationMarkedAsRead', (notificationId) => {
      console.log('[SignalR] Notificação marcada como lida:', notificationId);
    });

    this.connection.onreconnecting((error) => {
      console.log('[SignalR] Reconectando...', error);
      this.isConnected = false;
    });

    this.connection.onreconnected((connectionId) => {
      console.log('[SignalR] Reconectado:', connectionId);
      this.isConnected = true;
      this.reconnectAttempts = 0;
    });

    this.connection.onclose((error) => {
      console.log('[SignalR] Conexão fechada', error);
      this.isConnected = false;
    });
  }

  async markAsRead(notificationId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      await this.connection.invoke('MarkAsRead', notificationId);
    }
  }

  async stopConnection(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.isConnected = false;
      this.connection = null;
      console.log('[SignalR] Conexão encerrada');
    }
  }

  getConnectionState(): boolean {
    return this.isConnected;
  }

  getReconnectAttempts(): number {
    return this.reconnectAttempts;
  }
}

export const signalRService = new SignalRService();
