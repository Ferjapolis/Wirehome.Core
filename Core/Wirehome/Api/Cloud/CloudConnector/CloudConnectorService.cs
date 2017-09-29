﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Api.Cloud;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Settings;
using Newtonsoft.Json;

namespace Wirehome.Api.Cloud.CloudConnector
{
    public class CloudConnectorService : ServiceBase, IApiAdapter
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly StringContent _emptyContent = new StringContent(string.Empty);
        private string _receiveRequestsUri;
        private string _sendResponseUri;

        private CloudConnectorServiceSettings _settings;
        private readonly IApiDispatcherService _apiDispatcherService;
        private readonly ILogger _log;

        private bool _isConnected;
        private readonly ISettingsService _settingsService;
        private readonly ISystemInformationService _systemInformationService;

        public CloudConnectorService(IApiDispatcherService apiDispatcherService, ISettingsService settingsService, ISystemInformationService systemInformationService, ILogService logService)
        {
            _log = logService?.CreatePublisher(nameof(CloudConnectorService)) ?? throw new ArgumentNullException(nameof(logService));
            _apiDispatcherService = apiDispatcherService ?? throw new ArgumentNullException(nameof(apiDispatcherService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _systemInformationService = systemInformationService ?? throw new ArgumentNullException(nameof(systemInformationService));
        }

        public event EventHandler<ApiRequestReceivedEventArgs> ApiRequestReceived;

        public override Task Initialize()
        {
            //TODO moved to INIT
            _settings = _settingsService?.GetSettings<CloudConnectorServiceSettings>();

            _receiveRequestsUri = $"{_settings.ServerAddress}/api/ControllerProxy/ReceiveRequests";
            _sendResponseUri = $"{_settings.ServerAddress}/api/ControllerProxy/SendResponse";

            _systemInformationService.Set("CloudConnector/IsConnected", () => _isConnected);
            //


            if (!_settings.IsEnabled)
            {
                _log.Info("Cloud Connector service is disabled.");
                return Task.CompletedTask;
            }

            _log.Info("Starting Cloud Connector service.");

            _apiDispatcherService.RegisterAdapter(this);

            Task.Run(ReceivePendingMessagesAsyncLoop, _cancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        public void NotifyStateChanged(IComponent component)
        {
        }

        private async Task ReceivePendingMessagesAsyncLoop()
        {
            _log.Verbose("Starting receiving pending Cloud messages.");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = CreateAuthorizationHeader();
                httpClient.Timeout = TimeSpan.FromMinutes(1.25);
                
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        var response = await ReceivePendingMessagesAsync(httpClient, _cancellationTokenSource.Token);
                        _isConnected = response.Succeeded;

                        if (response.Succeeded && !string.IsNullOrEmpty(response.Response))
                        {
                            Task.Run(() => ProcessPendingCloudMessages(response.Response)).Forget();
                        }
                    }
                    catch (Exception exception)
                    {
                        _log.Error(exception, "Error while receiving pending Cloud messages.");
                        _isConnected = false;

                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }
            }
        }

        private async Task<ReceivePendingMessagesAsyncResult> ReceivePendingMessagesAsync(HttpClient httpClient, CancellationToken cancellationToken)
        {
            HttpResponseMessage result;
            try
            {
                result = await httpClient.PostAsync(_receiveRequestsUri, _emptyContent, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                return new ReceivePendingMessagesAsyncResult();
            }

            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content))
                {
                    return new ReceivePendingMessagesAsyncResult();
                }

                return new ReceivePendingMessagesAsyncResult
                {
                    Succeeded = true,
                    Response = content
                };
            }

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                _log.Warning("Credentials for Cloud access are invalid.");
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                _log.Warning("Cloud access is not working properly.");
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
            else
            {
                _log.Warning($"Failed to receive pending Cloud message (Error code: {result.StatusCode}).");
            }

            return new ReceivePendingMessagesAsyncResult();
        }

        private async Task ProcessPendingCloudMessages(string content)
        {
            try
            {
                var pendingCloudMessages = JsonConvert.DeserializeObject<List<CloudRequestMessage>>(content);
                if (pendingCloudMessages == null)
                {
                    return;
                }

                foreach (var cloudMessage in pendingCloudMessages)
                {
                    var eventArgs = ProcessCloudMessage(cloudMessage);
                    await SendResponse(eventArgs);
                }

                _log.Verbose($"Handled {pendingCloudMessages.Count} pending Cloud messages.");
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Unhandled exception while processing cloud messages. " + exception.Message);
            }
        }

        private async Task SendResponse(CloudConnectorApiContext apiCall)
        {
            try
            {
                using (var httpClient = new HttpClient())
                using (var content = CreateContent(apiCall))
                {
                    httpClient.DefaultRequestHeaders.Authorization = CreateAuthorizationHeader();

                    var result = await httpClient.PostAsync(_sendResponseUri, content);
                    if (result.IsSuccessStatusCode)
                    {
                        _log.Verbose("Sent response message to Cloud.");
                    }
                    else
                    {
                        _log.Warning($"Failed to send response message to Cloud (Error code: {result.StatusCode}).");
                    }
                }
            }
            catch (Exception exception)
            {
                _log.Warning(exception, "Error while sending response message to cloud.");
            }
        }

        private CloudConnectorApiContext ProcessCloudMessage(CloudRequestMessage cloudMessage)
        {
            var apiCall = new CloudConnectorApiContext(cloudMessage);
            var eventArgs = new ApiRequestReceivedEventArgs(apiCall);

            ApiRequestReceived?.Invoke(this, eventArgs);

            if (!eventArgs.IsHandled)
            {
                apiCall.ResultCode = ApiResultCode.ActionNotSupported;
            }

            return apiCall;
        }

        private static StringContent CreateContent(CloudConnectorApiContext apiCall)
        {
            var cloudMessage = new CloudResponseMessage();
            cloudMessage.Header.CorrelationId = apiCall.RequestMessage.Header.CorrelationId;
            cloudMessage.Response.ResultCode = apiCall.ResultCode;
            cloudMessage.Response.Result = apiCall.Result;

            var stringContent = new StringContent(JsonConvert.SerializeObject(cloudMessage));
            stringContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            stringContent.Headers.ContentEncoding.Add("utf-8");

            return stringContent;
        }

        private AuthenticationHeaderValue CreateAuthorizationHeader()
        {
            var value = $"{_settings.ControllerId}:{_settings.ApiKey}";
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
        }
    }
}
