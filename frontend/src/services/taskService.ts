import type { ApiError } from '@/types/api/apiError';
import { apiService } from './apiService';
import type { TaskItem, TaskItemStatus, CreateTaskItemRequest, UpdateTaskItemRequest } from '@/types';

class TaskService {
  async getTasks(): Promise<TaskItem[]> {
    try {
      return await apiService.get<TaskItem[]>('/Tasks');
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to get tasks');
    }
  }

  async getTask(id: string): Promise<TaskItem> {
    try {
      return await apiService.get<TaskItem>(`/Tasks/${id}`);
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to get task');
    }
  }

  async createTask(task: CreateTaskItemRequest): Promise<TaskItem> {
    try {
      return await apiService.post<TaskItem>('/Tasks', task);
    } catch (error) {
      console.log(error);
      throw new Error((error as ApiError).message || 'Failed to create task');
    }
  }

  async updateTask(id: string, task: UpdateTaskItemRequest): Promise<TaskItem> {
    try {
      return await apiService.put<TaskItem>(`/Tasks/${id}`, task);
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to update task');
    }
  }

  async deleteTask(id: string): Promise<void> {
    try {
      await apiService.delete(`/Tasks/${id}`);
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to delete task');
    }
  }

  async updateTaskOrder(updates: { taskId: string; status: TaskItemStatus; order: number }[]): Promise<void> {
    try {
      await apiService.put('/Tasks/reorder', { updates });
    } catch (error) {
      throw new Error((error as ApiError).message || 'Failed to update task order');
    }
  }
}

export const taskService = new TaskService();
