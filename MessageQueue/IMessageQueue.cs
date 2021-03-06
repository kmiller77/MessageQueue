﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace KM.MessageQueue
{
    public interface IMessageQueue<TMessage> : IDisposable
#if !NETSTANDARD2_0
        , IAsyncDisposable
#endif
    {
        Task PostMessageAsync(TMessage message, CancellationToken cancellationToken);
        Task PostMessageAsync(TMessage message, MessageAttributes attributes, CancellationToken cancellationToken);

        Task<IMessageReader<TMessage>> GetReaderAsync(CancellationToken cancellationToken);
    }
}
