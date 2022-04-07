# hangfire-sample

## Why Hangfire

### Queue-based Processing

```c#
BackgroundJob.Enqueue(() => Console.WriteLine("Hello, world!"));
```

### Delayed Method Invocation

```c#
BackgroundJob.Schedule(() => Console.WriteLine("Hello, world!"), TimeSpan.FromMinutes(5));
```

### Recurring Tasks

```c#
RecurringJob.AddOrUpdate("easyjob", () => Console.Write("Easy!"), Cron.Daily);
```

CRON expressions:

```c#
RecurringJob.AddOrUpdate("powerfuljob", () => Console.Write("Powerful!"), "0 12 * */2");
```

### SQL Server and Redis Support

- SQL Server provides simplified installation together with usual maintenance plans.
- Redis provides awesome speed, especially comparing to SQL Server, but requires additional knowledge.

### Automatic Retries

```c#
[AutomaticRetry(Attempts = 100)]
public static void GenerateStatistics() { }

BackgroundJob.Enqueue(() => GenerateStatistics());
```

### Guaranteed Processing
Hangfire was made with the knowledge that the hosting environment can kill all the threads on each line. So, it does not remove the job until it is successfully completed and contains different implicit retry logic to do the job when its processing was aborted.

### Instance Method Calls
All the examples above uses static method invocation, but instance methods are supported as well:

```c#
public class EmailService
{
    public void Send() { }
}

BackgroundJob.Enqueue<EmailService>(x => x.Send());
```

When a worker sees that the given method is an instance-method, it will activate its class first. By default, the Activator.CreateInstance method is used, so only classes with default constructors are supported by default. But you can plug in your IoC container and pass the dependencies through the constructor.

### Culture Capturing

### Cancellation Tokens
Hangfire can tell your methods were aborted or canceled due to shutdown event, so you can stop them gracefully using job cancellation tokens that are similar to the regular CancellationToken class.

```
public void Method(CancellationToken token)
{
    for (var i = 0; i < Int32.MaxValue; i++)
    {
        token.ThrowIfCancellationRequested();
        Thread.Sleep(1000);
    }
}
```

### IoC Containers

### Logging

### Web Garden- and Web Farm-friendly

### Multiple Queue Processing
Hangfire can process multiple queues. If you want to prioritize your jobs or split the processing across your servers (some processes the archive queue, others – the images queue, etc), you can tell Hangfire about your decisions.

To place a job into a different queue, use the QueueAttribute class on your method:

```c#
[Queue("critical")]
public void SomeMethod() { }

BackgroundJob.Enqueue(() => SomeMethod());
```

To start to process multiple queues, you need to update your OWIN bootstrapper’s configuration action:

services.AddHangfireServer(options => options.Queues = new [] { "critical", "default" });
The order is important, workers will fetch jobs from the critical queue first, and then from the default queue.

Concurrency Level Control
Hangfire uses its own fixed worker thread pool to consume queued jobs. Default worker count is set to Environment.ProcessorCount * 5. This number is optimized both for CPU-intensive and I/O intensive tasks. If you experience excessive waits or context switches, you can configure the amount of workers manually:

services.AddHangfireServer(options => options.WorkerCount = 100);
Process Jobs Anywhere
By default, the job processing is made within an ASP.NET application. But you can process jobs either in a console application, Windows Service, or anywhere else.

### Extensibility
Hangfire is built to be as generic as possible. You can extend the following parts:

### storage implementation;
states subsystem (including the creation of new states);
job creation process;
job performance process;
state changing process;
job activation process.
Some of core components are made as extensions: QueueAttribute, PreserveCultureAttribute, AutomaticRetryAttribute, SqlServerStorage, RedisStorage, NinjectJobActivator, AutofacJobActivator, ScheduledState.

Did you know that you can edit this page on GitHub and send a Pull Request?

### 安裝套件

```xml
    <PackageReference Include="Hangfire.AspNetCore" Version="1.7.28" />
    <PackageReference Include="Hangfire.Console" Version="1.4.2" />
    <PackageReference Include="Hangfire.Console.Extensions" Version="1.0.5" />
    <PackageReference Include="Hangfire.MemoryStorage" Version="1.7.0" />
    <PackageReference Include="Hangfire.Redis.StackExchange" Version="1.8.5" />
    <PackageReference Include="Hangfire.SqlServer" Version="1.7.28" />
    <PackageReference Include="Hangfire.Storage.SQLite" Version="0.3.1" />
    <PackageReference Include="NLog.Web.AspNetCore" Version="4.14.0" />
```    

### 注入設定

```csharp
builder.Services.AddHangfire(config =>
    config.UseMemoryStorage()
        .WithJobExpirationTimeout(TimeSpan.FromDays(3))
        .UseConsole()
        .UseNLogLogProvider());
```
