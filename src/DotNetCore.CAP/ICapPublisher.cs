﻿// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Data;
using System.Threading.Tasks;
using DotNetCore.CAP.Abstractions;

namespace DotNetCore.CAP
{
    /// <summary>
    /// A publish service for publish a message to CAP.
    /// </summary>
    public interface ICapPublisher
    {
        /// <summary>
        /// (EntityFramework) Asynchronous publish an object message.
        /// <para>
        /// If you are using the EntityFramework, you need to configure the DbContextType first.
        /// otherwise you need to use overloaded method with  IDbTransaction.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of content object.</typeparam>
        /// <param name="name">the topic name or exchange router key.</param>
        /// <param name="contentObj">message body content, that will be serialized of json.</param>
        /// <param name="callbackName">callback subscriber name</param>
        Task PublishAsync<T>(string name, T contentObj, string callbackName = null);

        /// <summary>
        /// (EntityFramework) Publish an object message.
        /// <para>
        /// If you are using the EntityFramework, you need to configure the DbContextType first.
        /// otherwise you need to use overloaded method with IDbTransaction.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of content object.</typeparam>
        /// <param name="name">the topic name or exchange router key.</param>
        /// <param name="contentObj">message body content, that will be serialized of json.</param>
        /// <param name="callbackName">callback subscriber name</param>
        void Publish<T>(string name, T contentObj, string callbackName = null);

        /// <summary>
        /// (ado.net) Asynchronous publish an object message.
        /// </summary>
        /// <param name="name">the topic name or exchange router key.</param>
        /// <param name="contentObj">message body content, that will be serialized of json.</param>
        /// <param name="dbTransaction">the transaction of <see cref="IDbTransaction" /></param>
        /// <param name="callbackName">callback subscriber name</param>
        Task PublishAsync<T>(string name, T contentObj, IDbTransaction dbTransaction, string callbackName = null);

        /// <summary>
        /// (ado.net) Publish an object message.
        /// </summary>
        /// <param name="name">the topic name or exchange router key.</param>
        /// <param name="contentObj">message body content, that will be serialized of json.</param>
        /// <param name="dbTransaction">the transaction of <see cref="IDbTransaction" /></param>
        /// <param name="callbackName">callback subscriber name</param>
        void Publish<T>(string name, T contentObj, IDbTransaction dbTransaction, string callbackName = null);

        /// <summary>
        /// Publish an object message with mongo.
        /// </summary>
        /// <param name="name">the topic name or exchange router key.</param>
        /// <param name="contentObj">message body content, that will be serialized of json.</param>
        /// <param name="mongoTransaction">if transaction was set null, the message will be published directly.</param>
        /// <param name="callbackName">callback subscriber name</param>
        void PublishWithMongo<T>(string name, T contentObj, IMongoTransaction mongoTransaction = null, string callbackName = null);

        /// <summary>
        ///  Asynchronous publish an object message with mongo.
        /// </summary>
        /// <param name="name">the topic name or exchange router key.</param>
        /// <param name="contentObj">message body content, that will be serialized of json.</param>
        /// <param name="mongoTransaction">if transaction was set null, the message will be published directly.</param>
        /// <param name="callbackName">callback subscriber name</param>
        Task PublishWithMongoAsync<T>(string name, T contentObj, IMongoTransaction mongoTransaction = null, string callbackName = null);
    }
}