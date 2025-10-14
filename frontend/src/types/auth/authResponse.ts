import type { User } from '@/types/user/user';

export interface AuthResponse {
  token: string;
  user: User;
}
