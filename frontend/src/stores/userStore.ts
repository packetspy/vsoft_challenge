import { ref, readonly } from 'vue';
import { defineStore } from 'pinia';
import type { CreateRandomUsersRequest, NewUser, User } from '@/types';
import { userService } from '@/services/userService';

export const useUserStore = defineStore('users', () => {
  const users = ref<User[]>([]);
  const selectedUser = ref<User>();
  const isLoading = ref(false);
  const error = ref<string | null>(null);
  const success = ref<string | null>(null);

  const fetchAllUsers = async (): Promise<void> => {
    try {
      isLoading.value = true;
      error.value = null;
      users.value = await userService.getAllUsers();
    } catch (err) {
      error.value = 'Failed to fetch users';
      console.error('Error fetching users:', err);
    } finally {
      isLoading.value = false;
    }
  };

  const populateUsers = async (taskData: CreateRandomUsersRequest): Promise<void> => {
    try {
      error.value = null;
      await userService.createRandomUsers(taskData);
    } catch (err) {
      error.value = 'Failed to populate database with users';
      throw err;
    }
  };

  const registerNewUser = async (taskData: NewUser): Promise<void> => {
    try {
      error.value = null;
      await userService.registerUser(taskData);
      success.value = 'User registered successfully';
    } catch (err) {
      error.value = 'Failed to register new user';
      throw err;
    }
  };

  return {
    users: readonly(users),
    selectedUser: readonly(selectedUser),
    isLoading: readonly(isLoading),
    error: readonly(error),
    registerNewUser,
    fetchAllUsers,
    populateUsers,
  };
});
