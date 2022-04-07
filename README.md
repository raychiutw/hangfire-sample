# hangfire-sample

## Why to Use Hangfire

### Queue-based 執行

```c#
BackgroundJob.Enqueue(() => Console.WriteLine("Hello, world!"));
```

### 延遲執行

```c#
BackgroundJob.Schedule(() => Console.WriteLine("Hello, world!"), TimeSpan.FromMinutes(5));
```

### 重複執行(排程)

```c#
RecurringJob.AddOrUpdate("easyjob", () => Console.Write("Easy!"), Cron.Daily);
```

CRON expressions:

```c#
RecurringJob.AddOrUpdate("powerfuljob", () => Console.Write("Powerful!"), "0 12 * */2");
```

### SQL Server and Redis 持久資料資源

官方支援

- SQL Server provides simplified installation together with usual maintenance plans.
- Redis provides awesome speed, especially comparing to SQL Server, but requires additional knowledge.

第三方支援

- MySql, PostgreSql, SQLite ...
- Memory ...

### 自動 Retry

```c#
[AutomaticRetry(Attempts = 100)]
public static void GenerateStatistics() { }

BackgroundJob.Enqueue(() => GenerateStatistics());
```

### 保證執行

Hangfire was made with the knowledge that the hosting environment can kill all the threads on each line. So, it does not remove the job until it is successfully completed and contains different implicit retry logic to do the job when its processing was aborted.

### 呼叫類別或介面執行

```c#
public class EmailService
{
    public void Send() { }
}

BackgroundJob.Enqueue<EmailService>(x => x.Send());
```

When a worker sees that the given method is an instance-method, it will activate its class first. By default, the `Activator.CreateInstance` method is used, so only classes with default constructors are supported by default. But you can plug in your IoC container and pass the dependencies through the constructor.

### 多語系捕捉

When you marshal your method invocation into another execution context, you should be able to preserve some environment settings. Some of them – `Thread.CurrentCulture` and `Thread.CurrentUICulture` are automatically captured for you.

It is done by the `PreserveCultureAttribute` class that is applied to all of your methods by default.

### Cancellation Tokens

Hangfire can tell your methods were aborted or canceled due to shutdown event, so you can stop them gracefully using job cancellation tokens that are similar to the regular CancellationToken class.

```c#
public void Method(CancellationToken token)
{
    for (var i = 0; i < Int32.MaxValue; i++)
    {
        token.ThrowIfCancellationRequested();
        Thread.Sleep(1000);
    }
}
```

### IoC Containers 支援 DI

- Hangfire.Ninject
- Hangfire.Autofac

### Logging

Hangfire uses the Common.Logging library to log all its events

### Web Garden- and Web Farm-friendly (多台 Web 執行)

You can run multiple Hangfire instances, either on the same or different machines. It uses distributed locking to prevent race conditions. Each Hangfire instance is redundant, and you can add or remove instances seamlessly (but control the queues they listen).

### 多 Queue 執行

Hangfire can process multiple queues. If you want to prioritize your jobs or split the processing across your servers (some processes the archive queue, others – the images queue, etc), you can tell Hangfire about your decisions.

To place a job into a different queue, use the QueueAttribute class on your method:

```c#
[Queue("critical")]
public void SomeMethod() { }

BackgroundJob.Enqueue(() => SomeMethod());
```

To start to process multiple queues, you need to update your OWIN bootstrapper’s configuration action:

```c#
services.AddHangfireServer(options => options.Queues = new [] { "critical", "default" });
```

The order is important, workers will fetch jobs from the critical queue first, and then from the default queue.

### 即時的設定控制

Hangfire uses its own fixed worker thread pool to consume queued jobs. Default worker count is set to Environment.ProcessorCount * 5. This number is optimized both for CPU-intensive and I/O intensive tasks. If you experience excessive waits or context switches, you can configure the amount of workers manually:

```c#
services.AddHangfireServer(options => options.WorkerCount = 100);
```

### 跨環境執行 Job

By default, the job processing is made within an ASP.NET application. But you can process jobs either in a console application, Windows Service, or anywhere else.

### Extensibility (可擴展性)

Hangfire is built to be as generic as possible. You can extend the following parts:

- storage implementation;
- states subsystem (including the creation of new states);
- job creation process;
- job performance process;
- state changing process;
- job activation process.

Some of core components are made as extensions: QueueAttribute, PreserveCultureAttribute, AutomaticRetryAttribute, SqlServerStorage, RedisStorage, NinjectJobActivator, AutofacJobActivator, ScheduledState.

Did you know that you can edit this page on GitHub and send a Pull Request?

## How to Use Hangfire

### 安裝套件

