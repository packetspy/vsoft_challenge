import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useAuthStore } from '@/stores/authStore';
import { authService } from '@/services/authService';
import { userService } from '@/services/userService';
import type { User, LoginRequest } from '@/types';

// Mock the services
vi.mock('@/services/authService');
vi.mock('@/services/userService');

describe('Auth Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();

    // Clear localStorage and reset mocks
    vi.mocked(localStorage.getItem).mockReturnValue(null);
    vi.mocked(localStorage.setItem).mockImplementation(() => {});
    vi.mocked(localStorage.removeItem).mockImplementation(() => {});
    vi.mocked(localStorage.clear).mockImplementation(() => {});
  });

  afterEach(() => {
    localStorage.clear();
  });

  const mockUser: User = {
    publicId: 'user-1',
    username: 'testuser',
    email: 'test@example.com',
    permissions: ['CanCreateTask', 'CanUpdateTask'],
  };

  const mockAuthResponse = {
    token: 'mock-jwt-token',
    user: mockUser,
  };

  describe('initAuth', () => {
    it('should initialize auth from localStorage', () => {
      // Mock localStorage to return the stored values
      vi.mocked(localStorage.getItem).mockImplementation((key: string) => {
        if (key === 'auth_token') return 'stored-token';
        if (key === 'user') return JSON.stringify(mockUser);
        return null;
      });

      const store = useAuthStore();

      expect(store.token).toBe('stored-token');
      expect(store.user).toEqual(mockUser);
      expect(store.isAuthenticated).toBe(true);
    });

    it('should not initialize auth if no data in localStorage', () => {
      const store = useAuthStore();

      expect(store.token).toBeNull();
      expect(store.user).toBeNull();
      expect(store.isAuthenticated).toBe(false);
    });
  });

  describe('login', () => {
    it('should login successfully and store credentials', async () => {
      const credentials: LoginRequest = {
        username: 'testuser',
        password: 'password123',
      };

      vi.mocked(authService.login).mockResolvedValue(mockAuthResponse);

      const store = useAuthStore();
      await store.login(credentials);

      expect(store.token).toBe('mock-jwt-token');
      expect(store.user).toEqual(mockUser);
      expect(store.isAuthenticated).toBe(true);
      expect(localStorage.setItem).toHaveBeenCalledWith('auth_token', 'mock-jwt-token');
      expect(localStorage.setItem).toHaveBeenCalledWith('user', JSON.stringify(mockUser));
    });

    it('should handle login errors', async () => {
      const credentials: LoginRequest = {
        username: 'testuser',
        password: 'wrongpassword',
      };

      const error = new Error('Login failed');
      vi.mocked(authService.login).mockRejectedValue(error);

      const store = useAuthStore();

      await expect(store.login(credentials)).rejects.toThrow();
      expect(store.token).toBeNull();
      expect(store.user).toBeNull();
      expect(store.isAuthenticated).toBe(false);
    });
  });

  describe('logout', () => {
    it('should clear auth data and localStorage', () => {
      // Mock localStorage to return stored values initially
      vi.mocked(localStorage.getItem).mockImplementation((key: string) => {
        if (key === 'auth_token') return 'token';
        if (key === 'user') return JSON.stringify(mockUser);
        return null;
      });

      const store = useAuthStore();
      store.logout();

      expect(store.token).toBeNull();
      expect(store.user).toBeNull();
      expect(store.isAuthenticated).toBe(false);
      expect(localStorage.removeItem).toHaveBeenCalledWith('auth_token');
      expect(localStorage.removeItem).toHaveBeenCalledWith('user');
    });
  });

  describe('hasPermission', () => {
    it('should return true if user has permission', () => {
      // Mock localStorage to return stored values
      vi.mocked(localStorage.getItem).mockImplementation((key: string) => {
        if (key === 'auth_token') return 'token';
        if (key === 'user') return JSON.stringify(mockUser);
        return null;
      });

      const store = useAuthStore();

      expect(store.hasPermission('CanCreateTask')).toBe(true);
      expect(store.hasPermission('CanUpdateTask')).toBe(true);
    });

    it('should return false if user does not have permission', () => {
      localStorage.setItem('auth_token', 'token');
      localStorage.setItem('user', JSON.stringify(mockUser));

      const store = useAuthStore();

      expect(store.hasPermission('CanDeleteTask')).toBe(false);
    });

    it('should return false if user is not authenticated', () => {
      const store = useAuthStore();

      expect(store.hasPermission('CanCreateTask')).toBe(false);
    });
  });
});
