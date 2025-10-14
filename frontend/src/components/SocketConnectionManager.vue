<template>
  <div>
    <div></div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import { useNotificationStore } from '@/stores/notificationStore';
import { signalRService } from '@/services/signalRService';

const authStore = useAuthStore();
const notificationStore = useNotificationStore();
const connectionStatus = ref('disconnected');
const lastError = ref('');

const startConnection = async (): Promise<void> => {
  if (!authStore.isAuthenticated || !authStore.token) {
    connectionStatus.value = 'waiting-auth';
    return;
  }

  try {
    connectionStatus.value = 'connecting';
    await signalRService.startConnection();
    connectionStatus.value = 'connected';
    await notificationStore.loadNotifications();
  } catch (error) {
    connectionStatus.value = 'error';
    lastError.value = (error as Error).message;

    if (lastError.value) console.log('[SignalR] Erro na conexão:', lastError.value);
    setTimeout(() => {
      if (authStore.isAuthenticated) {
        startConnection();
      }
    }, 5000);
  }
};

const stopConnection = async (): Promise<void> => {
  try {
    await signalRService.stopConnection();
    connectionStatus.value = 'disconnected';
  } catch (error) {
    console.error('Erro ao parar SignalR:', error);
  }
};

watch(
  () => authStore.isAuthenticated,
  (isAuthenticated) => {
    if (isAuthenticated) startConnection();
    else stopConnection();
  }
);

watch(
  () => authStore.token,
  (newToken) => {
    if (newToken && authStore.isAuthenticated) {
      stopConnection().then(() => {
        setTimeout(() => startConnection(), 1000);
      });
    }
  }
);

onMounted(() => {
  if (authStore.isAuthenticated) startConnection();
});

onUnmounted(() => {
  stopConnection();
});
</script>
