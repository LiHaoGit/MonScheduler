using System.Threading.Tasks;
using Horarium.IntegrationTest.Jobs;
using Xunit;

namespace Horarium.IntegrationTest
{
    [Collection(IntegrationTestCollection)]
    public class OneTimeJobTest: IntegrationTestBase
    {
        [Fact]
        public async Task OneTimeJob_RunAfterAdded()
        {
            var horarium = CreateHorariumServer();
            
#pragma warning disable CS0618 // Type or member is obsolete
            await horarium.Create<OneTimeJob, int>(5).Schedule();
#pragma warning restore CS0618 // Type or member is obsolete
            
            await Task.Delay(1000, TestContext.Current.CancellationToken);

            horarium.Dispose();

            Assert.True(OneTimeJob.Run);
        }
    }
}