using System.Xml.Serialization;
using SalesforceMagic.SoapApi.RequestTemplates;

namespace SalesforceMagic.ORM.BaseRequestTemplates
{
    public partial class XmlBody
    {
        [XmlElement("login", Namespace = SalesforceNamespaces.SalesforceRequest)]
        public LoginRequestTemplate LoginTemplate { get; set; }

        [XmlElement("query", Namespace = SalesforceNamespaces.SalesforceRequest)]
        public QueryTemplate QueryTemplate { get; set; }

        [XmlElement("queryMore", Namespace = SalesforceNamespaces.SalesforceRequest)]
        public QueryMoreTemplate QueryMoreTemplate { get; set; }

        [XmlElement("create", Namespace = SalesforceNamespaces.SalesforceRequest)]
        public BasicCrudTemplate InsertTemplate { get; set; }

        [XmlElement("upsert", Namespace = SalesforceNamespaces.SalesforceRequest)]
        public UpsertTemplate UpsertTemplate { get; set; }

        [XmlElement("update", Namespace = SalesforceNamespaces.SalesforceRequest)]
        public BasicCrudTemplate UpdateTemplate { get; set; }

        [XmlElement("delete", Namespace = SalesforceNamespaces.SalesforceRequest)]
        public DeleteTemplate DeleteTemplate { get; set; }

        [XmlElement("retrieve", Namespace = SalesforceNamespaces.SalesforceRequest)]
        public RetrieveTemplate RetrieveTemplate { get; set; }
    }
}