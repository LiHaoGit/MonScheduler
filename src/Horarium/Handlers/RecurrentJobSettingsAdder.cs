using System;
using System.Text.Json;
using System.Threading.Tasks;
using Horarium.Interfaces;
using Horarium.Repository;

namespace Horarium.Handlers
{
    public class RecurrentJobSettingsAdder(IJobRepository jobRepository, JsonSerializerOptions jsonSerializerSettings)
        : IRecurrentJobSettingsAdder
    {
        private readonly JsonSerializerOptions _jsonSerializerSettings = jsonSerializerSettings;

        public async Task Add(string cron, Type jobType, string jobKey)
        {
            var settings = new RecurrentJobSettingsMetadata(jobKey, jobType, cron);

            await jobRepository.AddRecurrentJobSettings(RecurrentJobSettings.CreatedRecurrentJobSettings(settings));
        }
    }
}
