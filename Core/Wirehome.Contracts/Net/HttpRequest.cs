using System;
using System.Collections.Generic;
using System.Net.Http;

namespace HA4IoT.Net.Http
{
    public class HttpRequest
    {
        public HttpMethod Method { get; set; }
        public string Uri { get; set; }
        public string Query { get; set; }
        public Version HttpVersion { get; set; }
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public byte[] Body { get; set; }
    }
}