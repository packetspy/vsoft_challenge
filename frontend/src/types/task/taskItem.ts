import type { TaskItemStatus } from '@/types/task/taskItemStatus';

export interface TaskItem {
  publicId: string;
  title: string;
  description: string;
  dueDate: string;
  status: TaskItemStatus;
  order: number;
  assignedToId: string;
  assignedToUsername?: string;
  createdById: string;
  createdAt: string;
  updatedAt: string;
}
