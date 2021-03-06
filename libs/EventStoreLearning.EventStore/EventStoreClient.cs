﻿using System;
using EventStore.ClientAPI;
using EventStoreLearning.Common.Logging;
using Microsoft.Extensions.Options;
using NLog;

namespace EventStoreLearning.EventStore
{
    public class EventStoreClient : IEventStoreClient
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly EventStoreConfig _config;

        public EventStoreClient(IOptionsMonitor<EventStoreConfig> eventStoreOptions)
        {
            _config = eventStoreOptions.CurrentValue;
        }

        public IEventStoreConnection Connect()
        {
            if (_config == null)
            {
                throw new InvalidOperationException("No EventStore config found!");
            }

            if (string.IsNullOrEmpty(_config.ConnectionString))
            {
                throw new InvalidOperationException("Invalid EventStore config!");
            }

            var connection = EventStoreConnection.Create(_config.ConnectionString);
            connection.AuthenticationFailed += Connection_AuthenticationFailed;
            connection.Closed += Connection_Closed;
            connection.Connected += Connection_Connected;
            connection.Disconnected += Connection_Disconnected;
            connection.ErrorOccurred += Connection_ErrorOccurred;
            //connection.Reconnecting += Connection_Reconnecting;

            connection.ConnectAsync().Wait();

            return connection;
        }

        private void Connection_AuthenticationFailed(object sender, ClientAuthenticationFailedEventArgs e)
        {
            _logger.ErrorWithContext("The event store connection was unable to authenticate!", e);
        }

        private void Connection_Closed(object sender, ClientClosedEventArgs e)
        {
            _logger.TraceWithContext("The event store connection was closed.", e);
        }

        private void Connection_Connected(object sender, ClientConnectionEventArgs e)
        {
            _logger.TraceWithContext("The event store connection was connected.", e);
        }

        private void Connection_Disconnected(object sender, ClientConnectionEventArgs e)
        {
            _logger.TraceWithContext("The event store connection was disconnected.", e);
        }

        private void Connection_ErrorOccurred(object sender, ClientErrorEventArgs e)
        {
            _logger.WarnWithContext("An error has occured while trying to commmunicate with the event store!", e);
        }

        private void Connection_Reconnecting(object sender, ClientReconnectingEventArgs e)
        {
            _logger.TraceWithContext("The event store connection is reconnecting.", e);
        }
    }
}
