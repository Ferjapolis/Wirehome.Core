﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Wirehome.Contracts.ExternalServices.Twitter;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Scripting;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Settings;

namespace Wirehome.ExternalServices.Twitter
{
    public class TwitterClientService : ServiceBase, ITwitterClientService
    {
        private readonly ILogger _log;

        private string _nonce;
        private string _timestamp;
        private readonly ISettingsService _settingsService;

        public TwitterClientService(ISettingsService settingsService, ILogService logService, IScriptingService scriptingService)
        {
            if (logService == null) throw new ArgumentNullException(nameof(logService));
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            
            _log = logService.CreatePublisher(nameof(TwitterClientService));

            scriptingService.RegisterScriptProxy(s => new TwitterClientScriptProxy(this));   
        }

        public override Task Initialize()
        {
            //TODO Moved to Init
            _settingsService.CreateSettingsMonitor<TwitterClientServiceSettings>(s => Settings = s.NewSettings);

            return Task.CompletedTask;
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

            return GenerateSignature(key, signingContent);
        }

        private string GenerateSignature(string key, string content)
        {
            var hmac = new System.Security.Cryptography.HMACSHA1()
            {
                Key = Encoding.UTF8.GetBytes(key)
            };

            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(content)));
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
