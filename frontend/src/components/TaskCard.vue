<template>
  <div :data-task-id="task.publicId" class="bg-white rounded-lg shadow-md p-4 cursor-move hover:shadow-lg transition-shadow">
    <div class="flex justify-between items-start mb-2">
      <h4 class="font-semibold text-gray-900" data-testid="task-title">{{ task.title }}</h4>
      <div class="flex space-x-2">
        <button @click.stop="showEditModal = true" class="text-gray-400 hover:text-emerald-600 transition-colors" data-testid="edit-task" title="Edit task">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
            />
          </svg>
        </button>
        <button @click.stop="handleDelete" class="text-gray-400 hover:text-red-600 transition-colors" title="Delete task" data-testid="delete-task">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
            />
          </svg>
        </button>
      </div>
    </div>

    <p class="text-gray-600 text-sm mb-3 line-clamp-2">{{ task.description }}</p>

    <div class="flex justify-between items-center text-xs text-gray-500">
      <span>Due: {{ formatDate(task.dueDate) }}</span>
      <span>Assigned to: {{ task.assignedToUsername }}</span>
    </div>

    <Teleport to="body">
      <EditTaskModal v-if="showEditModal" :task="task" @close="showEditModal = false" @task-updated="handleTaskUpdated" />
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useTaskStore } from '@/stores/taskStore';
import EditTaskModal from '@/components/EditTaskModal.vue';
import type { TaskItem, UpdateTaskItemRequest } from '@/types';

interface Props {
  task: TaskItem;
}

interface Emits {
  (e: 'task-updated', event: { taskId: string; updates: any }): void;
}

const emit = defineEmits<Emits>();
const props = defineProps<Props>();
const task = ref<TaskItem>({ ...props.task });

const taskStore = useTaskStore();
const showEditModal = ref(false);

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString();
};

const handleTaskUpdated = (event: { taskId: string; updates: UpdateTaskItemRequest }): void => {
  emit('task-updated', { taskId: event.taskId, updates: event.updates });
};

const handleDelete = async (): Promise<void> => {
  if (window.confirm('Are you sure you want to delete this task?')) {
    await taskStore.deleteTask(task.value.publicId);
  }
};

watch(
  () => props.task,
  (newTask) => {
    task.value = newTask;
  }
);
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>
