using System;
using Horarium.Repository;
using MongoDB.Driver;

namespace Horarium.Mongo
{
    public static class MongoRepositoryFactory
    {
        public static IJobRepository Create(string connectionString, Action<MongoClientSettings> configAction = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Connection string is empty");

            var provider = new MongoClientProvider(new MongoUrl(connectionString), configAction);
            return new MongoRepository(provider);
        }

        public static IJobRepository Create(MongoUrl mongoUrl, Action<MongoClientSettings> configAction = null)
        {
            if (mongoUrl == null)
                throw new ArgumentNullException(nameof(mongoUrl), "mongoUrl is null");

            var provider = new MongoClientProvider(mongoUrl, configAction);
            return new MongoRepository(provider);
        }
    }
}
