using System;
using System.Collections.Generic;
using Horarium.InMemory.Indexes;
using Horarium.Repository;

namespace Horarium.InMemory
{
    internal class JobsStorage
    {
        private readonly Dictionary<string, JobDb> _jobs = new();

        private readonly ReadyJobIndex _readyJobIndex = new();
        private readonly ExecutingJobIndex _executingJobIndex = new();
        private readonly RepeatJobIndex _repeatJobIndex = new();
        private readonly FailedJobIndex _failedJobIndex = new();

        private readonly List<IAddRemoveIndex> _indexes;

        public JobsStorage()
        {
            _indexes =
            [
                _readyJobIndex,
                _executingJobIndex,
                _repeatJobIndex,
                _failedJobIndex
            ];
        }

        public void Add(JobDb job)
        {
            _jobs.Add(job.JobId, job);

            _indexes.ForEach(x => x.Add(job));
        }

        public void AddRange(IEnumerable<JobDb> jobs)
        {
            ArgumentNullException.ThrowIfNull(jobs);

            foreach (var job in jobs)
            {
                _jobs.Add(job.JobId, job);
                _indexes.ForEach(x => x.Add(job));
            }
        }

        public void Remove(string jobId)
        {
            if (!_jobs.TryGetValue(jobId, out var job)) return;

            Remove(job);
        }

        public void Remove(JobDb job)
        {
            _jobs.Remove(job.JobId);

            _indexes.ForEach(x => x.Remove(job));
        }

        public Dictionary<JobStatus, int> GetStatistics()
        {
            return new Dictionary<JobStatus, int>
            {
                { JobStatus.Ready, _readyJobIndex.Count() },
                { JobStatus.Executing, _executingJobIndex.Count() },
                { JobStatus.RepeatJob, _repeatJobIndex.Count() },
                { JobStatus.Failed, _failedJobIndex.Count() }
            };
        }

        public JobDb GetById(string jobId)
        {
            if (!_jobs.TryGetValue(jobId, out var job)) return null;

            return job;
        }

        public JobDb FindRecurrentJobToUpdate(string jobKey)
        {
            return _readyJobIndex.GetJobKeyEqual(jobKey) ?? _executingJobIndex.GetJobKeyEqual(jobKey);
        }

        public JobDb FindReadyJob(TimeSpan obsoleteTime)
        {
            var now = DateTime.UtcNow;

            return _readyJobIndex.GetStartAtLessThan(now) ??
                   _repeatJobIndex.GetStartAtLessThan(now) ??
                   _executingJobIndex.GetStartedExecutingLessThan(now - obsoleteTime);
        }
    }
}