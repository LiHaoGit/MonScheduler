using System.Threading.Tasks;
using Horarium.IntegrationTest.Jobs;
using Xunit;

namespace Horarium.IntegrationTest
{
    [Collection(IntegrationTestCollection)]
    public class SequenceJobTest : IntegrationTestBase
    {
        [Fact]
        public async Task SequenceJobsAdded_ExecutedSequence()
        {
            var horarium = CreateHorariumServer();
            
#pragma warning disable CS0618 // Type or member is obsolete
            await horarium.Create<SequenceJob, int>(0)
#pragma warning restore CS0618 // Type or member is obsolete
              .Next<SequenceJob, int>(1)
                .Next<SequenceJob, int>(2)
                .Schedule();
            
            await Task.Delay(1000, TestContext.Current.CancellationToken);

            horarium.Dispose();

            var queueJobs = SequenceJob.QueueJobs.ToArray();
            
            Assert.NotEmpty(queueJobs);
            
            Assert.Equal(new []  {0,1,2}, queueJobs);
        }
    }
}