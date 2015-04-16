using System.Collections.Generic;
using System.Net;
using SalesforceMagic.Http.Enums;

namespace SalesforceMagic.Http.Models
{
    internal class HttpRequest
    {
        internal HttpRequest()
        {
            Method = RequestType.POST;
            ContentType = "text/xml";
            Headers = new Dictionary<string, string>();
        }

        internal string Url { get; set; }
        internal RequestType Method { get; set; }
        internal Dictionary<string, string> Headers { get; set; }
        internal string Body { get; set; }
        internal string ContentType { get; set; }

        internal WebProxy Proxy { get; set; }

        internal bool IsValid
        {
            get
            {
                bool success = Url != null;

                if (Method != RequestType.GET && Body == null)
                    success =  false;

                return success;
            }
        }
    }
}