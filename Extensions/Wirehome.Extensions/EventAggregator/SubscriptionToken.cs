﻿using System;

namespace Wirehome.Extensions.Messaging.Core
{
    public class SubscriptionToken : IDisposable
    {
        private readonly IEventAggregator _eventAggregator;

        public SubscriptionToken(Guid token, IEventAggregator eventAggregator)
        {
            this.Token = token;
            this._eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        public Guid Token { get; }

        public void Dispose()
        {
            _eventAggregator.UnSubscribe(Token);
        }
    }
}