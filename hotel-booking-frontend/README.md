# Hotel Booking Frontend

Enterprise Angular 19 monorepo for the Hotel Booking API: a **customer portal** (browse, book, pay, reviews) and an **admin dashboard** (hotels, rooms, bookings, reviews, KPIs).

## Prerequisites

- Node.js 20+
- npm 10+
- [Hotel Booking API](https://github.com) running at `https://localhost:7062` (see backend repo)
- Trust the dev HTTPS certificate in your browser if prompted

## Quick start

```bash
cd hotel-booking-frontend
npm install
npm run generate:api   # after updating tools/openapi/swagger.json
npm run start:customer # http://localhost:4200
npm run start:admin    # http://localhost:4201 (separate terminal)
```

## Applications

| App | Port | Audience | Main routes |
|-----|------|----------|-------------|
| `customer-portal` | 4200 | Customers | `/`, `/hotels`, `/booking`, `/my-bookings`, `/my-reviews`, `/auth/login` |
| `admin-dashboard` | 4201 | Admin, HotelManager | `/login`, `/dashboard`, `/hotels`, `/rooms`, `/bookings`, `/reviews` |

## Shared libraries

Path aliases use `@hotel/shared/*`:

| Library | Purpose |
|---------|---------|
| `shared-models` | Shared TypeScript types |
| `shared-core` | API errors, utilities |
| `shared-data-access` | OpenAPI-generated client + facades |
| `shared-auth` | JWT store, guards, HTTP interceptors |
| `shared-i18n` | Transloco (en-US, ar RTL), locale/formatting |
| `shared-monitoring` | Monitoring abstraction (noop in dev) |
| `shared-ui` | Shell, theme, hotel cards, empty/loading/error UI |

## Configuration

Development API and Stripe keys live in:

- `projects/customer-portal/src/environments/environment.development.ts`
- `projects/admin-dashboard/src/environments/environment.development.ts`

Production builds use `environment.ts` — replace placeholders before deploy (see [DEPLOYMENT.md](./DEPLOYMENT.md)).

## Scripts

| Command | Description |
|---------|-------------|
| `npm run start:customer` | Customer portal dev server (:4200) |
| `npm run start:admin` | Admin dashboard dev server (:4201) |
| `npm run build:customer` | Production build → `dist/customer-portal` |
| `npm run build:admin` | Production build → `dist/admin-dashboard` |
| `npm run generate:api` | Regenerate client from `tools/openapi/swagger.json` |
| `npm run test` | Karma unit tests |
| `npm run e2e` | Playwright smoke tests (starts dev servers if needed) |
| `npm run e2e:ui` | Playwright UI mode |

## API client generation

1. Export OpenAPI from the backend into `tools/openapi/swagger.json`.
2. Run `npm run generate:api`.
3. Facades in `projects/shared-data-access/src/lib/facades/` wrap generated services with `unwrapApiResponse`.

## Auth

- JWT stored in `localStorage`; user profile in `sessionStorage`.
- `authGuard` redirects guests to login with `returnUrl`.
- Admin login accepts roles **Admin** and **HotelManager** only.

## i18n

- **en-US** (LTR) and **ar** (RTL) via Transloco.
- Shared strings: `projects/shared-i18n/assets/i18n/`.
- App-specific: `projects/customer-portal/src/assets/i18n/`, `projects/admin-dashboard/src/assets/i18n/`.

## UI stack

- Angular Material + Tailwind CSS
- Theme tokens: primary `#2563EB`, secondary `#0F172A`, light/dark toggle in shell

## E2E tests

Playwright smoke specs live in `e2e/`. They verify shell pages load without requiring seeded data. Run with both apps’ dev servers already up, or let Playwright start them via `webServer` in `playwright.config.ts`.

## Related documentation

- [DEPLOYMENT.md](./DEPLOYMENT.md) — hosting, CORS, environment variables
- Backend: `Hotel_Booking_API` in the parent solution folder

## License

Private / project use — align with the backend repository license.
