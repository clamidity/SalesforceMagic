using System.Net;

namespace SalesforceMagic.Configuration
{
    public class SalesforceConfig
    {
        public SalesforceSession Session { get; set; }
        public bool IsSandbox { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SecurityToken { get; set; }
        public string Environment { get; set; }
        public bool LogoutOnDisposal { get; set; }
        public bool UseSessionStore { get; set; }
        public string ApiVersion { get; set; }
        public string InstanceUrl { get; set; }
        public WebProxy Proxy { get; set; }

        /// <summary>
        /// Min: 200, Max: 2000, Default: 500.
        /// </summary>
        public int? BatchSize { get; set; }
    }
}