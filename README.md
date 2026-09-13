# dn-lab

A collection of independent .NET development projects used for experimenting with
ASP.NET Core, authentication/authorization, and API design. Each top-level folder
is a self-contained project with its own dependencies and build.

| Project | Description |
| --- | --- |
| [DashApp](#dashapp) | Vue 3 SPA + ASP.NET Core backend-for-frontend (BFF) with OIDC/OAuth |
| [OAuthClean](#oauthclean) | Minimal ASP.NET Core OAuth provider-linking sample |
| [WebApi](#webapi) | Enterprise-friendly Web API template for a BFF |

---

## DashApp

**Summary**

An authentication-oriented sample that combines a Vue 3 single-page application
with an ASP.NET Core backend-for-frontend. The BFF is the browser-facing entry
point for sign-in (Microsoft Entra ID via OpenID Connect), optional GitHub account
linking via OAuth, and server-side token handling. The Vue app is intentionally
thin: it renders the UI and calls the BFF over same-origin HTTP, never receiving
provider access tokens directly.

**Frameworks used**

- ASP.NET Core (`.NET 8.0`) for the BFF
- Vue 3 + Vue Router 4 as the frontend, built with Vite, TypeScript, and Tailwind CSS 4
- Microsoft.Identity.Web for Entra ID integration
- YARP Reverse Proxy for proxying non-API requests to the Vite dev server
- Swashbuckle (Swagger/OpenAPI)

**Patterns used**

- Backend-for-frontend (BFF) architecture
- Cookie-based authentication with OpenID Connect and OAuth challenges
- Claims transformation for provider-specific identity data
- Server-side token storage (in-memory token database)
- Minimal APIs grouped into endpoint route modules
- Reverse proxy configuration via the options pattern
- Same-origin browser communication (SPA never receives provider tokens)

## OAuthClean

**Summary**

A minimal ASP.NET Core sample that demonstrates a clean, generic OAuth provider
linking flow. It issues a cookie session, then lets users connect a YouTube
(Google) account through OAuth, storing the resulting access token server-side and
keyed by user. The focus is on a reusable challenge-router design rather than
hard-coded per-provider routes.

**Frameworks used**

- ASP.NET Core (`.NET 8.0`)
- Swashbuckle (Swagger/OpenAPI)

**Patterns used**

- Minimal APIs with extension-method endpoint mapping
- Cookie authentication with a generic challenge router
  (`ChallengeRouteOptions` maps path prefixes to authentication schemes via `IOptions`)
- OAuth provider registration and per-provider event hooks
- Claims transformation (`IClaimsTransformation`) to surface provider state
- Typed `HttpClient` for the downstream YouTube API
- In-memory token store keyed by user id
- Authorization policies with claim requirements

## WebApi

**Summary**

A minimal ASP.NET Core Web API template demonstrating enterprise-friendly
cross-cutting concerns for a backend-for-frontend: enforced standard request
headers, a consistent error envelope, configuration validation at startup, and a
typed downstream client with error mapping.

**Frameworks used**

- ASP.NET Core (`.NET 10.0`)
- FluentValidation 12
- Swashbuckle (Swagger/OpenAPI)
- Ulid for reference ids

**Patterns used**

- Options pattern with data-annotation validation and `ValidateOnStart`
- Typed `HttpClient` with configurable timeout and downstream error mapping
- FluentValidation for request validation
- `IExceptionHandler` + `ProblemDetails` for a global error envelope
- Cross-cutting concerns layered across middleware, action filters, and marker
  attributes (`x-correlation-id`, `x-session-id`)
- Centralized constants for error codes, messages, and header names
- Primary constructors for controllers and services
