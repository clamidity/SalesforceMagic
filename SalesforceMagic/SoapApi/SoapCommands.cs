using SalesforceMagic.Configuration;
using SalesforceMagic.Entities;
using SalesforceMagic.Http;
using SalesforceMagic.ORM.BaseRequestTemplates;
using SalesforceMagic.SoapApi.Enum;
using SalesforceMagic.SoapApi.RequestTemplates;

namespace SalesforceMagic.SoapApi
{
    internal static class SoapCommands
    {
        internal static string Login(string username, string password)
        {
            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                LoginTemplate = new LoginRequestTemplate(username, password)
            });
        }

        internal static string Query(string query, SalesforceSession session)
        {
            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                QueryTemplate = new QueryTemplate(query)
            }, BuildXmlHeader(session));
        }

        internal static string Retrieve<T>(string[] ids, SalesforceSession session) where T : SObject
        {
            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                RetrieveTemplate = new RetrieveTemplate
                {
                    Type = typeof(T),
                    Ids = ids
                }
            }, BuildXmlHeader(session));
        }

        public static string QueryMore(string queryLocator, SalesforceSession session)
        {
            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                QueryMoreTemplate = new QueryMoreTemplate(queryLocator)
            }, BuildXmlHeader(session));
        }

        public static string CrudOperation<T>(CrudOperation<T> operation, SalesforceSession session) where T : SObject
        {
            XmlBody body = GetCrudBody(operation);
            return XmlRequestGenerator.GenerateRequest(body, BuildXmlHeader(session));
        }

        private static XmlBody GetCrudBody<T>(CrudOperation<T> operation) where T : SObject
        {
            XmlBody body = new XmlBody();

            switch (operation.OperationType)
            {
                case CrudOperations.Insert:
                    body.InsertTemplate = new BasicCrudTemplate
                    {
                        SObjects = operation.Items
                    };
                    break;
                case CrudOperations.Upsert:
                    body.UpsertTemplate = new UpsertTemplate
                    {
                        SObjects = operation.Items,
                        ExternalIdFieldName = operation.ExternalIdField
                    };
                    break;
                case CrudOperations.Update:
                    body.UpdateTemplate = new BasicCrudTemplate
                    {
                        SObjects = operation.Items
                    };
                    break;
                case CrudOperations.Delete:
                    body.DeleteTemplate = new DeleteTemplate
                    {
                        SObjects = operation.Items
                    };
                    break;
            }

            return body;
        }

        private static XmlHeader BuildXmlHeader(SalesforceSession session)
        {
            var header = new XmlHeader
            {
                SessionHeader = new SessionHeader
                {
                    SessionId = session.SessionId
                }
            };

            if (session.BatchSize.HasValue)
            {
                header.QueryOptions = new QueryOptionsHeader
                {
                    BatchSize = session.BatchSize.Value
                };
            }

            return header;
        }
    }
}
