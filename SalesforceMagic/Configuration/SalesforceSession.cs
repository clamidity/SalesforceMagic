using System;
using System.Net;

namespace SalesforceMagic.Configuration
{
    public class SalesforceSession
    {
        public string SessionId { get; set; }
        public string InstanceUrl { get; set; }
        public string Environment { get; set; }
        public bool IsSandbox { get; set; }
        public string ApiVersion { get; set; }
        public DateTime LastLogin { get; set; }
        public WebProxy Proxy { get; set; }

        /// <summary>
        /// Min: 200, Max: 2000, Default: 500.
        /// </summary>
        public int? BatchSize { get; set; }
    }
}