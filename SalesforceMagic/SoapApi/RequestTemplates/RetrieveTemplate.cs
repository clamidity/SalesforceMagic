using System;
using System.Xml.Serialization;
using SalesforceMagic.Extensions;
using SalesforceMagic.ORM.BaseRequestTemplates;

namespace SalesforceMagic.SoapApi.RequestTemplates
{
    [Serializable]
    public class RetrieveTemplate
    {
        [XmlIgnore]
        public Type Type { get; set; }

        [XmlElement("fieldList", Namespace = SalesforceNamespaces.SalesforceRequest)]
        public string FieldList 
        {
            get { return string.Join(", ", Type.GetPropertyNames(true)); }
            set { }  // Apparently this is needed for XML serialization...
        }

        [XmlElement("sObjectType", Namespace = SalesforceNamespaces.SalesforceRequest)]
        public string SObjectType 
        {
            get { return Type.GetName(); }
            set { }  // Apparently this is needed for XML serialization...
        }

        [XmlElement("ids", Namespace = SalesforceNamespaces.SalesforceRequest)]
        public string[] Ids { get; set; }
    }
}