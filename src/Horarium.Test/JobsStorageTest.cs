using System;
using System.Collections.Generic;
using Horarium.InMemory;
using Horarium.Repository;
using Xunit;

namespace Horarium.Test
{
    public class JobsStorageTest
    {
        private readonly JobsStorage _storage = new();

        [Fact]
        public void AddRange_WithMultipleJobs_ShouldAddAllJobs()
        {
            // Arrange
            var jobs = new List<JobDb>
            {
                CreateTestJob("job1"),
                CreateTestJob("job2"),
                CreateTestJob("job3")
            };

            // Act
            _storage.AddRange(jobs);

            // Assert
            var job1 = _storage.GetById("job1");
            var job2 = _storage.GetById("job2");
            var job3 = _storage.GetById("job3");

            Assert.NotNull(job1);
            Assert.NotNull(job2);
            Assert.NotNull(job3);

            Assert.Equal("job1", job1.JobId);
            Assert.Equal("job2", job2.JobId);
            Assert.Equal("job3", job3.JobId);
        }

        [Fact]
        public void AddRange_WithEmptyList_ShouldNotThrowException()
        {
            // Arrange
            // ReSharper disable once CollectionNeverUpdated.Local
            List<JobDb> emptyJobs = [];

            // Act & Assert
            var exception = Record.Exception(() => _storage.AddRange(emptyJobs));
            Assert.Null(exception);
        }

        [Fact]
        public void AddRange_WithNullList_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _storage.AddRange(null));
        }

        [Fact]
        public void AddRange_WithDuplicateJobIds_ShouldThrowArgumentException()
        {
            // Arrange
            var jobs = new List<JobDb>
            {
                CreateTestJob("duplicate-id"),
                CreateTestJob("duplicate-id") // Same ID
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _storage.AddRange(jobs));
        }

        [Fact]
        public void AddRange_WithDifferentStatuses_ShouldUpdateIndexesCorrectly()
        {
            // Arrange
            var jobs = new List<JobDb>
            {
                CreateTestJob("ready-job"),
                CreateTestJob("executing-job", JobStatus.Executing),
                CreateTestJob("failed-job", JobStatus.Failed),
                CreateTestJob("repeat-job", JobStatus.RepeatJob)
            };

            // Act
            _storage.AddRange(jobs);

            // Assert
            var stats = _storage.GetStatistics();

            Assert.Equal(1, stats[JobStatus.Ready]);
            Assert.Equal(1, stats[JobStatus.Executing]);
            Assert.Equal(1, stats[JobStatus.Failed]);
            Assert.Equal(1, stats[JobStatus.RepeatJob]);
        }

        [Fact]
        public void AddRange_WithReadyJobs_ShouldBeFindable()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var jobs = new List<JobDb>
            {
                CreateTestJob("job1", JobStatus.Ready, now.AddMinutes(-1)), // Past
                CreateTestJob("job2", JobStatus.Ready, now.AddMinutes(1)), // Future
                CreateTestJob("job3", JobStatus.Ready, now.AddMinutes(-2)) // Past
            };

            // Act
            _storage.AddRange(jobs);

            // Assert
            var readyJob = _storage.FindReadyJob(TimeSpan.FromMinutes(5));
            Assert.NotNull(readyJob);

            // Should find the earliest past job
            Assert.True(readyJob.JobId == "job1" || readyJob.JobId == "job3");
        }

        [Fact]
        public void AddRange_ShouldWorkWithIndividualAdd()
        {
            // Arrange
            var individualJob = CreateTestJob("individual-job");
            var batchJobs = new List<JobDb>
            {
                CreateTestJob("batch-job1"),
                CreateTestJob("batch-job2")
            };

            // Act
            _storage.Add(individualJob);
            _storage.AddRange(batchJobs);

            // Assert
            Assert.NotNull(_storage.GetById("individual-job"));
            Assert.NotNull(_storage.GetById("batch-job1"));
            Assert.NotNull(_storage.GetById("batch-job2"));

            var stats = _storage.GetStatistics();
            Assert.Equal(3, stats[JobStatus.Ready]);
        }

        private JobDb CreateTestJob(string jobId, JobStatus status = JobStatus.Ready, DateTime? startAt = null)
        {
            return new JobDb
            {
                JobId = jobId,
                JobType = "TestJob",
                Status = status,
                StartAt = startAt ?? DateTime.UtcNow,
                CountStarted = 0,
                JobParam = "{}"
            };
        }
    }
}