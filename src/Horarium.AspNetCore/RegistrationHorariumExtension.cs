﻿using System;
using Horarium.Interfaces;
using Horarium.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Horarium.AspNetCore
{
    public static class RegistrationHorariumExtension
    {
        public static IServiceCollection AddHorariumServer(this IServiceCollection service,
            IJobRepository jobRepository)
        {
            return service.AddHorariumServer(jobRepository, serviceProvider => new HorariumSettings());
        }

        public static IServiceCollection AddHorariumServer(this IServiceCollection service,
            IJobRepository jobRepository,
            Func<IServiceProvider, HorariumSettings> func)
        {
            service.AddSingleton<IHorarium>(serviceProvider =>
            {
                var settings = func(serviceProvider);

                PrepareSettings(settings, serviceProvider);

                return new HorariumServer(jobRepository, settings);
            });

            service.AddHostedService<HorariumServerHostedService>();

            return service;
        }
        
        public static IServiceCollection AddHorariumClient(this IServiceCollection service,
            IJobRepository jobRepository)
        {
            return service.AddHorariumClient(jobRepository, serviceProvider => new HorariumSettings());
        }

        public static IServiceCollection AddHorariumClient(this IServiceCollection service,
            IJobRepository jobRepository,
            Func<IServiceProvider, HorariumSettings> func)
        {
            service.AddSingleton<IHorarium>(serviceProvider =>
            {
                var settings = func(serviceProvider);

                PrepareSettings(settings, serviceProvider);

                return new HorariumClient(jobRepository, settings);
            });

            return service;
        }

        private static void PrepareSettings(HorariumSettings settings, IServiceProvider serviceProvider)
        {
            if (settings.JobScopeFactory is DefaultJobScopeFactory)
            {
                settings.JobScopeFactory = new JobScopeFactory(serviceProvider);
            }

            if (settings.Logger is EmptyLogger)
            {
                settings.Logger = new HorariumLogger(serviceProvider.GetService<ILogger<HorariumLogger>>());
            }
        }
    }
}