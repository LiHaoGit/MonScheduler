using System.Linq;
using System.Threading.Tasks;
using Horarium.IntegrationTest.Jobs;
using Xunit;

namespace Horarium.IntegrationTest
{
    [Collection(IntegrationTestCollection)]
    public class TestParallelsWorkTwoManagers : IntegrationTestBase
    {
        [Fact]
        public async Task TestParallels()
        {
            var firstScheduler = CreateHorariumServer();
            var secondScheduler = CreateHorariumServer();

            for (var i = 0; i < 1000; i++)
            {
#pragma warning disable CS0618 // Type or member is obsolete
              await firstScheduler.Create<TestJob, int>(i).Schedule();
#pragma warning restore CS0618 // Type or member is obsolete
              await Task.Delay(10, TestContext.Current.CancellationToken);
            }

            await Task.Delay(10000, TestContext.Current.CancellationToken);

            firstScheduler.Dispose();
            secondScheduler.Dispose();

            Assert.NotEmpty(TestJob.StackJobs);

            Assert.False(TestJob.StackJobs.GroupBy(x => x).Any(g => g.Count() > 1),
                "Same job was executed multiple times");
        }
        
    }
}