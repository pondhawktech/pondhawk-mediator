# Build brief: mediator `Response<T>` envelope + shared error model (Pondhawk.Core)

> **Historical record.** This brief was written when the mediator was in `Pondhawk.Core`, in the repo
> then named `pondhawk-tools`. The code is now in `Pondhawk.Mediator` and `Pondhawk.Exceptions` in this
> repo. Paths and package names below are as they were at the time.

**Repo:** `pondhawk-tools` · **Package:** `Pondhawk.Core` · **Target:** `net10.0`

## Why

`Pondhawk.Mediator` is dispatched from three kinds of caller: **HTTP endpoints**, **queue
consumers**, and **batch**. Today `IMediator.SendAsync` returns the raw `TResponse` and **throws**
on error. That works for HTTP (middleware maps the exception) but is hostile everywhere else:

- A **queue consumer** has no HTTP pipeline. It must decide *retry vs. dead-letter* per message,
  which is a function of the error's **`ErrorKind`** — a signal that is lost the moment an error is
  a bare thrown exception or a string.
- **Batch** wants per-command success/failure so it can collect partial results; exceptions abort.

So the mediator should return a **`Response<T>` envelope** that carries either the value or a
structured error (**including the `ErrorKind`**). Domain code still **throws** `ErrorKind`
exceptions — nothing about authoring changes — the **mediator is the single seam** that converts a
throw into an envelope. Each edge then adapts the envelope to its transport.

## Scope

**In scope (this brief, Pondhawk.Core only):**

1. `ErrorInfo` — one shared error shape.
2. `Response<T>` — the success/failure envelope.
3. Enveloping dispatch — `IMediator.SendAsync` returns `Response<TResponse>`; catches and maps.
4. `ErrorKind` **retryable** classification (transport-agnostic).
5. Common exceptions in Core: `NotFoundException`, `ConflictException`, `NotAuthorizedException`.
6. Refactor `BatchCommandResult` so batch errors preserve the `ErrorKind` (not a bare string).

**Out of scope (do NOT build here — lives in the ASP.NET / consumer layer):**

- The HTTP `ErrorKind → status code` mapping, the `ResultFilter`, and ASP.NET `IExceptionHandler`.
- Endpoint/Dispatcher base classes (`CreateEntityEndpoint`, `BaseOneDispatcher`, …).
- Anything that references `Microsoft.AspNetCore.*`. **Pondhawk.Core stays transport-agnostic.**

## Current state (read these first)

- `src/Pondhawk.Core/Mediator/IMediator.cs` — `Task<TResponse> SendAsync<TResponse>(IRequest<TResponse>, CancellationToken)`. No catch.
- `src/Pondhawk.Core/Mediator/Mediator.cs` — resolves handler, runs pipeline behaviors. No try/catch today.
- `src/Pondhawk.Core/Mediator/IRequestHandler.cs`, `IPipelineBehavior.cs` — handlers/behaviors return `Task<TResponse>`. **These do NOT change** (see note below).
- `src/Pondhawk.Core/Mediator/BatchCommandResult.cs` — existing envelope, but errors are a bare
  `ErrorMessage` string with **no kind**. This is the lossy thing we are fixing.
- `src/Pondhawk.Core/Exceptions/` — `ExternalException` (abstract; has `Kind`, `ErrorCode`,
  `Explanation`, details), `FluentException<TDescendant>` (CRTP, `WithKind`/`WithErrorCode`/…),
  `FunctionalException` (Kind=Functional), `FailedValidationException` (Kind=Predicate, carries
  violation `EventDetail`s), `ErrorKind` enum, `EventDetail`. Namespace: `Pondhawk.Exceptions`.
- `Pondhawk.Rules.EFCore` throws `EntityValidationException` with structured violations —
  **verify** its hierarchy (see step 3 requirement).

### Key insight: only the mediator's outer contract changes

