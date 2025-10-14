import type { AuthResponse, LoginRequest } from '@/types';
import { apiService } from './apiService';
import type { ApiError } from '@/types/api/apiError';

class AuthService {
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    try {
      return await apiService.post<AuthResponse>('/Auth/login', credentials, { skipInterceptor: true });
    } catch (error) {
      throw new Error((error as ApiError).message || 'Authentication failed');
    }
  }

  async validateUser(credentials: LoginRequest): Promise<boolean> {
    try {
      return await apiService.post<boolean>('/Auth/validate', credentials);
    } catch (error) {
      throw new Error((error as ApiError).message || 'Validation failed');
    }
  }
}

export const authService = new AuthService();
