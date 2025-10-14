<template>
  <div class="container mx-auto px-4 py-8">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-3xl font-bold text-gray-900">My Tasks</h1>
      <button
        @click="showCreateModal = true"
        class="bg-emerald-600 text-white px-4 py-2 rounded-md hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-500"
        data-testid="create-task-button"
      >
        Create Task
      </button>
    </div>

    <div v-if="taskStore.isLoading" class="text-center py-8">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-emerald-600 mx-auto"></div>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-3 gap-6">
      <TaskColumn
        title="Pending"
        :tasks="taskStore.pendingTasks"
        :status="TaskItemStatus.Pending"
        @task-updated="handleTaskUpdated"
        @task-deleted="handleTaskDeleted"
        @task-reorder="handleTaskReorder"
        @task-moved="handleTaskMoved"
        data-testid="pending-column"
      />
      <TaskColumn
        title="In Progress"
        :tasks="taskStore.inProgressTasks"
        :status="TaskItemStatus.InProgress"
        @task-updated="handleTaskUpdated"
        @task-deleted="handleTaskDeleted"
        @task-reorder="handleTaskReorder"
        @task-moved="handleTaskMoved"
        data-testid="in-progress-column"
      />

      <TaskColumn
        title="Completed"
        :tasks="taskStore.completedTasks"
        :status="TaskItemStatus.Completed"
        @task-updated="handleTaskUpdated"
        @task-deleted="handleTaskDeleted"
        @task-reorder="handleTaskReorder"
        @task-moved="handleTaskMoved"
        data-testid="completed-column"
      />
    </div>

    <CreateTaskModal v-if="showCreateModal" @close="showCreateModal = false" @task-created="handleTaskCreated" />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useTaskStore } from '@/stores/taskStore';
import TaskColumn from '@/components/TaskColumn.vue';
import CreateTaskModal from '@/components/CreateTaskModal.vue';
import type { UpdateTaskItemRequest } from '@/types';
import { TaskItemStatus } from '@/types';

const taskStore = useTaskStore();
const showCreateModal = ref(false);

onMounted(() => {
  taskStore.fetchTasks();
});

const handleTaskCreated = (): void => {
  showCreateModal.value = false;
};

const handleTaskUpdated = async (taskId: string, updates: UpdateTaskItemRequest): Promise<void> => {
  await taskStore.updateTask(taskId, updates);
};

const handleTaskDeleted = async (taskId: string): Promise<void> => {
  await taskStore.deleteTask(taskId);
};

const handleTaskReorder = async (updates: { taskId: string; status: TaskItemStatus; order: number }[]): Promise<void> => {
  await taskStore.updateTaskOrder(updates);
};

const handleTaskMoved = async (taskId: string, newStatus: TaskItemStatus, newOrder: number): Promise<void> => {
  const updates = [
    {
      taskId,
      status: newStatus,
      order: newOrder,
    },
  ];

  await taskStore.updateTaskOrder(updates);
};
</script>
