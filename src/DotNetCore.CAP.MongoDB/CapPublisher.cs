// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Threading.Tasks;
using DotNetCore.CAP.Abstractions;
using DotNetCore.CAP.Diagnostics;
using DotNetCore.CAP.Infrastructure;
using DotNetCore.CAP.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace DotNetCore.CAP.MongoDB
{
    public class CapPublisher : CapPublisherBase, ICallbackPublisher
    {
        private readonly MongoDBOptions _options;
        private readonly IMongoDatabase _database;
        private bool _usingTransaction = true;

        public CapPublisher(
            ILogger<CapPublisherBase> logger,
            IDispatcher dispatcher,
            IMongoClient client,
            MongoDBOptions options,
            IServiceProvider provider)
            : base(logger, dispatcher)
        {
            _options = options;
            _database = client.GetDatabase(_options.DatabaseName);
            ServiceProvider = provider;
        }

        public async Task PublishCallbackAsync(CapPublishedMessage message)
        {
            var collection = _database.GetCollection<CapPublishedMessage>(_options.PublishedCollection);
            message.Id = await new MongoDBUtil().GetNextSequenceValueAsync(_database, _options.PublishedCollection);
            collection.InsertOne(message);
            Enqueue(message);
        }

        protected override int Execute(IDbConnection dbConnection, IDbTransaction dbTransaction,
            CapPublishedMessage message)
        {
            throw new NotImplementedException("Not work for MongoDB");
        }

        protected override Task<int> ExecuteAsync(IDbConnection dbConnection, IDbTransaction dbTransaction,
            CapPublishedMessage message)
        {
            throw new NotImplementedException("Not work for MongoDB");
        }

        protected override void PrepareConnectionForEF()
        {
            throw new NotImplementedException("Not work for MongoDB");
        }

        public override void PublishWithMongo<T>(string name, T contentObj, IMongoTransaction mongoTransaction = null, string callbackName = null)
        {
            if (mongoTransaction == null)
            {
                _usingTransaction = false;
                mongoTransaction = new NullMongoTransaction();
            }

            PublishWithTransaction<T>(name, contentObj, mongoTransaction, callbackName);
        }

        public override async Task PublishWithMongoAsync<T>(string name, T contentObj, IMongoTransaction mongoTransaction = null, string callbackName = null)
        {
            if (mongoTransaction == null)
            {
                _usingTransaction = false;
                mongoTransaction = new NullMongoTransaction();
            }

            await PublishWithSessionAsync<T>(name, contentObj, mongoTransaction, callbackName);
        }

        private void PublishWithTransaction<T>(string name, T contentObj, IMongoTransaction transaction, string callbackName)
        {
            var operationId = default(Guid);

            var content = Serialize(contentObj, callbackName);

            var message = new CapPublishedMessage
            {
                Name = name,
                Content = content,
                StatusName = StatusName.Scheduled
            };

            var session = transaction.GetSession();

            try
            {
                operationId = s_diagnosticListener.WritePublishMessageStoreBefore(message);
                var id = Execute(session, message);

                if (transaction.AutoCommit)
                {
                    session.CommitTransaction();
                }

                if (!_usingTransaction || (transaction.AutoCommit && id > 0))
                {
                    _logger.LogInformation($"message [{message}] has been persisted in the database.");
                    s_diagnosticListener.WritePublishMessageStoreAfter(operationId, message);
                    message.Id = id;
                    Enqueue(message);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception was occurred when publish message. message:" + name);
                s_diagnosticListener.WritePublishMessageStoreError(operationId, message, e);
                throw;
            }
        }

        private int Execute(IClientSessionHandle session, CapPublishedMessage message)
        {
            message.Id = new MongoDBUtil().GetNextSequenceValue(_database, _options.PublishedCollection, session);

            var collection = _database.GetCollection<CapPublishedMessage>(_options.PublishedCollection);
            if (_usingTransaction)
            {
                collection.InsertOne(session, message);
            }
            else
            {
                collection.InsertOne(message);
            }

            return message.Id;
        }

        private async Task PublishWithSessionAsync<T>(string name, T contentObj, IMongoTransaction transaction, string callbackName)
        {
            var operationId = default(Guid);
            var content = Serialize(contentObj, callbackName);

            var message = new CapPublishedMessage
            {
                Name = name,
                Content = content,
                StatusName = StatusName.Scheduled
            };

            var session = transaction.GetSession();

            try
            {
                operationId = s_diagnosticListener.WritePublishMessageStoreBefore(message);

                var id = await ExecuteAsync(session, message);

                if (transaction.AutoCommit)
                {
                    await session.CommitTransactionAsync();
                }

                if (!_usingTransaction || (transaction.AutoCommit && id > 0))
                {
                    _logger.LogInformation($"message [{message}] has been persisted in the database.");
                    s_diagnosticListener.WritePublishMessageStoreAfter(operationId, message);

                    message.Id = id;

                    Enqueue(message);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception was occurred when publish message async. exception message:" + name);
                s_diagnosticListener.WritePublishMessageStoreError(operationId, message, e);
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<int> ExecuteAsync(IClientSessionHandle session, CapPublishedMessage message)
        {
            message.Id =
                await new MongoDBUtil().GetNextSequenceValueAsync(_database, _options.PublishedCollection, session);
            var collection = _database.GetCollection<CapPublishedMessage>(_options.PublishedCollection);
            if (_usingTransaction)
            {
                await collection.InsertOneAsync(session, message);
            }
            else
            {
                await collection.InsertOneAsync(message);
            }

            return message.Id;
        }
    }
}