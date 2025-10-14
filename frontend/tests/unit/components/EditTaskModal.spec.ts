import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount, flushPromises } from '@vue/test-utils';
import { setActivePinia, createPinia } from 'pinia';
import EditTaskModal from '@/components/EditTaskModal.vue';
import type { TaskItem, User } from '@/types';
import { TaskItemStatus } from '@/types';

// Mock stores
const mockUpdateTask = vi.fn();
const mockFetchAllUsers = vi.fn();

vi.mock('@/stores/taskStore', () => ({
  useTaskStore: () => ({
    updateTask: mockUpdateTask,
  }),
}));

vi.mock('@/stores/userStore', () => ({
  useUserStore: () => ({
    users: [],
    fetchAllUsers: mockFetchAllUsers,
  }),
}));

// Mock vue-toast-notification
vi.mock('vue-toast-notification', () => ({
  useToast: () => ({
    success: vi.fn(),
    error: vi.fn(),
  }),
}));

describe('EditTaskModal', () => {
  const mockTask: TaskItem = {
    publicId: '1',
    title: 'Original Task',
    description: 'Original Description',
    dueDate: '2024-01-01T10:00:00Z',
    status: TaskItemStatus.Pending,
    order: 0,
    assignedToId: 'user-1',
    assignedToUsername: 'user1',
    createdById: 'user-1',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  };

  const mockUsers: User[] = [
    {
      publicId: 'user-1',
      username: 'user1',
      email: 'user1@example.com',
      permissions: [],
    },
    {
      publicId: 'user-2',
      username: 'user2',
      email: 'user2@example.com',
      permissions: [],
    },
  ];

  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  it('renders modal with task data pre-filled', () => {
    const wrapper = mount(EditTaskModal, {
      props: { task: mockTask },
      global: {
        plugins: [createPinia()],
      },
    });

    expect(wrapper.text()).toContain('Edit Task');

    const titleInput = wrapper.find('input#edit-title').element as HTMLInputElement;
    const descriptionTextarea = wrapper.find('textarea#edit-description').element as HTMLTextAreaElement;
    const statusSelect = wrapper.find('select#edit-status').element as HTMLSelectElement;

    expect(titleInput.value).toBe('Original Task');
    expect(descriptionTextarea.value).toBe('Original Description');
    // The status will be the enum value (0) as string, not the display string
    expect(statusSelect.value).toBe('0'); // TaskItemStatus.Pending = 0
  });

  it('updates form when task prop changes', async () => {
    const wrapper = mount(EditTaskModal, {
      props: { task: mockTask },
      global: {
        plugins: [createPinia()],
      },
    });

    const updatedTask: TaskItem = {
      ...mockTask,
      title: 'Updated Task',
      description: 'Updated Description',
      status: TaskItemStatus.InProgress,
    };

    await wrapper.setProps({ task: updatedTask });

    const titleInput = wrapper.find('input#edit-title').element as HTMLInputElement;
    const descriptionTextarea = wrapper.find('textarea#edit-description').element as HTMLTextAreaElement;
    const statusSelect = wrapper.find('select#edit-status').element as HTMLSelectElement;

    expect(titleInput.value).toBe('Updated Task');
    expect(descriptionTextarea.value).toBe('Updated Description');
    // The status will be the enum value (1) as string
    expect(statusSelect.value).toBe('1'); // TaskItemStatus.InProgress = 1
  });

  it('emits close event when backdrop is clicked', async () => {
    const wrapper = mount(EditTaskModal, {
      props: { task: mockTask },
    });

    await wrapper.find('.fixed.inset-0').trigger('click');

    expect(wrapper.emitted('close')).toBeTruthy();
  });

  it('emits close event when escape key is pressed', async () => {
    const wrapper = mount(EditTaskModal, {
      props: { task: mockTask },
    });

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));

    expect(wrapper.emitted('close')).toBeTruthy();
  });

  it('submits form with updated data', async () => {
    mockUpdateTask.mockResolvedValue({});

    const wrapper = mount(EditTaskModal, {
      props: { task: mockTask },
      global: {
        plugins: [createPinia()],
      },
    });

    // Update form fields
    await wrapper.find('input#edit-title').setValue('Updated Task Title');
    await wrapper.find('textarea#edit-description').setValue('Updated Description');
    await wrapper.find('select#edit-status').setValue('1'); // InProgress = 1

    // Set future date
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 1);
    const dateTimeString = futureDate.toISOString().slice(0, 16);
    await wrapper.find('input#edit-dueDate').setValue(dateTimeString);

    // Submit form
    await wrapper.find('form').trigger('submit.prevent');

    await flushPromises();

    expect(mockUpdateTask).toHaveBeenCalledWith('1', {
      title: 'Updated Task Title',
      description: 'Updated Description',
      dueDate: dateTimeString,
      status: '1', // The form will have this as a string
      assignedToId: 'user-1',
    });

    expect(wrapper.emitted('task-updated')).toBeTruthy();
    expect(wrapper.emitted('task-updated')?.[0]).toEqual([
      {
        taskId: '1',
        updates: {
          title: 'Updated Task Title',
          description: 'Updated Description',
          dueDate: dateTimeString,
          status: '1', // The string value from the select element
          assignedToId: 'user-1',
        },
      },
    ]);

    expect(wrapper.emitted('close')).toBeTruthy();
  });

  it('shows error when due date is in the past', async () => {
    const wrapper = mount(EditTaskModal, {
      props: { task: mockTask },
    });

    // Set past date
    const pastDate = new Date();
    pastDate.setDate(pastDate.getDate() - 1);
    const dateTimeString = pastDate.toISOString().slice(0, 16);
    await wrapper.find('input#edit-dueDate').setValue(dateTimeString);

    // Submit form
    await wrapper.find('form').trigger('submit.prevent');

    expect(wrapper.text()).toContain('Due date must be in the future');
    expect(wrapper.emitted('task-updated')).toBeFalsy();
  });

  it('shows error when task update fails', async () => {
    mockUpdateTask.mockRejectedValue(new Error('Update failed'));

    const wrapper = mount(EditTaskModal, {
      props: { task: mockTask },
      global: {
        plugins: [createPinia()],
      },
    });

    // Fill form with valid data
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 1);
    const dateTimeString = futureDate.toISOString().slice(0, 16);
    await wrapper.find('input#edit-dueDate').setValue(dateTimeString);

    // Submit form
    await wrapper.find('form').trigger('submit.prevent');

    await flushPromises();

    expect(wrapper.text()).toContain('Failed to update task');
  });

  it('cleans up event listeners when unmounted', () => {
    const removeEventListenerSpy = vi.spyOn(document, 'removeEventListener');

    const wrapper = mount(EditTaskModal, {
      props: { task: mockTask },
      global: {
        plugins: [createPinia()],
      },
    });

    wrapper.unmount();

    expect(removeEventListenerSpy).toHaveBeenCalledWith('keydown', expect.any(Function));
  });
});
