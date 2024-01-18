﻿using CacheFlow.Builders;
using CacheFlow.Services;
using CacheFlow.Settings;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CacheFlow.Extensions;

public static class ServiceCollectionExtensions
{
    private static IServiceCollection AddCacheService(
        this IServiceCollection services,
        TimeSpan cacheExpireTime,
        string redisConnectionString, 
        ServiceLifetime serviceLifetime)
    {
        services.AddOptions<CacheSettings>()
            .Configure(settings => { settings.ExpireTime = cacheExpireTime; });
        
        return services
            .AddConnectionMultiplexer(redisConnectionString, serviceLifetime)
            .AddCacheService(serviceLifetime);
    }

    public static IServiceCollection AddCacheService(
        this IServiceCollection services,
        Action<CacheServiceBuilder> options = null!)
    {
        var builder = new CacheServiceBuilder();
        options.Invoke(builder);

        return services.AddCacheService(builder.ExpireTime, builder.ConnectionString, builder.ServiceLifetime);
    }

    private static IServiceCollection AddConnectionMultiplexer(this IServiceCollection services, string connectionString,
        ServiceLifetime serviceLifetime)
    {
        var connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
        return serviceLifetime switch
        {
            ServiceLifetime.Singleton => services.AddSingleton<IConnectionMultiplexer>(_ => connectionMultiplexer),
            ServiceLifetime.Scoped => services.AddScoped<IConnectionMultiplexer>(_ => connectionMultiplexer),
            ServiceLifetime.Transient => services.AddTransient<IConnectionMultiplexer>(_ => connectionMultiplexer),
            _ => throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null)
        };
    }

    private static IServiceCollection AddCacheService(this IServiceCollection services, ServiceLifetime serviceLifetime)
    {
        return serviceLifetime switch
        {
            ServiceLifetime.Singleton => services.AddSingleton<ICacheService, CacheService>(),
            ServiceLifetime.Scoped => services.AddScoped<ICacheService, CacheService>(),
            ServiceLifetime.Transient => services.AddTransient<ICacheService, CacheService>(),
            _ => throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null)
        };
    }
}