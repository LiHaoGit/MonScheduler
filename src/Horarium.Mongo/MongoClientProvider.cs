﻿using System;
using System.Collections.Concurrent;
using System.Reflection;
using MongoDB.Driver;

namespace Horarium.Mongo
{
    public sealed class MongoClientProvider : IMongoClientProvider
    {
        private readonly ConcurrentDictionary<Type, string> _collectionNameCache = new ConcurrentDictionary<Type, string>();

        private readonly MongoClient _mongoClient;
        private readonly string _databaseName;
        private bool _initialized;
        private object _lockObject = new object();

        public MongoClientProvider(MongoUrl mongoUrl)
        {
            _databaseName = mongoUrl.DatabaseName;
            _mongoClient = new MongoClient(mongoUrl);
        }
        
        public MongoClientProvider(string mongoConnectionString): this (new MongoUrl(mongoConnectionString))
        {
        }

        private string GetCollectionName(Type entityType)
        {
            var collectionAttr = entityType.GetTypeInfo().GetCustomAttribute<MongoEntityAttribute>();

            if (collectionAttr == null)
                throw new InvalidOperationException($"Entity with type '{entityType.GetTypeInfo().FullName}' is not Mongo entity (use MongoEntityAttribute)");

            return collectionAttr.CollectionName;
        }

        public IMongoCollection<TEntity> GetCollection<TEntity>()
        {
            EnsureInitialized();

            var collectionName = _collectionNameCache.GetOrAdd(typeof(TEntity), GetCollectionName);
            return _mongoClient.GetDatabase(_databaseName).GetCollection<TEntity>(collectionName);
        }

        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            lock (_lockObject)
            {
                if (_initialized)
                    return;

                _initialized = true;
                CreateIndexes();
            }
        }

        private void CreateIndexes()
        {
            var indexKeyBuilder = Builders<JobMongoModel>.IndexKeys;

            var collection = GetCollection<JobMongoModel>();

            collection.Indexes.CreateMany(new[]
            {
                new CreateIndexModel<JobMongoModel>(
                    indexKeyBuilder    
                        .Ascending(x => x.Status)
                        .Ascending(x=>x.StartAt)
                        .Ascending(x=>x.StartedExecuting),
                    new CreateIndexOptions
                    {
                        Background = true
                    }),
                
                new CreateIndexModel<JobMongoModel>(
                    indexKeyBuilder
                        .Ascending(x => x.Status)
                        .Ascending(x => x.JobKey),
                    new CreateIndexOptions
                    {
                        Background = true
                    }),
                
                new CreateIndexModel<JobMongoModel>(
                    indexKeyBuilder
                        .Ascending(x => x.JobKey),
                    new CreateIndexOptions
                    {
                        Background = true
                    })
            });
        }
    }
}