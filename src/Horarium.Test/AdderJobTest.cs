using System;
using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using Horarium.Handlers;
using Horarium.Repository;
using Xunit;

namespace Horarium.Test
{
    public class AdderJobTest
    {
        [Fact]
        public async Task AddNewRecurrentJob_Success()
        {
            // Arrange
            var jobRepositoryMock = new Mock<IJobRepository>();

            var jobsAdder = new AdderJobs(jobRepositoryMock.Object, new JsonSerializerOptions());

            var job = new JobMetadata
            {
                Cron = Cron.SecondInterval(15),
                ObsoleteInterval = TimeSpan.FromMinutes(5),
                JobType = typeof(TestReccurrentJob),
                JobKey = nameof(TestReccurrentJob),
                Status = JobStatus.Ready,
                JobId = Guid.NewGuid().ToString("N"),
                StartAt = DateTime.UtcNow + TimeSpan.FromSeconds(10),
                CountStarted = 0
            };

            // Act
            var addRecurrentJob = await jobsAdder.AddRecurrentJob(job);

            // Assert
            jobRepositoryMock.Verify(x => x.AddRecurrentJob(It.Is<JobDb>(j => j.Status == job.Status
                                                                              && j.CountStarted == job.CountStarted
                                                                              && j.JobKey == job.JobKey
                                                                              && j.Cron == job.Cron
                                                                              && j.JobId == job.JobId
            )), Times.Once);

            Assert.Equal(addRecurrentJob, job.JobId);
        }
        
        
        [Fact]
        public async Task AddEnqueueJob_Success()
        {
            // Arrange
            var jobRepositoryMock = new Mock<IJobRepository>();

            var jobsAdder = new AdderJobs(jobRepositoryMock.Object, new JsonSerializerOptions());

            var job = new JobMetadata
            {
                JobType = typeof(TestReccurrentJob),
                JobKey = nameof(TestReccurrentJob),
                Status = JobStatus.Ready,
                JobId = Guid.NewGuid().ToString("N"),
                StartAt = DateTime.UtcNow + TimeSpan.FromSeconds(10),
                CountStarted = 0,
                Delay = TimeSpan.FromSeconds(20)
            };

            // Act
            var enqueueJob = await jobsAdder.AddEnqueueJob(job);

            // Assert
            jobRepositoryMock.Verify(x => x.AddJob(It.Is<JobDb>(j => j.Status == job.Status
                                                                              && j.CountStarted == job.CountStarted
                                                                              && j.JobKey == job.JobKey
                                                                              && j.JobId == job.JobId
                                                                              && j.Delay == job.Delay
            )), Times.Once);

            Assert.Equal(enqueueJob, job.JobId);
        }
    }
}