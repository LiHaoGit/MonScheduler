using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Horarium.IntegrationTest.Jobs;
using Xunit;

namespace Horarium.IntegrationTest
{
    [Collection(IntegrationTestCollection)]
    public class RecurrentJobTest : IntegrationTestBase
    {
        [Fact]
        public async Task RecurrentJob_RunEverySeconds()
        {
            var horarium = CreateHorariumServer();

            await horarium.CreateRecurrent<RecurrentJob>(Cron.Secondly()).Schedule();

            await Task.Delay(10000, TestContext.Current.CancellationToken);

            horarium.Dispose();

            var executingTimes = RecurrentJob.ExecutingTime.ToArray();

            Assert.NotEmpty(executingTimes);

            var nextJobTime = executingTimes.First();

            foreach (var time in executingTimes)
            {
                Assert.Equal(nextJobTime, time, TimeSpan.FromMilliseconds(999));
                nextJobTime = time.AddSeconds(1);
            }
        }

        /// <summary>
        /// 该测试验证，如果同一作业由不同的 Scheduler 同时注册，则第一个作业将开始执行，而第二个作业将不会，因为一次只能执行一个实例来执行循环作业
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Scheduler_SecondInstanceStart_MustUpdateRecurrentJobCronParameters()
        {
            var watch = Stopwatch.StartNew();
            var scheduler = CreateHorariumServer();

            while (true)
            {
                await scheduler.CreateRecurrent<RecurrentJobForUpdate>(Cron.SecondInterval(1)).Schedule();

                if (watch.Elapsed > TimeSpan.FromSeconds(15))
                {
                    break;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            scheduler.Dispose();

            Assert.Single(RecurrentJobForUpdate.StackJobs);
        }
    }
}