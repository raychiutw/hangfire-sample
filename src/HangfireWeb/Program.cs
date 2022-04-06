using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Hangfire.MemoryStorage;
using HangfireWeb;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(config =>
    config.UseMemoryStorage()
        .WithJobExpirationTimeout(TimeSpan.FromDays(3))
        .UseConsole()
        .UseNLogLogProvider());

//builder.Services.AddHangfire(config =>
//    config.UseSQLiteStorage()
//        .WithJobExpirationTimeout(TimeSpan.FromDays(3))
//        .UseConsole()
//        .UseNLogLogProvider());

// SQL Server
//builder.Services.AddHangfire(config =>
//    config.UseSqlServerStorage(
//            builder.Configuration.GetConnectionString("SQLServer"),
//            new SqlServerStorageOptions
//            {
//                SchemaName = "PMPTransaction",
//                JobExpirationCheckInterval = TimeSpan.FromMinutes(60),
//                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
//                //SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
//                QueuePollInterval = TimeSpan.Zero,
//                UseRecommendedIsolationLevel = true,
//                DisableGlobalLocks = true // Migration to Schema 7 is required
//            })
//        .WithJobExpirationTimeout(TimeSpan.FromDays(3))
//        .UseConsole()
//        .UseNLogLogProvider());

// Redis
//builder.Services.AddHangfire(config =>
//    config.UseRedisStorage(
//            builder.Configuration.GetConnectionString("Redis"),
//            new RedisStorageOptions
//            {
//                Prefix = "PmpTransaction"
//            })
//        .WithJobExpirationTimeout(TimeSpan.FromDays(7))
//        .UseConsole()
//        .UseNLogLogProvider());

builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "default" };
    options.WorkerCount = 40;
});

builder.Services.AddScoped<IHangfireJobTrigger, HangfireJobTrigger>();
builder.Services.AddScoped<ISystemJob, SystemJob>();

builder.Services.AddHangfireConsoleExtensions();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();