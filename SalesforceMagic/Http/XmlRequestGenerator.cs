using System.Xml;
using System.Xml.Serialization;
using SalesforceMagic.ORM.BaseRequestTemplates;

namespace SalesforceMagic.Http
{
    internal static class XmlRequestGenerator
    {
        internal static string GenerateRequest(XmlBody body)
        {
            return GenerateRequest(body, null);
        }

        internal static string GenerateRequest(XmlHeader header)
        {
            return GenerateRequest(null, header);
        }

        internal static string GenerateRequest(XmlBody body, XmlHeader header)
        {
            XmlDocument document = SerializeToDocument(new XmlRequest
            {
                Body = body,
                Header = header
            });

            return document.OuterXml;
        }

        public static string GenerateRequest<T>(T template)
        {
            XmlDocument document = SerializeToDocument(template);
            return document.OuterXml;
        }

        private static XmlDocument SerializeToDocument(XmlRequest request)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("env", SalesforceNamespaces.Envelope);
            namespaces.Add("n1", SalesforceNamespaces.SalesforceRequest);

            using (XmlWriter writer = doc.CreateNavigator().AppendChild())
            {
                new XmlSerializer(request.GetType()).Serialize(writer, request, namespaces);
            }

            return doc;
        }

        private static XmlDocument SerializeToDocument<T>(T request)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("env", SalesforceNamespaces.Envelope);
            namespaces.Add("n1", SalesforceNamespaces.SalesforceRequest);

            using (XmlWriter writer = doc.CreateNavigator().AppendChild())
            {
                new XmlSerializer(request.GetType()).Serialize(writer, request, namespaces);
            }

            return doc;
        }
    }
}