`IRequestHandler<TRequest,TResponse>.HandleAsync` and `IPipelineBehavior` keep returning
`Task<TResponse>`. Handlers keep returning a plain `TResponse` and keep **throwing** on error. The
**only** change is that `Mediator.SendAsync` wraps the final `TResponse` in `Response<TResponse>`
and catches exceptions into `Response.Failure`. Keep the change localized to that one method.

## Decision: Y1a (breaking, uniform) — recommended

Change `SendAsync` to return `Response<TResponse>`. Internal blast radius is **3 test call sites**
(`grep -rn SendAsync test`); there are **zero** `src` call sites. So make the clean break now.

> Fallback (Y1b) only if external consumers already depend on the throwing `SendAsync` and cannot
> migrate: keep `SendAsync` throwing and add `Task<Response<TResponse>> InvokeAsync<TResponse>(...)`
> alongside it. Default to **Y1a** unless told otherwise.

## What to build

### 1. `ErrorInfo` (namespace `Pondhawk.Exceptions`)

One transport-agnostic error shape, shared by exceptions and the envelope so they can't drift.

```csharp
public sealed record ErrorInfo
{
    public required ErrorKind Kind { get; init; }
    public required string ErrorCode { get; init; }      // stable, machine-readable
    public required string Explanation { get; init; }    // human-readable
    public IReadOnlyList<EventDetail> Details { get; init; } = [];

    public static ErrorInfo From(ExternalException ex);  // copy Kind/ErrorCode/Explanation/Details
    public static ErrorInfo System(Exception ex);        // Kind=System, ErrorCode="System", Explanation=ex.Message
}
```

### 2. `Response<T>` (namespace `Pondhawk.Mediator`)

```csharp
public readonly record struct Response<T>
{
    public bool Ok { get; }            // true => success
    public T? Value { get; }
    public ErrorInfo? Error { get; }   // non-null iff !Ok

    public static Response<T> Success(T value);
    public static Response<T> Failure(ErrorInfo error);
}
```

Provide whatever small ergonomics fit the repo style (e.g. `Match`/`GetValueOrThrow`), but keep it
minimal. The envelope is **internal to the process** — consumers (ASP.NET ResultFilter, queue
consumer) adapt it; it is not itself a wire contract.

### 3. Enveloping dispatch in `Mediator.SendAsync`

```csharp
public async Task<Response<TResponse>> SendAsync<TResponse>(
    IRequest<TResponse> request, CancellationToken ct = default)
{
    try
    {
        var value = await /* existing handler + pipeline invocation */;
        return Response<TResponse>.Success(value);
    }
    catch (OperationCanceledException) { throw; }              // never swallow cancellation
    catch (ExternalException ex)                               // expected, app-level
    {
        LogByKind(ex);                                         // 4xx-ish kinds Info/Warn
        return Response<TResponse>.Failure(ErrorInfo.From(ex));
    }
    catch (Exception ex)                                       // unexpected = bug
    {
        _logger.LogError(ex, "Unhandled error dispatching {Request}", typeof(TResponse).Name);
        return Response<TResponse>.Failure(ErrorInfo.System(ex));
    }
}
```

Requirements:

- **Discriminate.** `ExternalException` → mapped onto the envelope. Unexpected exceptions →
  `ErrorInfo.System(ex)` **and logged at `Error`** (the bug must stay loud — do not silently
  launder it). `OperationCanceledException` rethrows.
- **Log by kind.** Inject `ILogger<Mediator>` (Microsoft.Extensions.Logging.Abstractions). Map the
  4xx-family kinds (`Predicate`, `BadRequest`, `NotFound`, `NotAuthorized`,
  `AuthenticationRequired`, `Conflict`, `Concurrency`, `Functional`, `NotImplemented`) to
  `Information`/`Warning`; the 5xx family (`System`, `Remote`, `Unknown`) to `Error`. (Pick the
  Info-vs-Warn split sensibly; the point is expected outcomes are not logged as errors.)
