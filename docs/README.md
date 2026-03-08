# platform-net-logging



## Key points on log formatting

Since the restrictions imposed on logging are those imposed by the ILogger interface (ie very few), some soft concepts are worth noting when using this library.

### Punctuation

Don't end log statements with a dot because often log statements end with a value where dot may carry a significance, eg a subject

Preffered

```csharp
"Some log statement with a {Value}" => "Some log statement with a value.with.dots"
```

Undesirable

```csharp
"Some log statement with a {Value}." => "Some log statement with a value.with.dots."
```

### Structured logging

Use structured logging instead of string interpolation to allow the logger to differentiate between the log statement and the actual values that back it. This allows the sink to make the value available as a property in the serialized log message that can easily be queried. The structured value should be PascalCase, and without dots, eg `{Value}`.

Preffered

```csharp
logger.LogDebug("Some log statement with a {Value}", variable.Value) => "Some log statement with a "value"
```

Undesirable

```csharp
logger.LogDebug($"Some log statement with a {variable.Value}") => "Some log statement with a value"
```

### Canonical logging

Many metrics can also be sourced from logs, and in some cases this is preferred. Primarily the prometheus histograms generate alot of data points when coupled with labels, if you're currently using a histogram with labels you should contemplate converting your histogram into a canonical log statement instead. The best examples of such use cases are logging request/response data with route data, response code, request size and request latency.

The canonical log statement combines a result set of metrics into a single log line, output at the end of a request or operation. Keep your keywords short and succinct.

```csharp
using (_logger.BeginScope(new Dictionary<string, object>
{
    {"Ct", LogType},
    {"Cs", context.EventBusMessage.Subject},
    {"Cet", context.EventBusMessage.Type},
    {"Ces", context.EventBusMessage.GetEventContext().Source},
    {"Cd", duration.ElapsedMilliseconds}
}))
{
    _logger.LogInformation("ClReq");
}
```

> **NOTE** If your logging request data for an API gateway route, please note that request data can also be sourced from existing gateway logs.

Querying the canonical logs for the request above then becomes as easy as

```sumo
(_collector=dev-k8s-*
AND _sourceCategory="kubernetes/platform/core/platform/core/idp")
| json field=_raw "log.@m" as LogMessage | where %LogMessage = "ClReq"
| json field=_raw "log.@l" as LogLevel | where %LogLevel = "Information"
| json field=_raw "log.cs" as Subject
| json field=_raw "log.cet" as EventType
| json field=_raw "log.ces" as EventSource
| json field=_raw "log.cd" as DurationMs
```

### Logging from libraries

Logging from libraries can be problematic, with a few exceptions, *especially* as it relates to error management. The user of the library should be given the opportunity to decide what is an error, and how that error should be logged in the context of that service (if at all).

Errors affect the error rate of a service, and in the end triggers any configured alerts for that service along with any associated call out policies. While logs from libraries can be excluded using SourceContextOverrides, the process of identifying what sources to override is through trial and error and the user probably won't notice it until they actually hit a real issue.

## Installation and configuration

### Servicehost based applications



```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServiceHost(sb =>
            {
                sb.AddService<ServiceImplementation>(o =>
                {
                    o.Name = "ServiceImplementation";
                    o.Group = "ServiceGroup";
                });
            })
            // Configure logging
            .ConfigureServiceHostLogging("Log")
            .ConfigureWebHost(builder =>
            {
                builder.UseKestrel();
                builder.UseStartup<Startup>();
                builder.Configure(configure =>
                {
                    configure.UseServiceHost();
                });
            });

        return host;
    }
}
```

You can either configure the builder manually, but the recommended way is to pass a configuration key matching the following json structure:

```json
  "Log": {
    "LogLevel": "Information",
    "Format": "CompactJson",
    "EnableSourceContext": true,
    "SourceContextOverrides": {
      "Microsoft": "Warning",
      "System": "Error"
    }
  }
```

You're recommended to at least override the source contexts for Microsoft and System like in the example above to avoid outputting unnecessary large amounts of unrelated logs, such MVC request logs.

### Non-servicehost based applications


If your application has considerable investment in another logging framework or is not compatible with the platform libraries, you can instead if possible ensure that your framework logs in the same format as this library.

## ILogger

Services should **always** log using *Microsoft.Extensions.Logging.ILogger*. This library automatically configures the underlying sinks and formatters offering a zero-configuration logging experience.

## Logging contextual information

When logging you should **always** try and provide the userid and tenantid if they are available as part of the *IEventBusMessage UserContext*. Using *ILogger* this is most easily done by creating a logging scope.

```csharp
using (_logger.BeginScope(new Dictionary<string, string>
{
    [Constants.Fields.TenantId] = message.GetUserContext()?.TenantId,
    [Constants.Fields.UserId] = message.GetUserContext()?.UserId
}))
{
    // We try/catch since any exceptions would otherwise dispose of the surrounding logger scope
    try {
        _logger.LogInformation("This logging statement will have the tenantId and userId fields appended to it");
        // business logic

    }
    catch (Exception e) {
        _logger.LogError(e, "This logging statement will also have the tenantId and userId fields appended to it");
    }
}
```
