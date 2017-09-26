﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HA4IoT.Contracts.ExternalServices.Twitter;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Settings;
using HA4IoT.Contracts.Cryptographic;

namespace HA4IoT.ExternalServices.Twitter
{
    public class TwitterClientService : ServiceBase, ITwitterClientService
    {
        private readonly ILogger _log;

        private string _nonce;
        private string _timestamp;
        private readonly ICryptoService _cryptoService;

        public TwitterClientService(ISettingsService settingsService, ILogService logService, IScriptingService scriptingService, ICryptoService cryptoService)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (logService == null) throw new ArgumentNullException(nameof(logService));
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));

            settingsService.CreateSettingsMonitor<TwitterClientServiceSettings>(s => Settings = s.NewSettings);

            _log = logService.CreatePublisher(nameof(TwitterClientService));

            scriptingService.RegisterScriptProxy(s => new TwitterClientScriptProxy(this));
            this._cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
        }

        public TwitterClientServiceSettings Settings { get; private set; }

        public async Task<bool> TryTweet(string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            if (!Settings.IsEnabled)
            {
                _log.Verbose("Twitter client service is disabled.");
                return false;
            }

            try
            {
                _log.Verbose("Trying to tweet '" + message + "'.");
                
                _nonce = GetNonce();
                _timestamp = GetTimeStamp();

                var signature = GetSignatureForRequest(message);
                var oAuthToken = GetAuthorizationToken(signature);

                using (var httpClient = new HttpClient())
                {
                    var url = "https://api.twitter.com/1.1/statuses/update.json?status=" + Uri.EscapeDataString(message);

                    var request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Headers.Add("Authorization", oAuthToken);

                    var response = await httpClient.SendAsync(request);

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new HttpRequestException(response.StatusCode.ToString());
                    }

                    _log.Info("Tweet successfully sent.");
                    return true;
                }
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error while trying to send tweet.");
                return false;
            }
        }

        private string GetSignatureForRequest(string message)
        {
            var parameters = new List<string>
            {
                $"oauth_consumer_key={Uri.EscapeDataString(Settings.ConsumerKey)}",
                $"oauth_nonce={_nonce}",
                "oauth_signature_method=HMAC-SHA1",
                $"oauth_timestamp={_timestamp}",
                $"oauth_token={Uri.EscapeDataString(Settings.AccessToken)}",
                "oauth_version=1.0",
                $"status={Uri.EscapeDataString(message)}"
            };

            var parametersString = Uri.EscapeDataString(string.Join("&", parameters));
            var url = Uri.EscapeDataString("https://api.twitter.com/1.1/statuses/update.json");

            var signingContent = string.Format(
                "{0}&{1}&{2}",
                "POST",
                url,
                parametersString);

            var key = Uri.EscapeDataString(Settings.ConsumerSecret) + "&" + Uri.EscapeDataString(Settings.AccessTokenSecret);

            return _cryptoService.GenerateSignature(key, signingContent);
        }

        private string GetAuthorizationToken(string signature)
        {
            var values = new List<string>
            {
                $"oauth_consumer_key=\"{Uri.EscapeDataString(Settings.ConsumerKey)}\"",
                $"oauth_nonce=\"{_nonce}\"",
                $"oauth_signature=\"{Uri.EscapeDataString(signature)}\"",
                "oauth_signature_method=\"HMAC-SHA1\"",
                $"oauth_timestamp=\"{_timestamp}\"",
                $"oauth_token=\"{Uri.EscapeDataString(Settings.AccessToken)}\"",
                "oauth_version=\"1.0\""
            };

            return "OAuth " + string.Join(", ", values);
        }

        private string GetNonce()
        {
            return Uri.EscapeDataString(Guid.NewGuid().ToString());
        }

        private string GetTimeStamp()
        {
            var sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Math.Round(sinceEpoch.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        
    }
}
