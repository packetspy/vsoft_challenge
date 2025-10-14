import type { TaskItemStatus } from '@/types/task/taskItemStatus';
export interface UpdateTaskItemRequest {
  title?: string;
  description?: string;
  dueDate?: string;
  status?: TaskItemStatus;
  assignedToId?: string;
}
