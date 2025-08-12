using System.Threading.Tasks;

namespace Horarium.Interfaces
{
    public interface IAdderJobs
    {
        Task<string> AddEnqueueJob(JobMetadata jobMetadata);

        Task<string> AddRecurrentJob(JobMetadata jobMetadata);
    }
}