﻿namespace Wirehome.Contracts.Api.Cloud
{
    public class CloudRequestMessage
    {
        public CloudMessageHeader Header { get; } = new CloudMessageHeader();

        public ApiRequest Request { get; } = new ApiRequest();
    }
}
