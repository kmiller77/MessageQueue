﻿using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KM.MessageQueue.Azure.Topic
{
    public sealed class AzureTopic<TMessage> : IMessageQueue<TMessage>
    {
        private bool _Disposed = false;
        private readonly TopicClient _TopicClient;
        private readonly AzureTopicOptions<TMessage> _Options;
        private readonly IMessageFormatter<TMessage> _Formatter;

        private static readonly MessageAttributes _EmptyAttributes = new MessageAttributes();

        public AzureTopic(IOptions<AzureTopicOptions<TMessage>> options, IMessageFormatter<TMessage> formatter)
        {
            this._Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            this._Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

            var builder = new ServiceBusConnectionStringBuilder()
            {
                Endpoint = this._Options.Endpoint,
                EntityPath = this._Options.EntityPath,
                SasKey = this._Options.SharedAccessKey,
                SasKeyName = this._Options.SharedAccessKeyName,
                TransportType = this._Options.TransportType
            };

            this._TopicClient = new TopicClient(builder);
        }

        public Task PostMessageAsync(TMessage message, CancellationToken cancellationToken) => this.PostMessageAsync(message, _EmptyAttributes, cancellationToken);

        public async Task PostMessageAsync(TMessage message, MessageAttributes attributes, CancellationToken cancellationToken)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (attributes is null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            var formattedMessageBytes = this._Formatter.Format(message);

            var topicMessage = new Message()
            {
                ContentType = attributes.ContentType,
                Body = formattedMessageBytes,
                Label = attributes.Label
            };

            if (attributes.UserProperties != null)
            {
                foreach (var userProperty in attributes.UserProperties)
                {
                    topicMessage.UserProperties.Add(userProperty.Key, userProperty.Value);
                }
            }

            await this._TopicClient.SendAsync(topicMessage);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this._Disposed)
            {
                return;
            }

            if (disposing)
            {
                this._TopicClient.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }

            this._Disposed = true;
        }

        ~AzureTopic() => this.Dispose(false);

#if NETSTANDARD2_1

        // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync
        public async ValueTask DisposeAsync()
        {
            if (this._Disposed)
            {
                return;
            }

            await _TopicClient.CloseAsync().ConfigureAwait(false);
            this.Dispose(false);
            GC.SuppressFinalize(this);
        }

#endif

    }
}
