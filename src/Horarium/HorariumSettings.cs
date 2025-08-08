using System;
using System.Text.Json;
using Horarium.Interfaces;

namespace Horarium
{
    public class HorariumSettings
    {
        public TimeSpan IntervalStartJob { get; set; } = TimeSpan.FromMilliseconds(100);

        public TimeSpan ObsoleteExecutingJob { get; set; } = TimeSpan.FromMinutes(5);

        public JobThrottleSettings JobThrottleSettings { get; set; } = new JobThrottleSettings();

        public IJobScopeFactory JobScopeFactory { get; set; } = new DefaultJobScopeFactory();

        public IHorariumLogger Logger { get; set; } = new EmptyLogger();

        public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions();

        public IFailedRepeatStrategy FailedRepeatStrategy { get; set; } = new DefaultRepeatStrategy();

        public int MaxRepeatCount { get; set; } = 10;
    }
}