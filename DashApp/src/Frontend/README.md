# Frontend

## Overview

The frontend is a Vue 3 single-page application that presents the DashApp dashboard and delegates authentication and provider integration to the BFF. Its responsibilities are narrow: render the current UI, route the user between the home view and the dashboard, and call the BFF for session and dashboard state.

The SPA uses:

- Vue 3 with the Composition API
- Vue Router
- TypeScript
- Vite
- Tailwind CSS via the Vite plugin

## Folder structure

- [src/main.ts](src/main.ts) mounts the application and installs the router.
- [src/App.vue](src/App.vue) provides the root layout.
- [src/router/index.ts](src/router/index.ts) defines the application routes.
- [src/views](src/views) contains the current page-level views.
- [src/api](src/api) contains wrappers for the BFF endpoints.
- [src/types](src/types) contains shared TypeScript interfaces.
- [src/style.css](src/style.css) contains the app-level styles and Tailwind import.

## Application flow

The application starts in [src/main.ts](src/main.ts), which mounts the root Vue app and enables routing. The router then renders either the home page or the dashboard based on the current URL.

The UI is not responsible for any provider OAuth logic itself. Instead, it redirects the browser to the BFF for authentication and provider linking flows.

## Routing

The current router configuration is defined in [src/router/index.ts](src/router/index.ts):

- `/` renders the home page
- `/dashboard` renders the dashboard page

There is no separate route for provider callbacks or API routes in the SPA router. Those remain handled by the BFF.

## API layer

The SPA uses a small helper object in [src/api/bff.ts](src/api/bff.ts). That file centralizes the browser-side calls to the BFF:

- `login()` redirects the browser to `/login`
- `logout()` submits a POST to `/logout`
- `connect(provider)` redirects to `/connect/{provider}`
- `unlink(provider)` posts to `/unlink/{provider}`
- `session()` calls `/api/session`
- `dashboard()` calls `/api/dashboard`

The browser uses `credentials: 'same-origin'` for the session and dashboard calls so the cookie-based session is sent automatically.

### Authentication behaviour

The SPA checks the current session by calling `/api/session`. If the response is `401`, the helper returns `null`, and the view can decide whether to redirect to the home page or prompt the user to sign in.

This means the UI does not perform its own token management. Authentication state is discovered by asking the BFF for the current session.

## Views

### Home view

The home view is implemented in [src/views/HomeView.vue](src/views/HomeView.vue). Its purpose is to provide the initial landing screen and to redirect authenticated users to the dashboard automatically.

The current implementation:

- checks the current session on mount
- sends the user to `/dashboard` when a session exists
- otherwise leaves the user on the sign-in screen

### Dashboard view

The dashboard view is implemented in [src/views/DashboardView.vue](src/views/DashboardView.vue). It is responsible for:

- loading the dashboard payload from the BFF
- displaying the current user name
- rendering provider tiles for the available integrations
- allowing the user to connect or unlink providers
- handling logout

## Dashboard

The dashboard data comes from the BFF’s `/api/dashboard` endpoint. The frontend expects the response to contain a `tiles` array where each tile describes one provider connection state.

The current UI renders one tile per provider and shows:

- the provider name
- whether the provider is connected
- a connect button when disconnected
- a connected badge and account name when connected
- an unlink action when connected

The connect flow is currently implemented as a navigation to `/connect/{provider}`. The BFF handles the provider-specific challenge and redirect back to the dashboard.

The unlink flow calls `BFF.unlink(provider)` and then reloads the dashboard state from the BFF.

## Development

### Vite

The frontend is served by Vite in [vite.config.ts](vite.config.ts). The project uses the Vue plugin and the Tailwind Vite plugin.

### Development proxy

The BFF provides a development reverse proxy to the Vite app. During local development, the browser can continue to use the BFF host while the BFF forwards requests to the Vite dev server on `http://localhost:5173`.

### Running locally

From [src/Frontend](.) :

```powershell
npm install
npm run dev
```

The Vite dev server runs on port `5173` by default.

## Styling

The app uses Tailwind CSS through the Vite plugin and imports Tailwind from [src/style.css](src/style.css). The components use Tailwind utility classes directly, so styling is mostly local to the component templates rather than being split into a separate design system layer.

## Authentication UX

The frontend’s authentication experience is intentionally simple:

- the home view detects whether the user already has a session
- the dashboard view detects whether the user is still authenticated
- the login action redirects the browser to the BFF `/login` route
- the logout action submits a POST to the BFF `/logout` route
- provider linking is initiated by navigating to `/connect/{provider}`

The frontend does not manage tokens directly. Browser redirects and same-origin fetches are used so that the BFF remains the authority for authentication.

## Current limitations

The current frontend implementation has a few important limitations:

- it only supports a single dashboard route and a simple home page
- it only renders the GitHub provider tile in the current dashboard state
- it uses a very small in-browser state model with no dedicated store
- it does not implement a richer error or loading state beyond the current message and spinner behavior
