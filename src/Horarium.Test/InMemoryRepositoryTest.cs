using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Horarium.InMemory;
using Horarium.Repository;
using Xunit;

namespace Horarium.Test
{
    public class InMemoryRepositoryTest
    {
        private readonly InMemoryRepository _repository = new();

        [Fact]
        public async Task AddJobs_WithMultipleJobs_ShouldAddAllJobs()
        {
            // Arrange
            var jobs = new List<JobDb>
            {
                CreateTestJob("job1"),
                CreateTestJob("job2"),
                CreateTestJob("job3")
            };

            // Act
            await _repository.AddJobs(jobs);

            // Assert
            // Verify that all jobs were added by checking the statistics
            var stats = await _repository.GetJobStatistic();
            Assert.Equal(3, stats[JobStatus.Ready]);

            // Get one job to verify it works
            var readyJob = await _repository.GetReadyJob("test-machine", TimeSpan.FromMinutes(1));
            Assert.NotNull(readyJob);
            Assert.Contains(readyJob.JobId, new[] { "job1", "job2", "job3" });

            // After getting one job, the remaining jobs should still be in Ready status
            stats = await _repository.GetJobStatistic();
            Assert.Equal(2, stats[JobStatus.Ready]);
            Assert.Equal(1, stats[JobStatus.Executing]);
        }

        [Fact]
        public async Task AddJobs_WithEmptyList_ShouldCompleteSuccessfully()
        {
            // Arrange
            var emptyJobs = new List<JobDb>();

            // Act & Assert
            await _repository.AddJobs(emptyJobs);

            // Should not throw any exception
        }

        [Fact]
        public async Task AddJobs_WithNullJobs_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddJobs(null));
        }

        [Fact]
        public async Task AddJobs_WithDuplicateJobIds_ShouldThrowArgumentException()
        {
            // Arrange
            var jobs = new List<JobDb>
            {
                CreateTestJob("duplicate-id"),
                CreateTestJob("duplicate-id") // Same ID
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.AddJobs(jobs));
        }

        [Fact]
        public async Task AddJobs_WithDifferentStartTimes_ShouldMaintainOrder()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var jobs = new List<JobDb>
            {
                CreateTestJob("job1", now.AddMinutes(-5)),  // 5 minutes ago (earliest)
                CreateTestJob("job2", now.AddMinutes(-1)),  // 1 minute ago (latest)
                CreateTestJob("job3", now.AddMinutes(-3))   // 3 minutes ago (middle)
            };

            // Act
            await _repository.AddJobs(jobs);

            // Assert
            var firstJob = await _repository.GetReadyJob("test-machine", TimeSpan.FromMinutes(1));
            Assert.NotNull(firstJob);
            Assert.Equal("job1", firstJob.JobId); // Should be the earliest job (5 minutes ago)
        }

        private JobDb CreateTestJob(string jobId, DateTime? startAt = null)
        {
            return new JobDb
            {
                JobId = jobId,
                JobType = "TestJob",
                Status = JobStatus.Ready,
                StartAt = startAt ?? DateTime.UtcNow,
                CountStarted = 0,
                JobParam = "{}", ExecutedMachine = "test-machine"
            };
        }
    }
}