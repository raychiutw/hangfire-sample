# hangfire-sample

## 安裝套件

    <PackageReference Include="Hangfire.AspNetCore" Version="1.7.28" />
    <PackageReference Include="Hangfire.Console" Version="1.4.2" />
    <PackageReference Include="Hangfire.Console.Extensions" Version="1.0.5" />
    <PackageReference Include="Hangfire.MemoryStorage" Version="1.7.0" />
    <PackageReference Include="Hangfire.Redis.StackExchange" Version="1.8.5" />
    <PackageReference Include="Hangfire.SqlServer" Version="1.7.28" />
    <PackageReference Include="Hangfire.Storage.SQLite" Version="0.3.1" />
    <PackageReference Include="NLog.Web.AspNetCore" Version="4.14.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.3.0" />
    
## 注入設定

```c#
builder.Services.AddHangfire(config =>
    config.UseMemoryStorage()
        .WithJobExpirationTimeout(TimeSpan.FromDays(3))
        .UseConsole()
        .UseNLogLogProvider());
```
