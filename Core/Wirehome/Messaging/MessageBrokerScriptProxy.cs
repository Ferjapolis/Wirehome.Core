﻿using System;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Scripting;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace Wirehome.Messaging
{
    public class MessageBrokerScriptProxy : IScriptProxy
    {
        private readonly IMessageBrokerService _messageBrokerService;
        private readonly IScriptingSession _scriptingSession;

        [MoonSharpHidden]
        public MessageBrokerScriptProxy(IMessageBrokerService messageBrokerService, IScriptingSession scriptingSession)
        {
            _messageBrokerService = messageBrokerService ?? throw new ArgumentNullException(nameof(messageBrokerService));
            _scriptingSession = scriptingSession ?? throw new ArgumentNullException(nameof(scriptingSession));
        }

        [MoonSharpHidden]
        public string Name => "messageBroker";

        public void Subscribe(string id, string topic, string payloadType, string callbackFunctionName)
        {
            var messageSubscription = new MessageSubscription
            {
                Id = id,
                Topic = topic,
                PayloadType = payloadType,
                Callback = m => _scriptingSession.Execute(callbackFunctionName)
            };

            _messageBrokerService.Subscribe(messageSubscription);
        }

        public string[] GetSubscriptions()
        {
            //ashdajshdjsahd
            return new string[0];
        }

        public void Unsubscribe(string uid)
        {
            _messageBrokerService.Unsubscribe(uid);
        }

        public void Publish(string topic, string type, string payload)
        {
            _messageBrokerService.Publish(new Message<JObject>(topic, new MessagePayload<JObject>(type, JObject.Parse(payload))));
        }
    }
}
