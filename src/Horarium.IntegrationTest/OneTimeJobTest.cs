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
            
            await horarium.Schedule<OneTimeJob,int>(5);
            
            await Task.Delay(1000, TestContext.Current.CancellationToken);

            horarium.Dispose();

            Assert.True(OneTimeJob.Run);
        }
    }
}