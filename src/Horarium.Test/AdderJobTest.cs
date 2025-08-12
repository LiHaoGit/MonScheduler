using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Horarium.Handlers;
using Horarium.Repository;
using Moq;
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

        [Fact]
        public async Task AddEnqueueJobs_Success()
        {
            // Arrange
            var jobRepositoryMock = new Mock<IJobRepository>();

            var jobsAdder = new AdderJobs(jobRepositoryMock.Object, new JsonSerializerOptions());

            var jobs = new[]
            {
                new JobMetadata
                {
                    JobType = typeof(TestJob),
                    JobKey = nameof(TestJob) + "1",
                    Status = JobStatus.Ready,
                    JobId = Guid.NewGuid().ToString("N"),
                    StartAt = DateTime.UtcNow + TimeSpan.FromSeconds(10),
                    CountStarted = 0,
                    Delay = TimeSpan.FromSeconds(20)
                },
                new JobMetadata
                {
                    JobType = typeof(TestJob),
                    JobKey = nameof(TestJob) + "2",
                    Status = JobStatus.Ready,
                    JobId = Guid.NewGuid().ToString("N"),
                    StartAt = DateTime.UtcNow + TimeSpan.FromSeconds(10),
                    CountStarted = 0,
                    Delay = TimeSpan.FromSeconds(20)
                }
            };

            // Act
            await jobsAdder.AddEnqueueJobs(jobs);

            // Assert
            jobRepositoryMock.Verify(x => x.AddJobs(It.Is<List<JobDb>>(j => j.Count == 2)), Times.Once);
        }
    }
}