# Pondhawk.Exceptions

Transport-agnostic error model and common exception types. Use it alone, or with [`Pondhawk.Mediator`](https://www.nuget.org/packages/Pondhawk.Mediator), which converts thrown exceptions to a `Response<T>` failure.

| Type | Purpose |
|------|---------|
| `ExternalException` | Base for application-facing errors with kind, code, explanation, details |
| `FluentException<T>` | Fluent builder API: `.WithKind().WithErrorCode().WithExplanation()` |
| `FunctionalException` | Business logic errors |
| `FailedValidationException` | Validation failures with violation details (`Kind = Predicate`) |
| `NotFoundException` | Resource not found (`Kind = NotFound`) |
| `ConflictException` | State conflict (`Kind = Conflict`) |
| `NotAuthorizedException` | Caller not authorized (`Kind = NotAuthorized`) |
| `InternalException` | Internal/system errors |
| `ErrorInfo` | Transport-agnostic error shape shared by exceptions and the `Response<T>` envelope |
| `ErrorKindPolicy` | `IsTransient(ErrorKind)` — canonical retry/dead-letter default |
| `ProblemDetail` | RFC 7807 problem detail for HTTP APIs |

