# WebApi Template

A minimal ASP.NET Core Web API template demonstrating enterprise-friendly
cross-cutting concerns for a Backend For Frontend (BFF).

## Features

- Standard request headers (`x-correlation-id`, `x-session-id`) enforced with a
  marker attribute, middleware, an action filter, and FluentValidation.
- A centralized exception handler that maps every failure to a consistent
  `ErrorResponse` envelope with a machine-readable code, trace id, and correlation id.
- The options pattern with startup validation for downstream services.
- A typed `HttpClient` with a configurable timeout and downstream error mapping.
- A liveness endpoint at `/api/health` for orchestrators such as Kubernetes.

## Structure

- `Attributes` - marker attributes that opt endpoints into behaviors.
- `Constants` - shared header names and error codes and messages.
- `Controllers` - API controllers, including the health check.
- `Exceptions` - application exception types mapped to HTTP status codes.
- `Filters` - action filters and Swagger operation filters.
- `Handlers` - the global exception handler.
- `Middleware` - request middleware.
- `Models` - request and response models.
- `Options` - strongly typed configuration.
- `Services` - application services.
- `Validators` - FluentValidation validators.

## Running

```bash
dotnet run
```

The API listens on `http://localhost:5285`. Swagger is available at `/swagger`
in the Development environment.

## Example requests

```bash
# Requires a UUIDv4 x-session-id header and an orderType query parameter.
curl "http://localhost:5285/api/orders?orderType=test" -H "X-Session-ID: <uuidv4>"

# Liveness probe.
curl "http://localhost:5285/api/health"
```

## Error contract

Every error response uses a single envelope:

```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred.",
  "timestamp": "2026-09-13T00:00:00+00:00",
  "traceId": "0HNO...",
  "correlationId": "78f27d6b-...",
  "referenceId": null
}
```

| Code               | HTTP status | Meaning                         |
| ------------------ | ----------- | ------------------------------- |
| `VALIDATION_ERROR` | 400         | Request validation failed.      |
| `DOWNSTREAM_ERROR` | 502         | A downstream call failed.       |
| `TIMEOUT`          | 504         | A downstream call timed out.    |
| `UNEXPECTED_ERROR` | 500         | An unexpected error occurred.   |
