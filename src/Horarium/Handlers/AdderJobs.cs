using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Horarium.Interfaces;
using Horarium.Repository;

namespace Horarium.Handlers
{
    public class AdderJobs : IAdderJobs
    {
        private readonly IJobRepository _jobRepository;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly IRecurrentJobSettingsAdder _recurrentJobSettingsAdder;

        public AdderJobs(IJobRepository jobRepository, JsonSerializerOptions jsonSerializerOptions)
        {
            _jobRepository = jobRepository;
            _jsonSerializerOptions = jsonSerializerOptions;
            _recurrentJobSettingsAdder = new RecurrentJobSettingsAdder(_jobRepository, _jsonSerializerOptions);
        }

        public async Task<string> AddEnqueueJob(JobMetadata jobMetadata)
        {
            var job = JobDb.CreatedJobDb(jobMetadata, _jsonSerializerOptions);

            await _jobRepository.AddJob(job);

            return jobMetadata.JobId;
        }

        public async Task AddEnqueueJobs(IEnumerable<JobMetadata> jobMetadatas)
        {
            var jobs = jobMetadatas.Select(x => JobDb.CreatedJobDb(x, _jsonSerializerOptions)).ToList();

            await _jobRepository.AddJobs(jobs);
        }

        public async Task<string> AddRecurrentJob(JobMetadata jobMetadata)
        {
            await _recurrentJobSettingsAdder.Add(jobMetadata.Cron, jobMetadata.JobType, jobMetadata.JobKey);

            await _jobRepository.AddRecurrentJob(JobDb.CreatedJobDb(jobMetadata, _jsonSerializerOptions));

            return jobMetadata.JobId;
        }
    }
}