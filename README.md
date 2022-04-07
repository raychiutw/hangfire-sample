# hangfire-sample

## Why Hangfire

Queue-based Processing
```c#
BackgroundJob.Enqueue(() => Console.WriteLine("Hello, world!"));
```

Delayed Method Invocation
Instead of invoking a method right now, you can postpone its execution for a specified time:

BackgroundJob.Schedule(() => Console.WriteLine("Hello, world!"), TimeSpan.FromMinutes(5));
This call also saves a job, but instead of placing it to a queue, it adds the job to a persistent schedule. When the given time has elapsed, the job will be added to its queue. Meanwhile, you can restart your application – it will be executed anyway.

Recurring Tasks
Recurring job processing has never been easier. All you need is a single line of code:

RecurringJob.AddOrUpdate("easyjob", () => Console.Write("Easy!"), Cron.Daily);
Hangfire uses Cronos library to perform scheduling tasks, so you can use more complex CRON expressions:

RecurringJob.AddOrUpdate("powerfuljob", () => Console.Write("Powerful!"), "0 12 * */2");
SQL Server and Redis Support
Hangfire uses persistent storage to store jobs, queues and statistics and let them survive application restarts. The storage subsystem is abstracted enough to support both classic SQL Server and fast Redis.

SQL Server provides simplified installation together with usual maintenance plans.
Redis provides awesome speed, especially comparing to SQL Server, but requires additional knowledge.
Automatic Retries
If your method encounters a transient exception, don’t worry – it will be retried automatically in a few seconds. If all retry attempts are exhausted, you are able to restart it manually from integrated web interface.

You can also control the retry behavior with the AutomaticRetryAttribute class. Just apply it to your method to tell Hangfire the number of retry attempts:

[AutomaticRetry(Attempts = 100)]
public static void GenerateStatistics() { }

BackgroundJob.Enqueue(() => GenerateStatistics());
Guaranteed Processing
Hangfire was made with the knowledge that the hosting environment can kill all the threads on each line. So, it does not remove the job until it is successfully completed and contains different implicit retry logic to do the job when its processing was aborted.

Instance Method Calls
All the examples above uses static method invocation, but instance methods are supported as well:

public class EmailService
{
    public void Send() { }
}

BackgroundJob.Enqueue<EmailService>(x => x.Send());
When a worker sees that the given method is an instance-method, it will activate its class first. By default, the Activator.CreateInstance method is used, so only classes with default constructors are supported by default. But you can plug in your IoC container and pass the dependencies through the constructor.

Culture Capturing
When you marshal your method invocation into another execution context, you should be able to preserve some environment settings. Some of them – Thread.CurrentCulture and Thread.CurrentUICulture are automatically captured for you.

It is done by the PreserveCultureAttribute class that is applied to all of your methods by default.

Cancellation Tokens
Hangfire can tell your methods were aborted or canceled due to shutdown event, so you can stop them gracefully using job cancellation tokens that are similar to the regular CancellationToken class.

public void Method(CancellationToken token)
{
    for (var i = 0; i < Int32.MaxValue; i++)
    {
        token.ThrowIfCancellationRequested();
        Thread.Sleep(1000);
    }
}
IoC Containers
In case you want to improve the testability of your job classes or simply don’t want to use a huge amount of different factories, you should use instance methods instead of static ones. But you either need to somehow pass the dependencies into these methods and the default job activator does not support parameterful constructors.

Don’t worry, you can use your favourite IoC container that will instantiate your classes. There are two packages, Hangfire.Ninject and Hangfire.Autofac for their respective containers. If you are using another container, please, write it yourself (on a basis of the given packages) and contribute to Hangfire project.

Logging
Hangfire uses the Common.Logging library to log all its events. It is a generic library and you can plug it to your logging framework using adapters. Please, see the list of available adapters on NuGet Gallery.

Web Garden- and Web Farm-friendly
You can run multiple Hangfire instances, either on the same or different machines. It uses distributed locking to prevent race conditions. Each Hangfire instance is redundant, and you can add or remove instances seamlessly (but control the queues they listen).

Multiple Queue Processing
Hangfire can process multiple queues. If you want to prioritize your jobs or split the processing across your servers (some processes the archive queue, others – the images queue, etc), you can tell Hangfire about your decisions.

To place a job into a different queue, use the QueueAttribute class on your method:

[Queue("critical")]
public void SomeMethod() { }

BackgroundJob.Enqueue(() => SomeMethod());
To start to process multiple queues, you need to update your OWIN bootstrapper’s configuration action:

services.AddHangfireServer(options => options.Queues = new [] { "critical", "default" });
The order is important, workers will fetch jobs from the critical queue first, and then from the default queue.

Concurrency Level Control
Hangfire uses its own fixed worker thread pool to consume queued jobs. Default worker count is set to Environment.ProcessorCount * 5. This number is optimized both for CPU-intensive and I/O intensive tasks. If you experience excessive waits or context switches, you can configure the amount of workers manually:

services.AddHangfireServer(options => options.WorkerCount = 100);
Process Jobs Anywhere
By default, the job processing is made within an ASP.NET application. But you can process jobs either in a console application, Windows Service, or anywhere else.

Extensibility
Hangfire is built to be as generic as possible. You can extend the following parts:

storage implementation;
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
