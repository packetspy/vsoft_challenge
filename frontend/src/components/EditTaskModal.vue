<template>
  <div class="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50" @click="handleBackdropClick">
    <div ref="modalContent" class="bg-white rounded-lg shadow-xl max-w-md w-full max-h-[90vh] overflow-y-auto relative z-10" @click.stop>
      <div class="p-6">
        <div class="flex justify-between items-center mb-4">
          <h3 class="text-lg font-semibold text-gray-900">Edit Task</h3>
          <button @click="closeModal" class="text-gray-400 hover:text-gray-600 transition-colors">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <form @submit.prevent="handleSubmit" class="space-y-4">
          <div>
            <label for="edit-title" class="block text-sm font-medium text-gray-700 mb-1"> Title * </label>
            <input
              id="edit-title"
              v-model="form.title"
              type="text"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
            />
          </div>

          <div>
            <label for="edit-description" class="block text-sm font-medium text-gray-700 mb-1"> Description </label>
            <textarea
              id="edit-description"
              v-model="form.description"
              rows="3"
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
            ></textarea>
          </div>

          <div>
            <label for="edit-dueDate" class="block text-sm font-medium text-gray-700 mb-1"> Due Date * </label>
            <input
              id="edit-dueDate"
              v-model="form.dueDate"
              type="datetime-local"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
            />
          </div>

          <div>
            <label for="edit-status" class="block text-sm font-medium text-gray-700 mb-1"> Status </label>
            <select id="edit-status" v-model="form.status" class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500">
              <option value="0">Pending</option>
              <option value="1">In Progress</option>
              <option value="2">Completed</option>
            </select>
          </div>

          <div>
            <label for="edit-assignedTo" class="block text-sm font-medium text-gray-700 mb-1"> Assign To </label>
            <select
              id="edit-assignedTo"
              v-model="form.assignedToId"
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
            >
              <option v-for="user in users" :key="user.publicId" :value="user.publicId" :selected="user.publicId === task.assignedToId">
                {{ user.username }}
              </option>
            </select>
          </div>

          <div v-if="error" class="text-red-600 text-sm">
            {{ error }}
          </div>

          <div class="flex justify-end space-x-3 pt-4">
            <button type="button" @click="closeModal" class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-md transition-colors">Cancel</button>
            <button
              type="submit"
              :disabled="isSubmitting"
              class="px-4 py-2 text-sm font-medium text-white bg-emerald-600 hover:bg-emerald-700 rounded-md transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <span v-if="isSubmitting">Updating...</span>
              <span v-else>Update Task</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, onUnmounted } from 'vue';
import { useTaskStore } from '@/stores/taskStore';
import { useUserStore } from '@/stores/userStore';
import type { TaskItem, UpdateTaskItemRequest, User, TaskItemStatus } from '@/types';

interface Props {
  task: TaskItem;
}

interface Emits {
  (e: 'close'): void;
  (e: 'task-updated', event: { taskId: string; updates: UpdateTaskItemRequest }): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const taskStore = useTaskStore();
const userStore = useUserStore();

const formatDateForInput = (dateString: string): string => {
  const date = new Date(dateString);
  return date.toISOString().slice(0, 16);
};

const form = ref<UpdateTaskItemRequest & { dueDate: string }>({
  title: props.task.title,
  description: props.task.description,
  dueDate: formatDateForInput(props.task.dueDate),
  status: props.task.status,
  assignedToId: props.task.assignedToId,
});

const users = ref<User[]>([]);
const isSubmitting = ref(false);
const error = ref('');
const modalContent = ref<HTMLElement>();

watch(
  () => props.task,
  (newTask) => {
    form.value = {
      title: newTask.title,
      description: newTask.description,
      dueDate: formatDateForInput(newTask.dueDate),
      status: newTask.status,
      assignedToId: newTask.assignedToId,
    };
  }
);

const handleEscapeKey = (event: KeyboardEvent) => {
  if (event.key === 'Escape') {
    closeModal();
  }
};

const handleBackdropClick = (event: MouseEvent) => {
  if (event.target === event.currentTarget) closeModal();
};

const closeModal = (): void => {
  emit('close');
};

onMounted(async () => {
  try {
    document.addEventListener('keydown', handleEscapeKey);
    await userStore.fetchAllUsers();
    users.value = userStore.users.map((u) => ({
      publicId: u.publicId,
      username: u.username,
      email: u.email,
      permissions: [...u.permissions],
    }));
  } catch (err) {
    console.error('Failed to fetch users:', err);
    error.value = 'Failed to load users';
  }
});

onUnmounted(() => {
  document.removeEventListener('keydown', handleEscapeKey);
});

const handleSubmit = async (): Promise<void> => {
  try {
    isSubmitting.value = true;
    error.value = '';

    const dueDate = new Date(form.value.dueDate!);
    if (dueDate <= new Date()) {
      error.value = 'Due date must be in the future';
      return;
    }

    const updates: UpdateTaskItemRequest = {
      title: form.value.title,
      description: form.value.description,
      dueDate: form.value.dueDate,
      status: form.value.status as TaskItemStatus,
      assignedToId: form.value.assignedToId,
    };

    await taskStore.updateTask(props.task.publicId, updates);
    emit('task-updated', { taskId: props.task.publicId, updates });
    closeModal();
  } catch (err) {
    error.value = 'Failed to update task';
    console.error('Error updating task:', err);
  } finally {
    isSubmitting.value = false;
  }
};
</script>
