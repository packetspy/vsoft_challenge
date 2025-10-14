import axios, { type AxiosInstance, type AxiosResponse, type AxiosRequestConfig } from 'axios';

declare module 'axios' {
  interface AxiosRequestConfig {
    skipInterceptor?: boolean;
  }
}

class ApiService {
  private api: AxiosInstance;

  constructor() {
    const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:7000';
    const suffixURL = import.meta.env.VITE_SUFFIX_URL ?? 'api';

    this.api = axios.create({
      baseURL: `${baseURL}/${suffixURL}`,
      timeout: 10000,
    });

    this.setupInterceptors();
  }

  private setupInterceptors(): void {
    this.api.interceptors.request.use(
      (config) => {
        if (config.skipInterceptor) return config;
        const token = localStorage.getItem('auth_token');
        if (token) config.headers.Authorization = `Bearer ${token}`;

        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    this.api.interceptors.response.use(
      (response: AxiosResponse) => response,
      (error) => {
        if (error.config?.skipInterceptor) return Promise.reject(error);
        if (error.response?.status === 401) this.handleUnauthorized();
        return Promise.reject(error);
      }
    );
  }

  private handleUnauthorized(): void {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('user');
    window.location.href = '/login';
  }

  async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response: AxiosResponse<T> = await this.api.get(url, config);
    return response.data;
  }

  async post<T>(url: string, data: any, config?: AxiosRequestConfig): Promise<T> {
    const response: AxiosResponse<T> = await this.api.post<T>(url, data, config);
    return response.data;
  }

  async put<T>(url: string, data: any, config?: AxiosRequestConfig): Promise<T> {
    const response: AxiosResponse<T> = await this.api.put<T>(url, data, config);
    return response.data;
  }

  async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response: AxiosResponse<T> = await this.api.delete(url, config);
    return response.data;
  }
}

export const apiService = new ApiService();
