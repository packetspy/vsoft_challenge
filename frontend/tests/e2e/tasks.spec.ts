import { test, expect } from '@playwright/test';

test.describe('Task Management', () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto('/login');
    await page.fill('input[type="text"]', 'testuser');
    await page.fill('input[type="password"]', 'password123');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL('/');
    await page.goto('/tasks');
  });

  test('should display columns', async ({ page }) => {
    // Verify columns are present
    await expect(page.locator('[data-testid="pending-column"]')).toBeVisible();
    await expect(page.locator('[data-testid="in-progress-column"]')).toBeVisible();
    await expect(page.locator('[data-testid="completed-column"]')).toBeVisible();
  });

  test('should create a new pending task', async ({ page }) => {
    // Click create task button
    await page.click('[data-testid="create-task-button"]');

    // Fill task form
    await page.fill('input#title', 'New E2E Task');
    await page.fill('textarea#description', 'E2E Test Description');

    // Set future date
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 1);
    const dateTimeString = futureDate.toISOString().slice(0, 16);
    await page.fill('input#dueDate', dateTimeString);

    // Submit form
    await page.click('button[type="submit"]');

    await page.goto('/tasks');

    const taskCards = page.locator('[data-testid="task-card"]').all();
    for (const itemLocator of await taskCards) {
      await itemLocator.hasText('New E2E Task');
    }
  });

  test('should display tasks in columns', async ({ page }) => {
    // Verify columns are present
    await expect(page.locator('[data-testid="pending-column"]')).toBeVisible();
    await expect(page.locator('[data-testid="in-progress-column"]')).toBeVisible();
    await expect(page.locator('[data-testid="completed-column"]')).toBeVisible();

    // Verify tasks are displayed
    await expect(page.locator('[data-testid="task-card"]').first()).toBeVisible();
  });

  test('should edit an existing task', async ({ page }) => {
    await page.goto('/tasks');

    const firstTaskCard = page.locator('[data-testid="task-card"]').first();

    // Click edit on first task
    await firstTaskCard.locator('[data-testid="edit-task"]').click();

    // Update task title
    await page.fill('input#edit-title', 'Updated Task Title');

    // Submit form
    await page.click('button[type="submit"]');

    // Verify task was updated
    await expect(page.locator('[data-testid="task-card"]', { hasText: 'Updated Task Title' })).toBeVisible();
  });

  test('should delete a task', async ({ page }) => {
    // Setup dialog handler
    page.on('dialog', (dialog) => dialog.accept());

    // Wait for task cards to load
    await page.waitForTimeout(1000);
    const taskCards = page.locator('[data-testid="task-card"]');
    const initialTaskCount = await taskCards.count();

    if (initialTaskCount === 0) {
      test.skip();
      return;
    }

    // Click delete on first task
    await taskCards.first().locator('[data-testid="delete-task"]').click();

    // Verify UI update
    await expect(taskCards).toHaveCount(initialTaskCount - 1);
  });

  test('should drag and drop tasks between columns', async ({ page }) => {
    await page.goto('/tasks');
    await page.waitForTimeout(1000);

    const pendingColumn = page.locator('[data-testid="pending-column"]');
    const inProgressColumn = page.locator('[data-testid="in-progress-column"]');

    // Get initial counts and task details
    const pendingTasks = pendingColumn.locator('[data-testid="task-card"]');
    const inProgressTasks = inProgressColumn.locator('[data-testid="task-card"]');

    const initialPendingCount = await pendingTasks.count();
    const initialInProgressCount = await inProgressTasks.count();

    // Skip test if no tasks in pending
    if (initialPendingCount === 0) {
      test.skip('No tasks available in pending column');
      return;
    }

    // Get task details before moving
    const taskToMove = pendingTasks.first();
    const taskTitle = await taskToMove.locator('[data-testid="task-title"]').textContent();

    // Perform drag and drop
    await taskToMove.dragTo(inProgressColumn);

    // Wait for UI update with proper condition
    await expect(pendingTasks).toHaveCount(initialPendingCount - 1);
    await expect(inProgressTasks).toHaveCount(initialInProgressCount + 1);

    // Verify the specific task moved to the new column
    if (taskTitle) {
      await expect(inProgressColumn.locator(`[data-testid="task-card"]:has-text("${taskTitle}")`)).toBeVisible();
    }
  });
});
