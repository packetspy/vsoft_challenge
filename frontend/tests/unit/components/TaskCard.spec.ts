import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import { setActivePinia, createPinia } from 'pinia';
import TaskCard from '@/components/TaskCard.vue';
import type { TaskItem } from '@/types';
import { TaskItemStatus } from '@/types';

// Mock the stores
const mockDeleteTask = vi.fn();
vi.mock('@/stores/taskStore', () => ({
  useTaskStore: () => ({
    deleteTask: mockDeleteTask,
  }),
}));

// Mock vue-toast-notification
vi.mock('vue-toast-notification', () => ({
  useToast: () => ({
    success: vi.fn(),
    error: vi.fn(),
  }),
}));

const mockTask: TaskItem = {
  publicId: '1',
  title: 'Test Task',
  description: 'Test Description',
  dueDate: '2024-01-01T00:00:00Z',
  status: TaskItemStatus.Pending,
  order: 0,
  assignedToId: 'user-1',
  assignedToUsername: 'john_doe',
  createdById: 'user-1',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
};

describe('TaskCard', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  it('renders task title and description correctly', () => {
    const wrapper = mount(TaskCard, {
      props: { task: mockTask },
      global: {
        plugins: [createPinia()],
      },
    });

    expect(wrapper.text()).toContain('Test Task');
    expect(wrapper.text()).toContain('Test Description');
  });

  it('displays assigned username when provided', () => {
    const wrapper = mount(TaskCard, {
      props: { task: mockTask },
    });

    expect(wrapper.text()).toContain('john_doe');
  });

  it('displays assigned username when provided', () => {
    const wrapper = mount(TaskCard, {
      props: { task: mockTask },
      global: {
        plugins: [createPinia()],
      },
    });

    expect(wrapper.text()).toContain('john_doe');
  });

  it('displays empty assigned user when not provided', () => {
    const unassignedTask = { ...mockTask, assignedToUsername: undefined };
    const wrapper = mount(TaskCard, {
      props: { task: unassignedTask },
      global: {
        plugins: [createPinia()],
      },
    });

    // Should show empty or undefined
    expect(wrapper.text()).toContain('Assigned to:');
  });

  it('triggers edit modal when edit button is clicked', async () => {
    const wrapper = mount(TaskCard, {
      props: { task: mockTask },
      global: {
        plugins: [createPinia()],
      },
    });

    // Click the edit button (first button)
    await wrapper.find('button').trigger('click');

    // Check if the modal opens (showEditModal becomes true)
    expect(wrapper.vm.showEditModal).toBe(true);
  });

  it('calls delete method when delete button is clicked', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);

    const wrapper = mount(TaskCard, {
      props: { task: mockTask },
      global: {
        plugins: [createPinia()],
      },
    });

    // Click the delete button (second button)
    const buttons = wrapper.findAll('button');
    await buttons[1].trigger('click');

    expect(confirmSpy).toHaveBeenCalledWith('Are you sure you want to delete this task?');
    expect(mockDeleteTask).toHaveBeenCalledWith(mockTask.publicId);

    confirmSpy.mockRestore();
  });

  it('has correct basic styling', () => {
    const wrapper = mount(TaskCard, {
      props: { task: mockTask },
      global: {
        plugins: [createPinia()],
      },
    });

    // Check for basic card classes
    expect(wrapper.classes()).toContain('bg-white');
    expect(wrapper.classes()).toContain('rounded-lg');
    expect(wrapper.classes()).toContain('shadow-md');
  });

  it('formats due date correctly', () => {
    const wrapper = mount(TaskCard, {
      props: { task: mockTask },
      global: {
        plugins: [createPinia()],
      },
    });

    // The component uses toLocaleDateString(), so let's check for the formatted date
    const expectedDate = new Date(mockTask.dueDate).toLocaleDateString();
    expect(wrapper.text()).toContain(expectedDate);
  });
});
