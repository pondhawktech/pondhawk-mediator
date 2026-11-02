# Pondhawk.Mediator

Lightweight CQRS-style request/response mediator with pipeline behaviors and a `Response<T>` envelope. Error types come from [`Pondhawk.Exceptions`](https://www.nuget.org/packages/Pondhawk.Exceptions).

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
    public Task<int> HandleAsync(CreateOrderCommand request, CancellationToken cancellationToken = default)
    {
        // ... create order, return ID
        return Task.FromResult(42);
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
exception is logged at `Error` and enveloped as `ErrorKind.System` with a generic `Explanation`.
`OperationCanceledException` propagates only when the caller's token is cancelled; any other
cancellation, such as an `HttpClient` timeout, is enveloped as `System`. Configuration errors (no
handler registered) still propagate. This lets non-HTTP callers — queue consumers and batch — branch
on the outcome without catching:

```csharp
var response = await mediator.SendAsync(command);
response.Match(
    onSuccess: value => Ack(value),
    onFailure: error => error.Kind.IsTransient() ? Requeue() : DeadLetter(error));
```

`ErrorKindPolicy.IsTransient(this ErrorKind)` is the canonical retry-vs-dead-letter default
(`System`, `Remote`, `Concurrency` are transient). Set a retry limit in the consumer: an unexpected
exception is also `System`, so a bug that fails every time is requeued like an infrastructure fault.
The HTTP status mapping deliberately lives in the ASP.NET layer, not here — this package stays
transport-agnostic.

### Pipeline Behaviors

Wrap handler execution for cross-cutting concerns:

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
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

### Upgrade from 1.x to 2.0

- **Handlers:** `AddMediator` throws when two different handler types serve one request, including a concrete handler and a concrete class derived from it. Make the base class abstract, or replace the handler with a direct DI registration after `AddMediator`. A direct registration before `AddMediator` also conflicts.
- **Open generic handlers:** the scan skips them. Register closed forms by hand.
- **Behaviors:** `AddPipelineBehavior` throws `ArgumentException` at the call unless the type is a concrete open generic that implements `IPipelineBehavior<,>` with its own type parameters, in order.
- **Requests:** a type that implements `IRequest<>` twice throws `InvalidOperationException` when sent.
- **Cancellation:** catch `OperationCanceledException` only for the caller's own token. Other cancellations now arrive as `ErrorKind.System`, which is transient by default.
- **`ErrorInfo.System`:** `Explanation` is fixed text. Read the real message from the log.
- **`ExternalException(IExceptionInfo)`:** it now copies `ErrorCode` and leaves `InnerExplanation` empty.
- **`BatchCommandResult.Succeeded`:** pass the command type first: `Succeeded(commandType, response, entityUid)`. Check calls with a `string` response and an entity UID: the old form `Succeeded(text, uid)` still compiles, with the wrong meaning.

