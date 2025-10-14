export interface CreateTaskItemRequest {
  title: string;
  description: string;
  dueDate: string;
  assignedToId?: string;
}
