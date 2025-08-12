using System.Collections.Concurrent;
using System.Threading.Tasks;
using Horarium.Interfaces;

namespace Horarium.IntegrationTest.Jobs
{
    public class OneTimeJob : IJob<int>
    {
        public static bool Run;

        public Task Execute(int param)
        {
            Run = true;

            return Task.CompletedTask;
        }
    }

    public class OneTimeJobs : IJob<int>
    {
        public static readonly ConcurrentQueue<int> QueueJobs = new ConcurrentQueue<int>();

        public Task Execute(int param)
        {
            QueueJobs.Enqueue(param);

            return Task.CompletedTask;
        }
    }
}