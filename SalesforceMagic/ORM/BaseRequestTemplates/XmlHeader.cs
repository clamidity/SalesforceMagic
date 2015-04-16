using System;
using System.Xml.Serialization;

namespace SalesforceMagic.ORM.BaseRequestTemplates
{
    [Serializable]
    public class XmlHeader
    {
        [XmlElement("SessionHeader", Namespace = SalesforceNamespaces.SalesforceRequest)]
        public SessionHeader SessionHeader { get; set; }

        [XmlElement("QueryOptions", Namespace = SalesforceNamespaces.SalesforceRequest)]
        public QueryOptionsHeader QueryOptions { get; set; }
    }
}