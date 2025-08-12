# Horarium

[![Unit Tests](https://github.com/LiHaoGit/MonScheduler/actions/workflows/unit-test.yml/badge.svg)](https://github.com/LiHaoGit/MonScheduler/actions/workflows/unit-test.yml)
[![Integration Tests](https://github.com/LiHaoGit/MonScheduler/actions/workflows/integration-tests.yml/badge.svg)](https://github.com/LiHaoGit/MonScheduler/actions/workflows/integration-tests.yml)
[![Nuget](https://img.shields.io/nuget/v/MonScheduler.svg)](https://www.nuget.org/packages/MonScheduler)

[English](README.md) | [中文](README_zh.md)

Horarium 是一个开源的 .NET 作业调度库，它拥有易于使用的 API，可以集成到任何规模的应用程序中——从最小的独立应用程序到最大的电子商务系统。

Horarium 完全基于异步工作模型，它允许您在单个应用程序实例中并行运行数百个作业。它支持在分布式系统中执行作业，并使用 MongoDB 作为同步后端。

Horarium 支持 .NET Core 9 及更高版本。

## 支持的数据库

| 数据库        | 支持                                                                   |
|------------|----------------------------------------------------------------------|
| MongoDB    | 是                                                                    |
| 内存         | 是                                                                    |


## 入门

添加 nuget-package Horarium

```bash
dotnet add package MonScheduler
dotnet add package MonScheduler.Mongo
```

添加实现 `IJob<T>` 接口的作业

```csharp
public class TestJob : IJob<int>
{
    public async Task Execute(int param)
    {
        Console.WriteLine(param);
        await Task.Run(() => { });
    }
}
```

创建 `HorariumServer` 并调度 `TestJob`

```csharp
var horarium = new HorariumServer(new InMemoryRepository());
horarium.Start();
await horarium.Schedule<TestJob, int>(666,conf => conf.WithDelay(TimeSpan.FromSeconds(20)));
```

## 添加到 `Asp.Net core` 应用程序

添加 nuget-package Horarium.AspNetCore

```bash
dotnet add package Horarium.AspNetCore
```

添加 `Horarium Server`。这将 Horarium 注册为[托管服务](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)，因此 .Net core 运行时会自动启动并正常停止 Horarium。

```csharp
public void ConfigureServices(IServiceCollection services)
{
    //...
    services.AddHorariumServer(MongoRepositoryFactory.Create("mongodb://localhost:27017/horarium"));
    //...
}
```

将 `IHorarium` 接口注入到控制器中

```csharp

private readonly IHorarium _horarium;

public HomeController(IHorarium horarium)
{
    _horarium = horarium;
}

[Route("api")]
public class HomeController : Controller
{
    [HttpPost]
    public async Task Run(int count)
    {
            await _horarium.Schedule<TestJob, int>(count,conf => conf.WithDelay(TimeSpan.FromSeconds(20)));
    }
}
```

## 创建循环作业

添加实现 `IJobRecurrent` 接口的作业

```csharp
public class TestRecurrentJob : IJobRecurrent
    {
        public Task Execute()
        {
            Console.WriteLine("Run -" + DateTime.Now);
            return Task.CompletedTask;
        }
    }
```

调度 `TestRecurrentJob` 每 15 秒运行一次

```csharp
await horarium.CreateRecurrent<TestRecurrentJob>(Cron.SecondInterval(15))
                .Schedule();
```

## 创建作业序列

有时您需要创建一系列作业，其中每个下一个作业仅在上一个作业成功时才会运行。如果序列中的任何作业失败，则后续作业将不会运行。

```csharp
await horarium
    .Create<TestJob, int>(1) // 第一个作业
    .Next<TestJob, int>(2) // 第二个作业
    .Next<TestJob, int>(3) // 第三个作业
    .Schedule();
```

## 分布式 Horarium
Horarium 有两种类型的工作器：服务器和客户端。服务器可以运行作业和调度新作业，而客户端只能调度新作业。

Horarium 保证作业将**精确执行一次**。

## 注意事项

每个 Horarium 实例每 100 毫秒（默认）向 MongoDB 查询要运行的新作业，从而给数据库服务器带来一些负载。可以在 `HorariumSettings` 中更改此间隔。

如果要减少负载，可以使用作业限制功能，如果在一定的尝试次数后没有可用的作业，该功能将自动增加间隔。要启用此功能，请将 `JobThrottleSettings` 传递给 `HorariumSettings`，并将 `UseJobThrottle` 属性设置为 `true`。

```csharp
var settings = new HorariumSettings
{
    JobThrottleSettings = new JobThrottleSettings
    {
        UseJobThrottle = true
    }
};
```

有关配置的更多信息，请参阅 `JobThrottleSettings`。

## 将 Horarium 与 SimpleInjector 一起使用

要将 Horarium 与 SimpleInjector 一起使用，应该使用 `SimpleInjector` 中的 `Container` 实现自己的 `IJobFactory`。例如：

```csharp
public class SimpleInjectorJobScopeFactory : IJobScopeFactory
{
    private readonly Container _container;

    public SimpleInjectorJobScopeFactory(Container container)
    {
        _container = container;
    }

    public IJobScope Create()
    {
        var scope = AsyncScopedLifestyle.BeginScope(_container);
        return new SimpleInjectorJobScope(scope);
    }
}

public class SimpleInjectorJobScope : IJobScope
{
    private readonly Scope _scope;

    public SimpleInjectorJobScope(Scope scope)
    {
        _scope = scope;
    }

    public object CreateJob(Type type)
    {
        return _scope.GetInstance(type);
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
```

然后添加 `HorariumServer`（或 `HorariumClient`）：

```csharp
container.RegisterSingleton<IHorarium>(() =>
{
    var settings = new HorariumSettings
    {
        JobScopeFactory = new SimpleInjectorJobScopeFactory(container),
        Logger = new YourHorariumLogger()
    };

    return new HorariumServer(jobRepository, settings);
});
```

如果是 `HorariumServer`，请不要忘记在您的入口点启动它：

```csharp
((HorariumServer) container.GetInstance<IHorarium>()).Start();
```

## 作业失败重试策略

当作业失败时，Horarium 可以使用相同的策略处理此异常。
默认情况下，作业会重试 10 次，延迟分别为 10 分钟、20 分钟、30 分钟等。
您可以使用 `IFailedRepeatStrategy` 接口覆盖此策略。

默认 `DefaultRepeatStrategy` 实现示例：

```csharp
public class DefaultRepeatStrategy :IFailedRepeatStrategy
{
    public TimeSpan GetNextStartInterval(int countStarted)
    {
        const int increaseRepeat = 10;
        return TimeSpan.FromMinutes(increaseRepeat * countStarted);
    }
}
```

每次作业失败时都会调用此类，它必须返回下一次计划作业运行的 `TimeSpan`。
要全局覆盖默认行为，请在 `HorariumSettings` 中更改设置。

```csharp
new HorariumSettings
{
    FailedRepeatStrategy = new CustomFailedRepeatStrategy(),
    MaxRepeatCount = 7
});
```

要为特定作业覆盖默认行为：

```csharp
await horarium.Create<TestJob, int>(666)
    .MaxRepeatCount(5)
    .AddRepeatStrategy<DefaultRepeatStrategy>()
    .Schedule();
```

如果要禁用所有重试，只需将 `MaxRepeatCount` 设置为 1。

```csharp
new HorariumSettings
{
    MaxRepeatCount = 1
});
```

## 致谢

**MonScheduler** 的诞生离不开 [Tinkoff/Horarium](https://github.com/Tinkoff/Horarium) 这个出色的项目。

由于原项目已停止更新，我们接过了这根接力棒，在原有代码的基础上进行维护和二次开发，旨在延续其生命力。我们对原作者的开创性工作和无私分享表示最诚挚的感谢，他的工作是 **MonScheduler** 项目的基石。