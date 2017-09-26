﻿using System;
using System.Threading.Tasks;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.ExternalServices.Twitter;

namespace Wirehome.ExternalServices.Twitter
{
    public class TweetAction : IAction
    {
        private readonly Func<string> _messageProvider;
        private readonly ITwitterClientService _twitterService;

        public TweetAction(Func<string> messageProvider, ITwitterClientService twitterService)
        {
            _messageProvider = messageProvider ?? throw new ArgumentNullException(nameof(messageProvider));
            _twitterService = twitterService ?? throw new ArgumentNullException(nameof(twitterService));
        }

        public TweetAction(string message, ITwitterClientService twitterService)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            _messageProvider = () => message;
            _twitterService = twitterService ?? throw new ArgumentNullException(nameof(twitterService));
        }

        public void Execute()
        {
            Task.Run(() => _twitterService.TryTweet(_messageProvider()));
        }
    }
}
