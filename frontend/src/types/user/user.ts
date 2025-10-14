export interface User {
  publicId: string;
  username: string;
  email: string;
  permissions: string[];
}

export interface NewUser {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}
