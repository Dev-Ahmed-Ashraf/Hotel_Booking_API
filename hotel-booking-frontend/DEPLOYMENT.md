# Deployment guide — Hotel Booking Frontend

Two static SPAs are built and deployed independently: **customer-portal** and **admin-dashboard**.

## Build artifacts

```bash
npm ci
npm run build:customer   # dist/customer-portal/browser
npm run build:admin      # dist/admin-dashboard/browser
```

Serve each `browser` folder as a static site (Azure Static Web Apps, AWS S3 + CloudFront, Nginx, IIS, etc.). Configure **SPA fallback** so all routes return `index.html`.

## Environment variables (build-time)

Angular bakes environment files at compile time. For production, edit before build:

| File | Keys |
|------|------|
| `projects/customer-portal/src/environments/environment.ts` | `apiBaseUrl`, `stripePublishableKey` |
| `projects/admin-dashboard/src/environments/environment.ts` | `apiBaseUrl` |

| Key | Example | Notes |
|-----|---------|--------|
| `apiBaseUrl` | `https://api.yourdomain.com/api` | Must match API base including `/api` suffix |
| `stripePublishableKey` | `pk_live_...` | Customer portal only; use live key in production |

For CI/CD, generate `environment.ts` from secrets in the pipeline instead of committing production URLs.

## Backend CORS

The API must allow the deployed origins. In development, `Program.cs` (or equivalent) should include:

- `http://localhost:4200` — customer portal
- `http://localhost:4201` — admin dashboard

For production, add your real origins, e.g.:

- `https://book.yourdomain.com`
- `https://admin.yourdomain.com`

Allow credentials if you later move tokens to HttpOnly cookies.

## API URL and HTTPS

- Use HTTPS in production for both API and frontends.
- `apiBaseUrl` must match the API’s public URL (including `/api`).
- If the API uses a custom certificate, browsers must trust it.

## Stripe

- Customer portal loads Stripe.js with `stripePublishableKey`.
- Webhooks and secret keys stay on the **API** only.
- Use Stripe test keys in staging; live keys only in production.

## Routing and deep links

Both apps use client-side routing. The host must rewrite unknown paths to `index.html`:

**Nginx example**

```nginx
location / {
  try_files $uri $uri/ /index.html;
}
```

## Security checklist

- [ ] Production `environment.ts` does not contain dev localhost URLs
- [ ] CORS restricted to known frontend origins
- [ ] HTTPS everywhere
- [ ] Content-Security-Policy aligned with Stripe and API domains
- [ ] No secrets in the frontend bundle (only publishable Stripe key)

## Monitoring

`shared-monitoring` exposes `MonitoringService`. Replace `NoopMonitoringService` with your provider (Application Insights, Sentry, etc.) in each app’s `app.config.ts` when ready.

## Optional: single domain, two paths

You can host both apps under one domain (`/app` and `/admin`) with separate base-href builds:

```bash
ng build customer-portal --base-href /app/
ng build admin-dashboard --base-href /admin/
```

Adjust router and asset paths accordingly.

## Health check

After deploy:

1. Open customer home — static shell loads.
2. Open `/hotels` — list or empty state (API reachable).
3. Open admin `/login` — sign-in form loads.
4. Sign in with a test account — dashboard or redirect works.

## Troubleshooting

| Symptom | Likely cause |
|---------|----------------|
| Blank page after refresh on `/hotels/1` | Missing SPA fallback |
| CORS errors in console | Origin not allowed on API |
| 401 on all API calls | Wrong `apiBaseUrl` or clock skew on JWT |
| Stripe payment fails | Wrong publishable key or API webhook/config |
