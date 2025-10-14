import { describe, it, expect, beforeEach, vi } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useTaskStore } from '@/stores/taskStore';
import { taskService } from '@/services/taskService';
import type { TaskItem, CreateTaskItemRequest, UpdateTaskItemRequest } from '@/types';
import { TaskItemStatus } from '@/types';

// Mock the taskService
vi.mock('@/services/taskService');

describe('Task Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  const mockTasks: TaskItem[] = [
    {
      publicId: '1',
      title: 'Task 1',
      description: 'Description 1',
      dueDate: '2024-01-01T00:00:00Z',
      status: TaskItemStatus.Pending,
      order: 0,
      assignedToId: 'user-1',
      assignedToUsername: 'user1',
      createdById: 'user-1',
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    },
    {
      publicId: '2',
      title: 'Task 2',
      description: 'Description 2',
      dueDate: '2024-01-02T00:00:00Z',
      status: TaskItemStatus.InProgress,
      order: 1,
      assignedToId: 'user-2',
      assignedToUsername: 'user2',
      createdById: 'user-1',
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    },
    {
      publicId: '3',
      title: 'Task 3',
      description: 'Description 3',
      dueDate: '2024-01-03T00:00:00Z',
      status: TaskItemStatus.Completed,
      order: 2,
      assignedToId: 'user-1',
      assignedToUsername: 'user1',
      createdById: 'user-2',
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    },
  ];

  describe('fetchTasks', () => {
    it('should fetch and store tasks successfully', async () => {
      vi.mocked(taskService.getTasks).mockResolvedValue(mockTasks);

      const store = useTaskStore();
      await store.fetchTasks();

      expect(store.tasks).toEqual(mockTasks);
      expect(store.isLoading).toBe(false);
      expect(store.error).toBeNull();
    });

    it('should handle fetch errors', async () => {
      const error = new Error('Network error');
      vi.mocked(taskService.getTasks).mockRejectedValue(error);

      const store = useTaskStore();
      await store.fetchTasks();

      expect(store.tasks).toEqual([]);
      expect(store.error).toBe('Network error'); // Store uses actual error message
      expect(store.isLoading).toBe(false);
    });
  });

  describe('computed properties', () => {
    beforeEach(async () => {
      vi.mocked(taskService.getTasks).mockResolvedValue(mockTasks);
      const store = useTaskStore();
      await store.fetchTasks();
    });

    it('should return pending tasks', () => {
      const store = useTaskStore();
      expect(store.pendingTasks).toHaveLength(1);
      expect(store.pendingTasks[0].status).toBe(TaskItemStatus.Pending);
    });

    it('should return in-progress tasks', () => {
      const store = useTaskStore();
      expect(store.inProgressTasks).toHaveLength(1);
      expect(store.inProgressTasks[0].status).toBe(TaskItemStatus.InProgress);
    });

    it('should return completed tasks', () => {
      const store = useTaskStore();
      expect(store.completedTasks).toHaveLength(1);
      expect(store.completedTasks[0].status).toBe(TaskItemStatus.Completed);
    });
  });

  describe('createTask', () => {
    it('should create a new task successfully', async () => {
      const newTaskData: CreateTaskItemRequest = {
        title: 'New Task',
        description: 'New Description',
        dueDate: '2024-01-04T00:00:00Z',
        assignedToId: 'user-1',
      };

      const createdTask: TaskItem = {
        ...newTaskData,
        publicId: '4',
        status: TaskItemStatus.Pending,
        order: 3,
        assignedToUsername: 'user1',
        createdById: 'user-1',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      vi.mocked(taskService.createTask).mockResolvedValue(createdTask);
      vi.mocked(taskService.getTasks).mockResolvedValue([...mockTasks, createdTask]);

      const store = useTaskStore();
      await store.fetchTasks(); // Load initial tasks

      const result = await store.createTask(newTaskData);

      expect(result).toEqual(createdTask);
      expect(store.tasks).toContainEqual(createdTask);
      expect(taskService.createTask).toHaveBeenCalledWith(newTaskData);
    });

    it('should handle create task errors', async () => {
      const newTaskData: CreateTaskItemRequest = {
        title: 'New Task',
        description: 'New Description',
        dueDate: '2024-01-04T00:00:00Z',
      };

      const error = new Error('Create failed');
      vi.mocked(taskService.createTask).mockRejectedValue(error);

      const store = useTaskStore();

      await expect(store.createTask(newTaskData)).rejects.toThrow();
      expect(store.error).toBe('Create failed'); // Store uses actual error message
    });
  });

  describe('updateTask', () => {
    it('should update an existing task successfully', async () => {
      const updateData: UpdateTaskItemRequest = {
        title: 'Updated Task',
        status: TaskItemStatus.InProgress,
      };

      const updatedTask: TaskItem = {
        ...mockTasks[0],
        ...updateData,
      };

      vi.mocked(taskService.updateTask).mockResolvedValue(updatedTask);
      vi.mocked(taskService.getTasks).mockResolvedValue(mockTasks);

      const store = useTaskStore();
      await store.fetchTasks();

      const result = await store.updateTask('1', updateData);

      expect(result).toEqual(updatedTask);
      expect(store.tasks.find((t) => t.publicId === '1')?.title).toBe('Updated Task');
      expect(taskService.updateTask).toHaveBeenCalledWith('1', updateData);
    });

    it('should handle update task errors', async () => {
      const updateData: UpdateTaskItemRequest = {
        title: 'Updated Task',
      };

      const error = new Error('Update failed');
      vi.mocked(taskService.updateTask).mockRejectedValue(error);

      const store = useTaskStore();

      await expect(store.updateTask('1', updateData)).rejects.toThrow();
      expect(store.error).toBe('Update failed'); // Store uses actual error message
    });
  });

  describe('deleteTask', () => {
    it('should delete a task successfully', async () => {
      vi.mocked(taskService.deleteTask).mockResolvedValue();
      vi.mocked(taskService.getTasks).mockResolvedValue(mockTasks);

      const store = useTaskStore();
      await store.fetchTasks();

      await store.deleteTask('1');

      expect(store.tasks.find((t) => t.publicId === '1')).toBeUndefined();
      expect(taskService.deleteTask).toHaveBeenCalledWith('1');
    });

    it('should handle delete task errors', async () => {
      const error = new Error('Delete failed');
      vi.mocked(taskService.deleteTask).mockRejectedValue(error);

      const store = useTaskStore();

      await expect(store.deleteTask('1')).rejects.toThrow();
      expect(store.error).toBe('Delete failed'); // Store uses actual error message
    });
  });

  describe('updateTaskOrder', () => {
    it('should update task order and refresh tasks', async () => {
      const updates = [
        { taskId: '1', status: TaskItemStatus.InProgress, order: 0 },
        { taskId: '2', status: TaskItemStatus.InProgress, order: 1 },
      ];

      vi.mocked(taskService.updateTaskOrder).mockResolvedValue();
      vi.mocked(taskService.getTasks).mockResolvedValue(mockTasks);

      const store = useTaskStore();
      // First populate the store with tasks so updateTaskOrder can find them
      await store.fetchTasks();

      await store.updateTaskOrder(updates);

      expect(taskService.updateTaskOrder).toHaveBeenCalledWith(updates);
      // updateTaskOrder doesn't call getTasks, it updates locally
      // So we don't expect getTasks to be called again after the initial fetch
      expect(taskService.getTasks).toHaveBeenCalledTimes(1); // Only from fetchTasks

      // Check that the tasks were updated locally
      const task1 = store.tasks.find((t) => t.publicId === '1');
      expect(task1?.status).toBe(TaskItemStatus.InProgress);
      expect(task1?.order).toBe(0);
    });
  });
});
