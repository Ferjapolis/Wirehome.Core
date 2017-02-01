﻿using System;

namespace HA4IoT.Networking.Http
{
    public class HttpRequestReceivedEventArgs : EventArgs
    {
        public HttpRequestReceivedEventArgs(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            Context = context;
        }

        public HttpContext Context { get; }

        public bool IsHandled { get; set; }
    }
}
