import { describe, it, expect, beforeEach, vi } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useUserStore } from '@/stores/userStore';
import { userService } from '@/services/userService';
import type { User, CreateRandomUsersRequest } from '@/types';

// Mock the userService
vi.mock('@/services/userService');

describe('User Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  const mockUsers: User[] = [
    {
      publicId: 'user-1',
      username: 'user1',
      email: 'user1@example.com',
      permissions: ['CanCreateTask'],
    },
    {
      publicId: 'user-2',
      username: 'user2',
      email: 'user2@example.com',
      permissions: ['CanCreateTask', 'CanUpdateTask'],
    },
  ];

  describe('fetchAllUsers', () => {
    it('should fetch and store users successfully', async () => {
      vi.mocked(userService.getAllUsers).mockResolvedValue(mockUsers);

      const store = useUserStore();
      await store.fetchAllUsers();

      expect(store.users).toEqual(mockUsers);
      expect(store.isLoading).toBe(false);
      expect(store.error).toBeNull();
    });

    it('should handle fetch errors', async () => {
      const error = new Error('Network error');
      vi.mocked(userService.getAllUsers).mockRejectedValue(error);

      const store = useUserStore();
      await store.fetchAllUsers();

      expect(store.users).toEqual([]);
      expect(store.error).toBe('Failed to fetch users');
      expect(store.isLoading).toBe(false);
    });
  });

  describe('populateUsers', () => {
    it('should create random users successfully', async () => {
      const request: CreateRandomUsersRequest = {
        amount: 5,
        userNameMask: 'testuser_{{random}}',
      };

      vi.mocked(userService.createRandomUsers).mockResolvedValue();

      const store = useUserStore();
      await store.populateUsers(request);

      expect(userService.createRandomUsers).toHaveBeenCalledWith(request);
      expect(store.error).toBeNull();
    });

    it('should handle populate users errors', async () => {
      const request: CreateRandomUsersRequest = {
        amount: 5,
        userNameMask: 'testuser_{{random}}',
      };

      const error = new Error('Create failed');
      vi.mocked(userService.createRandomUsers).mockRejectedValue(error);

      const store = useUserStore();

      await expect(store.populateUsers(request)).rejects.toThrow();
      expect(store.error).toBe('Failed to populate database with users');
    });
  });
});
