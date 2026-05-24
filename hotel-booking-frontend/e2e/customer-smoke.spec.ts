import { test, expect } from '@playwright/test';

const bootTimeout = { timeout: 30_000 };

test.describe('Customer portal smoke', () => {
  test('home page loads', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('app-home')).toBeVisible(bootTimeout);
    await expect(page.locator('a[href="/hotels"]').first()).toBeVisible(bootTimeout);
  });

  test('hotels list page loads', async ({ page }) => {
    await page.goto('/hotels');
    await expect(page.locator('app-hotel-list')).toBeVisible(bootTimeout);
    await expect(page.locator('form.filters')).toBeVisible(bootTimeout);
  });

  test('login page loads', async ({ page }) => {
    await page.goto('/auth/login');
    await expect(page.locator('app-login')).toBeVisible(bootTimeout);
    await expect(page.locator('input[type="email"]')).toBeVisible(bootTimeout);
  });

  test('register page loads', async ({ page }) => {
    await page.goto('/auth/register');
    await expect(page.locator('app-register')).toBeVisible(bootTimeout);
    await expect(page.locator('input[type="email"]')).toBeVisible(bootTimeout);
  });
});
