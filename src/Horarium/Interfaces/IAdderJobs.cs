using System.Collections.Generic;
using System.Threading.Tasks;

namespace Horarium.Interfaces
{
    public interface IAdderJobs
    {
        Task<string> AddEnqueueJob(JobMetadata jobMetadata);

        Task AddEnqueueJobs(IEnumerable<JobMetadata> jobMetadatas);

        Task<string> AddRecurrentJob(JobMetadata jobMetadata);
    }
}