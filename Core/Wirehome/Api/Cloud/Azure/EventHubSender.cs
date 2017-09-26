﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HA4IoT.Contracts.Logging;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HA4IoT.Api.Cloud.Azure
{
    public class EventHubSender
    {
        private readonly AutoResetEvent _eventsLock = new AutoResetEvent(false);
        private readonly List<JObject> _pendingEvents = new List<JObject>();

        private readonly Uri _uri;
        private readonly string _authorization;
        private readonly ILogger _log;

        public EventHubSender(string namespaceName, string eventHubName, string publisherName, string authorization, ILogger log)
        {
            if (namespaceName == null) throw new ArgumentNullException(nameof(namespaceName));
            if (eventHubName == null) throw new ArgumentNullException(nameof(eventHubName));
            if (publisherName == null) throw new ArgumentNullException(nameof(publisherName));
            if (authorization == null) throw new ArgumentNullException(nameof(authorization));
            if (log == null) throw new ArgumentNullException(nameof(log));

            _log = log;
            _uri = new Uri($"https://{namespaceName}.servicebus.windows.net/{eventHubName}/publishers/{publisherName}/messages");
            _authorization = authorization;
        }

        public void Enable()
        {
            var task = Task.Factory.StartNew(
                async () => await ProcessPendingEventsAsync(),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            task.ConfigureAwait(false);
        }

        public void EnqueueEvent(JObject eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            lock (_pendingEvents)
            {
                _pendingEvents.Add(eventData);
            }

            _eventsLock.Set();
        }

        private async Task ProcessPendingEventsAsync()
        {
            while (true)
            {
                try
                {
                    List<JObject> pendingEvents;
                    lock (_pendingEvents)
                    {
                        pendingEvents = new List<JObject>(_pendingEvents);
                        _pendingEvents.Clear();
                    }

                    if (!pendingEvents.Any())
                    {
                        _eventsLock.WaitOne();
                        continue;
                    }

                    foreach (var pendingEvent in pendingEvents)
                    {
                        await SendToAzureEventHubAsync(pendingEvent);
                    }
                }
                catch (Exception exception)
                {
                    _log.Error(exception, "Error while processing pending EventHub events.");
                }
            }
        }

        private async Task SendToAzureEventHubAsync(JObject body)
        {
            try
            {
                using (var httpClient = CreateHttpClient())
                using (var content = CreateContent(body))
                {
                    HttpResponseMessage result = await httpClient.PostAsync(_uri, content);
                    if (result.IsSuccessStatusCode)
                    {
                        _log.Verbose("Sent event to Azure EventHub.");
                    }
                    else
                    {
                        _log.Warning($"Failed to send Azure EventHub event (Error code: {result.StatusCode}).");
                    }
                }
            }
            catch (Exception exception)
            {
                _log.Warning(exception, "Error while sending Azure EventHub event.");
            }
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", _authorization);
            return httpClient;
        }

        private StringContent CreateContent(JObject data)
        {
            var content = new StringContent(data.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/atom+xml");
            content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("type", "entry"));
            content.Headers.ContentType.CharSet = "utf-8";

            return content;
        }
    }
}
