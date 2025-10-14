<template>
  <div id="app">
    <NavBar v-if="authStore.isAuthenticated" />
    <router-view />

    <NotificationToast />
    <SocketConnectionManager />
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, watch } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import { useNotificationStore } from '@/stores/notificationStore';
import { notificationService } from '@/services/notificationService';
import NavBar from '@/components/NavBar.vue';
import NotificationToast from '@/components/NotificationToast.vue';
import SocketConnectionManager from '@/components/SocketConnectionManager.vue';
const authStore = useAuthStore();
const notificationStore = useNotificationStore();

onMounted(() => {
  if (authStore.isAuthenticated) {
    notificationStore.loadNotifications();
  }
});
</script>
