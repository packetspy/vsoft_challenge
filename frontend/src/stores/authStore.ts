import { ref, computed, readonly } from 'vue';
import { defineStore } from 'pinia';
import type { User, LoginRequest } from '@/types';
import { authService } from '@/services/authService';
import { userService } from '@/services/userService';

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null);
  const token = ref<string | null>(null);
  const isLoading = ref(false);

  const isAuthenticated = computed(() => !!token.value && !!user.value);

  const hasPermission = (permission: string): boolean => {
    return user.value?.permissions.includes(permission) ?? false;
  };

  const initAuth = (): void => {
    const savedToken = localStorage.getItem('auth_token');
    const savedUser = localStorage.getItem('user');

    if (savedToken && savedUser) {
      token.value = savedToken;
      user.value = JSON.parse(savedUser);
    }
  };

  const login = async (credentials: LoginRequest): Promise<void> => {
    try {
      isLoading.value = true;
      const authResponse = await authService.login(credentials);

      token.value = authResponse.token;
      user.value = authResponse.user;

      localStorage.setItem('auth_token', authResponse.token);
      localStorage.setItem('user', JSON.stringify(authResponse.user));
    } finally {
      isLoading.value = false;
    }
  };

  const logout = (): void => {
    token.value = null;
    user.value = null;
    localStorage.removeItem('auth_token');
    localStorage.removeItem('user');
  };

  initAuth();

  return {
    user: readonly(user),
    token: readonly(token),
    isLoading: readonly(isLoading),
    isAuthenticated,
    hasPermission,
    login,
    logout,
  };
});
