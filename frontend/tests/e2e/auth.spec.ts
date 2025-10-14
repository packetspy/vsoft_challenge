import { test, expect } from '@playwright/test';

test.describe('Authentication', () => {
  test('shoud create user successfully', async ({ page }) => {
    await page.goto('/register');

    // Fill registration form
    await page.fill('input[name="username"]', 'testuser');
    await page.fill('input[name="email"]', 'test@example.com');
    await page.fill('input[name="password"]', 'password123');
    await page.fill('input[name="confirmPassword"]', 'password123');

    // Click register button
    await page.click('button[type="submit"]');

    // Verify success message
    await expect(page.locator('.success-message')).toContainText('User registered successfully');
  });

  test('should login successfully with valid credentials', async ({ page }) => {
    await page.goto('/login');

    // Fill login form
    await page.fill('input[type="text"]', 'testuser');
    await page.fill('input[type="password"]', 'password123');

    // Click login button
    await page.click('button[type="submit"]');

    // Verify redirect to tasks page
    await expect(page).toHaveURL('/');
    await expect(page.locator('h1')).toContainText('Task Manager');
    await expect(page.locator('h2')).toContainText('Dashboard');
  });

  test('should logout successfully', async ({ page }) => {
    await page.goto('/login');

    // Fill login form
    await page.fill('input[type="text"]', 'testuser');
    await page.fill('input[type="password"]', 'password123');

    // Click login button
    await page.click('button[type="submit"]');

    // Verify redirect to tasks page
    await expect(page).toHaveURL('/');
    await expect(page.locator('h1')).toContainText('Task Manager');
    await expect(page.locator('h2')).toContainText('Dashboard');

    // Click logout
    await page.click('[data-testid="logout-button"]');

    // Wait for navigation
    await expect(page).toHaveURL('/login');
  });

  test('should show error with invalid credentials', async ({ page }) => {
    await page.goto('/login');

    // Fill login form with invalid credentials
    await page.fill('input[type="text"]', 'wronguser');
    await page.fill('input[type="password"]', 'wrongpassword');

    // Click login button
    await page.click('button[type="submit"]');

    // Verify error message
    await expect(page.locator('.error-message')).toContainText('Invalid username or password');
  });

  test('should redirect to login when accessing protected route unauthenticated', async ({ page }) => {
    await page.goto('/tasks');

    // Should redirect to login
    await expect(page).toHaveURL('/login');
  });
});
