<template>
  <nav class="bg-white shadow-lg">
    <div class="container mx-auto px-4">
      <div class="flex justify-between h-16">
        <div class="flex items-center">
          <router-link to="/" class="flex-shrink-0 flex items-center">
            <h1 class="text-2xl font-bold text-transparent bg-clip-text bg-gradient-to-r from-violet-400 to-rose-600">Task Manager</h1>
          </router-link>

          <div class="hidden md:ml-6 md:flex md:space-x-4">
            <router-link
              to="/"
              class="px-3 py-2 rounded-md text-sm font-medium transition-colors"
              :class="[$route.name === 'Dashboard' ? 'bg-emerald-100 text-emerald-700' : 'text-gray-700 hover:bg-gray-100']"
            >
              Dashboard
            </router-link>
            <router-link
              to="/tasks"
              class="px-3 py-2 rounded-md text-sm font-medium transition-colors"
              :class="[$route.name === 'Tasks' ? 'bg-emerald-100 text-emerald-700' : 'text-gray-700 hover:bg-gray-100']"
            >
              Tasks
            </router-link>

            <router-link
              to="/notifications"
              class="px-3 py-2 rounded-md text-sm font-medium transition-colors relative"
              :class="[$route.name === 'Notifications' ? 'bg-emerald-100 text-emerald-700' : 'text-gray-700 hover:bg-gray-100']"
            >
              Notifications
              <span
                v-if="notificationStore.unreadCount > 0"
                class="notification-badge absolute -top-1 -right-1 bg-red-500 text-white rounded-full text-xs font-bold min-w-[18px] h-[18px] flex items-center justify-center"
              >
                {{ notificationStore.unreadCount > 99 ? '99+' : notificationStore.unreadCount }}
              </span>
            </router-link>
          </div>
        </div>

        <div class="flex items-center space-x-4">
          <div class="flex items-center space-x-2">
            <span class="text-sm text-gray-700">Welcome,</span>
            <span class="text-sm font-medium text-gray-900">{{ authStore.user?.username }}</span>
          </div>

          <button @click="handleLogout" class="bg-gray-100 hover:bg-gray-200 text-gray-800 px-3 py-2 rounded-md text-sm font-medium transition-colors" data-testid="logout-button">Logout</button>
        </div>
      </div>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/authStore';
import { useRouter } from 'vue-router';
import { useNotificationStore } from '@/stores/notificationStore';

const authStore = useAuthStore();
const notificationStore = useNotificationStore();
const router = useRouter();

const handleLogout = (): void => {
  authStore.logout();
  router.push('/login');
};
</script>
