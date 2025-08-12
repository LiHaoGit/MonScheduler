using System;
using System.Threading.Tasks;
using Horarium.Interfaces;

namespace Horarium.Builders.Recurrent
{
    internal class RecurrentJobBuilder : IRecurrentJobBuilder
    {
        private readonly JobMetadata _job;
        private readonly IAdderJobs _adderJobs;

        public RecurrentJobBuilder(IAdderJobs adderJobs, string cron, Type jobType, TimeSpan obsoleteInterval)
        {
            _job = JobBuilderHelpers.GenerateNewJob(jobType);

            _adderJobs = adderJobs;
            _job.ObsoleteInterval = obsoleteInterval;

            _job.Cron = cron;
        }

        public IRecurrentJobBuilder WithKey(string jobKey)
        {
            _job.JobKey = jobKey;
            return this;
        }

        public async Task<string> Schedule()
        {
            var nextOccurence = Utils.ParseAndGetNextOccurrence(_job.Cron);

            if (!nextOccurence.HasValue)
            {
                return _job.JobId;
            }

            _job.StartAt = nextOccurence.Value;
            _job.JobKey ??= _job.JobType.Name;

            return await _adderJobs.AddRecurrentJob(_job);
        }
    }
}