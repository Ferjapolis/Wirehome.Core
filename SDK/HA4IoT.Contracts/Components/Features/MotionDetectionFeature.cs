﻿using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components.Features
{
    public class MotionDetectionFeature : IComponentFeature
    {
        public JToken Serialize()
        {
            return JToken.FromObject(null);
        }
    }
}
