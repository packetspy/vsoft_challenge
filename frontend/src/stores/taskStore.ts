import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { taskService } from '@/services/taskService';
import type { TaskItem, CreateTaskItemRequest, UpdateTaskItemRequest } from '@/types';
import { TaskItemStatus } from '@/types';

export const useTaskStore = defineStore('tasks', () => {
  const tasks = ref<TaskItem[]>([]);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  const pendingTasks = computed(() => {
    return tasks.value.filter((task) => task.status === TaskItemStatus.Pending).sort((a, b) => a.order - b.order);
  });

  const inProgressTasks = computed(() => {
    return tasks.value.filter((task) => task.status === TaskItemStatus.InProgress).sort((a, b) => a.order - b.order);
  });

  const completedTasks = computed(() => {
    return tasks.value.filter((task) => task.status === TaskItemStatus.Completed).sort((a, b) => a.order - b.order);
  });

  const fetchTasks = async (): Promise<void> => {
    try {
      isLoading.value = true;
      error.value = null;
      tasks.value = await taskService.getTasks();
    } catch (err) {
      error.value = (err as Error).message;
      console.error('Error fetching tasks:', err);
    } finally {
      isLoading.value = false;
    }
  };

  const createTask = async (taskData: CreateTaskItemRequest): Promise<TaskItem> => {
    try {
      const newTask = await taskService.createTask(taskData);
      tasks.value = [...tasks.value, newTask];
      return newTask;
    } catch (err) {
      error.value = (err as Error).message;
      console.error('Error creating task:', err);
      throw err;
    }
  };

  const updateTask = async (taskId: string, updates: UpdateTaskItemRequest): Promise<TaskItem> => {
    try {
      const updatedTask = await taskService.updateTask(taskId, updates);

      const taskIndex = tasks.value.findIndex((task) => task.publicId === taskId);

      if (taskIndex !== -1) {
        const newTasks = [...tasks.value];
        newTasks[taskIndex] = { ...newTasks[taskIndex], ...updatedTask };
        tasks.value = newTasks; // Fix: Actually assign the updated tasks back
      } else {
        console.warn('Task not found in store, fetching all tasks');
      }

      return updatedTask;
    } catch (err) {
      error.value = (err as Error).message;
      console.error('Error updating task:', err);
      throw err;
    }
  };

  const deleteTask = async (taskId: string): Promise<void> => {
    try {
      await taskService.deleteTask(taskId);

      tasks.value = tasks.value.filter((task) => task.publicId !== taskId);
    } catch (err) {
      error.value = (err as Error).message;
      console.error('Error deleting task:', err);
      throw err;
    }
  };

  const updateTaskOrder = async (updates: { taskId: string; status: TaskItemStatus; order: number }[]): Promise<void> => {
    try {
      await taskService.updateTaskOrder(updates);

      const newTasks = [...tasks.value];

      updates.forEach((update) => {
        const taskIndex = newTasks.findIndex((t) => t.publicId === update.taskId);
        if (taskIndex !== -1) {
          newTasks[taskIndex] = {
            ...newTasks[taskIndex],
            status: update.status,
            order: update.order,
          };
        }
      });

      tasks.value = newTasks;
    } catch (err) {
      error.value = (err as Error).message;
      console.error('Error updating task order:', err);
      throw err;
    }
  };

  const clearError = (): void => {
    error.value = null;
  };

  return {
    tasks: tasks,
    isLoading,
    error,
    pendingTasks,
    inProgressTasks,
    completedTasks,
    fetchTasks,
    createTask,
    updateTask,
    deleteTask,
    updateTaskOrder,
    clearError,
  };
});
