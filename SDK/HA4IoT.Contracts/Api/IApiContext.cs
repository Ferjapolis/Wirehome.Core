﻿using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Api
{
    public interface IApiContext
    {
        string Action { get; }
        JObject Request { get; }
        ApiResultCode ResultCode { get; set; }
        JObject Response { get; set; }
        bool UseHash { get; set; }
    }
}