- **Validation must map correctly.** Verify `EntityValidationException` and
  `FailedValidationException` derive from `ExternalException` with `Kind = Predicate` and expose
  their violations as `EventDetail`s, so `ErrorInfo.From` carries the violations into
  `Details`. If `EntityValidationException` does **not** derive from `ExternalException`, add an
  explicit catch that maps it to a `Predicate` `ErrorInfo` with the violations.

### 4. `ErrorKind` retryable classification (namespace `Pondhawk.Exceptions`)

A single canonical default so queue consumers route identically.

```csharp
public static class ErrorKindPolicy
{
    // Transient = worth retrying / requeue; everything else = permanent / dead-letter.
    public static bool IsTransient(this ErrorKind kind) =>
        kind is ErrorKind.System or ErrorKind.Remote or ErrorKind.Concurrency;
}
```

(Consumers may override policy, but this is the default. Note: the HTTP status mapping is
deliberately NOT here — it belongs to the ASP.NET layer.)

### 5. Common exceptions (namespace `Pondhawk.Exceptions`)

Thin `FluentException<T>` subclasses whose constructor fixes the `Kind`, a standard `ErrorCode`,
and a message template. These are sugar over `WithKind(...)`; **mapping is always keyed on `Kind`,
never on the concrete type.**

```csharp
public sealed class NotFoundException : FluentException<NotFoundException>      // Kind=NotFound
{ public NotFoundException(string resource, object key) : base($"{resource} '{key}' was not found.") { WithKind(ErrorKind.NotFound); /* ErrorCode */ } }

public sealed class ConflictException : FluentException<ConflictException>      // Kind=Conflict
public sealed class NotAuthorizedException : FluentException<NotAuthorizedException> // Kind=NotAuthorized
```

Do **not** add a `NotImplementedException` (collides with `System.NotImplementedException`; a stray
`using` would throw the BCL type, which isn't an `ExternalException`). Use the base + `WithKind`
for that case.

### 6. `BatchCommandResult` keeps the kind

Refactor so a failed batch command carries an `ErrorInfo` (with `ErrorKind`), not a bare string.
Build batch results from `Response<T>`:

- success `Response` → `BatchCommandResult.Succeeded(value, …)`
- failure `Response` → `BatchCommandResult.Failed(commandType, entityUid, error: ErrorInfo)`

Keep a string `ErrorMessage` if convenient for back-compat, but derive it from
`ErrorInfo.Explanation`; the authoritative field is the `ErrorInfo`/`Kind`. Verify
`BatchExecutionContext` routes through this so kinds survive a batch.

## Acceptance criteria

- `dotnet build` and `dotnet test` green for `pondhawk-tools.slnx`.
- New tests cover:
  - success → `Response.Ok == true`, `Value` set.
  - thrown `NotFoundException` → `Response.Ok == false`, `Error.Kind == NotFound`, code/message set.
  - validation failure → `Error.Kind == Predicate` with violations in `Error.Details`.
  - unexpected exception (e.g. `InvalidOperationException`) → `Error.Kind == System`, **and** an
    `Error`-level log is emitted.
  - `OperationCanceledException` propagates (not enveloped).
  - `ErrorKind.IsTransient` table: `System`/`Remote`/`Concurrency` true, others false.
  - common exceptions set the correct `Kind`.
  - a batch of mixed success/failure preserves each command's `Kind`.
- Update the 3 existing `SendAsync` test call sites to the new return type.
- XML doc comments on all new public members; keep the existing MIT license header style.

## Conventions

Follow `pondhawk-tools/CLAUDE.md` and the surrounding code style. Central package management via
`Directory.Packages.props`. No new HTTP/ASP.NET dependencies in `Pondhawk.Core`. Update
`llms.txt`/package READMEs if they document the mediator's signature.
```
