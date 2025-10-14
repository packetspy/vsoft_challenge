<template>
  <div class="bg-gray-100 rounded-lg p-4">
    <h3 class="text-lg font-semibold text-gray-800 mb-4">{{ title }}</h3>
    <div ref="columnEl" class="min-h-[200px] space-y-3">
      <TaskCard v-for="task in tasks" :key="task.publicId" :task="task" @task-updated="handleTaskUpdated" data-testid="task-card" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, watch } from 'vue';
import { useTaskStore } from '@/stores/taskStore';
import type { TaskItem, UpdateTaskItemRequest } from '@/types';
import { TaskItemStatus } from '@/types';
import TaskCard from '@/components/TaskCard.vue';
import Sortable from 'sortablejs';

interface Props {
  title: string;
  tasks: TaskItem[];
  status: TaskItemStatus;
}

interface Emits {
  (e: 'task-reorder', updates: { taskId: string; status: TaskItemStatus; order: number }[]): void;
  (e: 'task-moved', taskId: string, newStatus: TaskItemStatus, newOrder: number): void;
  (e: 'task-updated', taskId: string, updates: UpdateTaskItemRequest): void;
  (e: 'task-deleted', taskId: string): void;
}

const taskStore = useTaskStore();
const tasks = ref<TaskItem[]>([]);
const props = defineProps<Props>();
const emit = defineEmits<Emits>();
const columnEl = ref<HTMLElement>();
let sortable: Sortable | null = null;

watch(
  () => {
    switch (props.status) {
      case TaskItemStatus.Pending:
        return taskStore.pendingTasks;
      case TaskItemStatus.InProgress:
        return taskStore.inProgressTasks;
      case TaskItemStatus.Completed:
        return taskStore.completedTasks;
      default:
        return [];
    }
  },
  (newTasks) => {
    tasks.value = newTasks;
  },
  { immediate: true, deep: true }
);

onMounted(() => {
  if (columnEl.value) {
    sortable = Sortable.create(columnEl.value, {
      group: 'tasks',
      animation: 150,
      ghostClass: 'opacity-50',
      onAdd: (evt) => {
        const taskId = evt.item.dataset.taskId;
        const newIndex = evt.newIndex!;

        if (!taskId) {
          console.error('Task ID is undefined when moving task between columns.');
          return;
        }

        emit('task-moved', taskId, props.status, newIndex);
      },
      onUpdate: (evt) => {
        const taskId = evt.item.dataset.taskId!;
        const newIndex = evt.newIndex!;
        const updates = [
          {
            taskId,
            status: props.status,
            order: newIndex,
          },
        ];

        emit('task-reorder', updates);
      },
    });
  }
});

onUnmounted(() => {
  sortable?.destroy();
});

const handleTaskUpdated = (event: { taskId: string; updates: UpdateTaskItemRequest }): void => {
  emit('task-updated', event.taskId, event.updates);
};

const handleDeleteTask = async (taskId: string): Promise<void> => {
  if (confirm('Are you sure you want to delete this task?')) {
    try {
      await taskStore.deleteTask(taskId);
      emit('task-deleted', taskId);
    } catch (error) {
      console.error('Error deleting task:', error);
    }
  }
};

const handleTaskReorder = (taskId: string, newIndex: number): void => {
  const updates = [
    {
      taskId,
      status: props.status,
      order: newIndex,
    },
  ];

  emit('task-reorder', updates);
};
</script>
