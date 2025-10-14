import type { ApiError } from '@/types/api/apiError';
import { apiService } from './apiService';
import type { User, CreateRandomUsersRequest, NewUser } from '@/types';

class UserService {
  async createRandomUsers(request: CreateRandomUsersRequest): Promise<{ message: string; defaultPassword: string; users: User[] }> {
    try {
      return await apiService.post('/Users/createRandom', request);
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to create random users');
    }
  }

  async registerUser(request: NewUser): Promise<{ message: string }> {
    try {
      return await apiService.post('/Users/register', request);
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to register new user');
    }
  }

  async getAllUsers(): Promise<User[]> {
    try {
      return await apiService.get<User[]>('/Users');
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to get all users');
    }
  }
}

export const userService = new UserService();
