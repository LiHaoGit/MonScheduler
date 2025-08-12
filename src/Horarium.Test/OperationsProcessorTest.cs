using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Horarium.InMemory;
using Horarium.Repository;
using Xunit;

namespace Horarium.Test
{
    public class OperationsProcessorTest
    {
        [Fact]
        public async Task Execute_ShouldExecuteAction()
        {
            // Arrange
            var processor = new OperationsProcessor();
            var executed = false;

            // Act
            await processor.Execute(() => { executed = true; });

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public async Task Execute_ShouldReturnResult()
        {
            // Arrange
            var processor = new OperationsProcessor();
            var expectedJob = new JobDb { JobId = "test-job" };

            // Act
            var result = await processor.Execute(() => expectedJob);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test-job", result.JobId);
        }

        [Fact]
        public async Task Execute_ShouldBeThreadSafe()
        {
            // Arrange
            var processor = new OperationsProcessor();
            var counter = 0;
            var tasks = new Task<JobDb>[10];

            // Act
            for (var i = 0; i < 10; i++)
            {
                var i1 = i;
                tasks[i] = processor.Execute(() => 
                {
                    Interlocked.Increment(ref counter);
                    return new JobDb { JobId = $"job-{i1}" };
                });
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(10, counter);
        }

        [Fact]
        public async Task Execute_WithException_ShouldPropagateException()
        {
            // Arrange
            var processor = new OperationsProcessor();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                processor.Execute(() => throw new InvalidOperationException("Test exception")));
        }

        [Fact]
        public async Task Execute_ShouldProcessInOrder()
        {
            // Arrange
            var processor = new OperationsProcessor();
            var executionOrder = new List<int>();

            // Act
            var task1 = processor.Execute(() => { executionOrder.Add(1); return new JobDb(); });
            var task2 = processor.Execute(() => { executionOrder.Add(2); return new JobDb(); });
            var task3 = processor.Execute(() => { executionOrder.Add(3); return new JobDb(); });

            await Task.WhenAll(task1, task2, task3);

            // Assert
            Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
        }

        [Fact]
        public async Task Execute_ShouldWorkWithMultipleProcessors()
        {
            // Arrange
            var processor1 = new OperationsProcessor();
            var processor2 = new OperationsProcessor();
            var counter = 0;

            // Act
            await Task.WhenAll(
                processor1.Execute(() => { Interlocked.Increment(ref counter); return new JobDb(); }),
                processor2.Execute(() => { Interlocked.Increment(ref counter); return new JobDb(); })
            );

            // Assert
            Assert.Equal(2, counter);
        }
    }
}