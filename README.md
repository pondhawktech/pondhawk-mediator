<p align="center">
  <img src="pht-small-logo.png" alt="Pondhawk.Mediator" width="120" />
</p>

<h1 align="center">Pondhawk.Mediator</h1>

<p align="center">
  A lightweight request/response mediator and a transport-agnostic error model for .NET.
</p>

| Package | Purpose |
|---|---|
| [`Pondhawk.Mediator`](src/Pondhawk.Mediator/README.md) | Mediator, pipeline behaviors, `Response<T>`, `Receipt`, batch types. Depends on `Pondhawk.Exceptions`. |
| [`Pondhawk.Exceptions`](src/Pondhawk.Exceptions/README.md) | `ErrorInfo`, `ErrorKind`, exception types, `ProblemDetail`. Usable alone. |

Both target `net10.0`.

## Migrate from Pondhawk.Core

These packages replace the mediator and exception types in `Pondhawk.Core`. The namespaces do not change.

1. Replace the `Pondhawk.Core` package reference with `Pondhawk.Mediator`, `Pondhawk.Exceptions`, or both.
2. Remove any use of `Error`, `NotFoundError`, `NotValidError` and `UnhandledError`. These types are gone.

Do not reference `Pondhawk.Core` in the same project: the duplicate types cause CS0433.

## Build

```bash
dotnet run --project build/Build.csproj -- --target=Test
```

Extracted from [pondhawktech/tools](https://github.com/pondhawktech/pondhawk-utilities), now `pondhawk-utilities`. Design record: [docs/mediator-response-envelope.md](docs/mediator-response-envelope.md).
