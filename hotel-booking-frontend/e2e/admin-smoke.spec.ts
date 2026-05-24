import { test, expect } from '@playwright/test';

const bootTimeout = { timeout: 30_000 };

test.describe('Admin dashboard smoke', () => {
  test('login page loads', async ({ page }) => {
    await page.goto('/login');
    await expect(page.locator('app-admin-login')).toBeVisible(bootTimeout);
    await expect(page.locator('input[type="email"]')).toBeVisible(bootTimeout);
    await expect(page.locator('input[type="password"]')).toBeVisible(bootTimeout);
  });
});
