# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Restore, build, and test via the Cake build script
dotnet run --project build/Build.csproj -- --target=Test

# Run tests directly
dotnet test pondhawk-mediator.slnx

# Pack NuGet packages (writes to ./artifacts)
dotnet run --project build/Build.csproj -- --target=Pack --build-number=<n>
```

## Project Setup

- `net10.0`, nullable enabled in `src`, central package management, `TreatWarningsAsErrors` and Meziantou analyzer in `src`.
- Each project's `version.json` holds `major.minor`; the Cake `Pack` target appends the build number.
- `build.yml` pushes dev packages to the `pondhawktech` GitHub Packages feed. `publish.yml` is a manual promotion to nuget.org with Trusted Publishing.

## Architecture

`Pondhawk.Mediator` → `Pondhawk.Exceptions`. `Pondhawk.Exceptions` must never depend on the mediator: applications use it alone.

- The mediator is request/response only, with exactly one handler per request. There is deliberately no notification or publish channel.
- `Mediator.SendAsync` is the single seam that converts a thrown error to a `Response<T>` failure. `ExternalException` keeps its `ErrorKind`; unexpected exceptions become `ErrorKind.System`. `OperationCanceledException` and missing-handler errors still throw.
- `default(Response<T>)` reads as a `System` failure, never a success with a null value.
- There is deliberately no non-generic `Response`. A payload-less mutation returns `Response<Receipt>`.
- The `ErrorKind` to HTTP status map deliberately stays out of this repo: it belongs in the ASP.NET layer.
