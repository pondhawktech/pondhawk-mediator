# Pondhawk.Mediator

Lightweight CQRS-style request/response mediator with pipeline behaviors and a `Response<T>` envelope. Error types come from [`Pondhawk.Exceptions`](https://www.nuget.org/packages/Pondhawk.Exceptions).

CQRS-style request/response dispatch with pipeline behaviors for cross-cutting concerns.

```csharp
using Pondhawk.Mediator;

// Register mediator and auto-discover handlers from assemblies
services.AddMediator(typeof(CreateOrderHandler).Assembly);

// Register open-generic pipeline behaviors (logging, validation, etc.)
services.AddPipelineBehavior(typeof(LoggingBehavior<,>));

// Define a command
public record CreateOrderCommand(string Customer, decimal Total) : ICommand<int>;

// Define a handler
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, int>
{
    public async Task<int> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // ... create order, return ID
    }
}

// Send through the mediator — returns a Response<T> envelope, never throws on app errors
Response<int> result = await mediator.SendAsync(new CreateOrderCommand("Acme", 500m));

if (result.Ok)
    Console.WriteLine($"Created order {result.Value}");
else
    Console.WriteLine($"Failed ({result.Error!.Kind}): {result.Error.Explanation}");
```

### The `Response<T>` envelope

Handlers still **throw** on error — nothing about authoring changes. The mediator is the single
seam that converts a throw into a `Response<T>`: a thrown `ExternalException` becomes a
`Response.Failure` carrying a structured `ErrorInfo` (with the error `Kind`), while an unexpected
exception is logged at `Error` and enveloped as `ErrorKind.System`. `OperationCanceledException`
and configuration errors (no handler registered) still propagate. This lets non-HTTP callers —
queue consumers and batch — branch on the outcome without catching:

```csharp
var response = await mediator.SendAsync(command);
response.Match(
    onSuccess: value => Ack(value),
    onFailure: error => error.Kind.IsTransient() ? Requeue() : DeadLetter(error));
```

`ErrorKindPolicy.IsTransient(this ErrorKind)` is the canonical retry-vs-dead-letter default
(`System`, `Remote`, `Concurrency` are transient). The HTTP status mapping deliberately lives in
the ASP.NET layer, not here — this package stays transport-agnostic.

### Pipeline Behaviors

Wrap handler execution for cross-cutting concerns:

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next)
    {
        Log.Information("Handling {Request}", typeof(TRequest).Name);
        var response = await next();
        Log.Information("Handled {Request}", typeof(TRequest).Name);
        return response;
    }
}
```

### Batch Support

Track batch operations with `AsyncLocal`-based context:

```csharp
using var scope = BatchExecutionContext.BeginBatch("import-2024");
// All mediator calls within this scope share the batch context
```