```xml
    <PackageReference Include="Hangfire.AspNetCore" Version="1.7.28" />
    <PackageReference Include="Hangfire.Console" Version="1.4.2" />
    <PackageReference Include="Hangfire.Console.Extensions" Version="1.0.5" />
    <PackageReference Include="Hangfire.MemoryStorage" Version="1.7.0" />
    <PackageReference Include="NLog.Web.AspNetCore" Version="4.14.0" />
```    

### 注入設定

此範例使用記憶體

Program.cs

```c#
builder.Services.AddHangfire(config =>
    config.UseMemoryStorage()
        .WithJobExpirationTimeout(TimeSpan.FromDays(3))
        .UseConsole()
        .UseNLogLogProvider());
        
 builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "default" };
    options.WorkerCount = 40;
});

builder.Services.AddScoped<IHangfireJobTrigger, HangfireJobTrigger>();
builder.Services.AddScoped<ISystemJob, SystemJob>();

// Hangfire Console 與 Commom Logger 整合
builder.Services.AddHangfireConsoleExtensions();


app.UseHangfireDashboard(
    "/hangfire",
    new DashboardOptions
    {
        IgnoreAntiforgeryToken = true
    });

using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;

    var hangfireJobTrigger = services.GetRequiredService<IHangfireJobTrigger>();
    hangfireJobTrigger.OnStart();
}

GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
{
    Attempts = 3
});

```

### 要執行的 Job 實作

ISystemJob.cs

```c#
    /// <summary>
    /// System Job 介面
    /// </summary>
    [AutomaticRetry(Attempts = 0)]
    public interface ISystemJob
    {
        /// <summary>
        /// 移除超過 N 日的 HttpLog
        /// </summary>
        /// <returns></returns>
        [JobDisplayName("移除超過 {1} 日的 HttpLog")]
        Task RemoveHttpLogAsync(PerformContext? context, int day);
    }
```

SystemJob.cs

```c#
    /// <summary>
    /// System Job
    /// </summary>
    /// <seealso cref="ISystemJob"/>
    public class SystemJob : ISystemJob
    {
        private readonly ILogger<SystemJob> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemJob"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public SystemJob(ILogger<SystemJob> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 移除超過 N 日的 AutoHistory
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task RemoveHttpLogAsync(PerformContext? context, int day)
        {
            _logger.LogInformation($"Job Id: {context.BackgroundJob.Id}");
            var dateTime = DateTime.Now;
            _logger.LogInformation($"開始: {dateTime}");

            // 實作
            var removeCount = day;

            _logger.LogInformation($"刪除筆數: {removeCount}");
            _logger.LogInformation($"結束: {DateTime.Now}");
        }
    }
```

### 單次 Job

```c#
_jobClient.Enqueue<ISystemJob>(x => x.RemoveHttpLogAsync(null, 1));
```

### 排程 Job

IHangfireJobTrigger.cs

```c#
    /// <summary>
    /// interface IHangfireJobTrigger.
    /// </summary>
    public interface IHangfireJobTrigger
    {
        /// <summary>
        /// Called when [start].
        /// </summary>
        Task OnStart();
    }
```

HangfireJobTrigger.cs

```c#
    /// <summary>
    /// Hangfire JobTrigger.
    /// </summary>
    /// <seealso cref="IHangfireJobTrigger"/>
    public class HangfireJobTrigger : IHangfireJobTrigger
    {
        /// <summary>
        /// Called when [start].
        /// </summary>
        public async Task OnStart()
        {
            // 清除已有的排程工作
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
            }

            RecurringJob.AddOrUpdate<ISystemJob>
            (
                s => s.RemoveHttpLogAsync(null, 10),
                "0 3 * * *",
                TimeZoneInfo.Local
            );
        }
    }
```

### 使用 SQLite

```c#
// SQLite
builder.Services.AddHangfire(config =>
    config.UseSQLiteStorage()
        .WithJobExpirationTimeout(TimeSpan.FromDays(3))
        .UseConsole()
        .UseNLogLogProvider());
```

### 使用 SQLServer

```c#
//SQL Server
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(
            builder.Configuration.GetConnectionString("SQLServer"),
            new SqlServerStorageOptions
            {
                SchemaName = "PMPTransaction",
                JobExpirationCheckInterval = TimeSpan.FromMinutes(60),
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                //SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true // Migration to Schema 7 is required
            })
        .WithJobExpirationTimeout(TimeSpan.FromDays(3))
        .UseConsole()
        .UseNLogLogProvider());
```

### 使用 Redis

```c#
//Redis
builder.Services.AddHangfire(config =>

   config.UseRedisStorage(
           builder.Configuration.GetConnectionString("Redis"),
            new RedisStorageOptions
            {
                Prefix = "PmpTransaction"
            })
        .WithJobExpirationTimeout(TimeSpan.FromDays(7))
        .UseConsole()
        .UseNLogLogProvider());
```
