﻿namespace HA4IoT.Api.Cloud.Azure
{
    public class AzureCloudServiceSettings
    {
        public bool IsEnabled { get; set; }
        public string AccountId { get; set; }
        public string OutboundQueueAuthorization { get; set; }
        public string InboundQueueAuthorization { get; set; }
    }
}
