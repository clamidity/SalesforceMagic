using System;
using System.Linq.Expressions;
using SalesforceMagic.Configuration;
using SalesforceMagic.Entities;
using SalesforceMagic.Http.Enums;
using SalesforceMagic.Http.Models;
using SalesforceMagic.ORM;

namespace SalesforceMagic.SoapApi
{
    internal class SoapRequestManager
    {
        internal static string DefaultApiVersion = "30.0";
        internal static string SoapUrl = "/services/Soap/";

        internal static HttpRequest GetLoginRequest(SalesforceConfig config)
        {
            string domain = !string.IsNullOrEmpty(config.InstanceUrl)
                ? config.InstanceUrl
                : config.IsSandbox ? "https://test.salesforce.com" : "https://login.salesforce.com";

            HttpRequest request = new HttpRequest
            {
                Url = GetSoapUrl(domain, config.ApiVersion),
                Body = SoapCommands.Login(config.Username, config.Password + config.SecurityToken),
                Method = RequestType.POST,
                Proxy = config.Proxy
            };
            request.Headers.Add("SOAPAction", "login");

            return request;
        }

        internal static HttpRequest GetQueryRequest<T>(Expression<Func<T, bool>> predicate, int limit, SalesforceSession session) where T : SObject
        {
            string query = QueryBuilder.GenerateQuery(predicate, limit);
            return GetQueryRequest(query, session);
        }

        internal static HttpRequest GetQueryAllRequest<T>(int limit, SalesforceSession session) where T : SObject
        {
            string query = QueryBuilder.GenerateQuery<T>(limit);
            return GetQueryRequest(query, session);
        }

        internal static HttpRequest GetQueryRequest(string query, SalesforceSession session)
        {
            HttpRequest request = new HttpRequest
            {
                Url = GetSoapUrl(session.InstanceUrl, session.ApiVersion),
                Body = SoapCommands.Query(query, session),
                Method = RequestType.POST,
                Proxy = session.Proxy
            };
            request.Headers.Add("SOAPAction", "query");

            return request;
        }

        internal static HttpRequest GetQueryMoreRequest(string queryLocator, SalesforceSession session)
        {
            HttpRequest request = new HttpRequest
            {
                Url = GetSoapUrl(session.InstanceUrl, session.ApiVersion),
                Body = SoapCommands.QueryMore(queryLocator, session),
                Method = RequestType.POST,
                Proxy = session.Proxy
            };
            request.Headers.Add("SOAPAction", "queryMore");

            return request;
        }

        internal static HttpRequest GetSearchRequest<T>(string searchQuery, string fieldType, SalesforceSession session) where T : SObject
        {
            string search = QueryBuilder.GenerateSearchQuery<T>(searchQuery, fieldType);
            return GetSearchRequest(search, session);
        }

        public static HttpRequest GetSearchRequest(string searchString, SalesforceSession session)
        {
            HttpRequest request = new HttpRequest
            {
                Url = GetSoapUrl(session.InstanceUrl, session.ApiVersion),
                Body = SoapCommands.Search(searchString, session),
                Method = RequestType.POST,
                Proxy = session.Proxy
            };
            request.Headers.Add("SOAPAction", "search");

            return request;
        }

        internal static HttpRequest GetRetrieveRequest<T>(string[] ids, SalesforceSession session) where T : SObject
        {
            string body = SoapCommands.Retrieve<T>(ids, session);
            HttpRequest request = new HttpRequest
            {
                Url = GetSoapUrl(session.InstanceUrl, session.ApiVersion),
                Body = body,
                Method = RequestType.POST,
                Proxy = session.Proxy
            };
            request.Headers.Add("SOAPAction", "retrieve");

            return request;
        }

        internal static HttpRequest GetDeleteRequest(string[] ids, SalesforceSession session)
        {
            string body = SoapCommands.Delete(ids, session);
            HttpRequest request = new HttpRequest
            {
                Url = GetSoapUrl(session.InstanceUrl, session.ApiVersion),
                Body = body,
                Method = RequestType.POST,
                Proxy = session.Proxy
            };
            request.Headers.Add("SOAPAction", "delete");

            return request;
        }

        internal static HttpRequest GetCrudRequest<T>(CrudOperation<T> operation, SalesforceSession session) where T : SObject
        {
            string body = SoapCommands.CrudOperation(operation, session);
            HttpRequest request = new HttpRequest
            {
                Url = GetSoapUrl(session.InstanceUrl, session.ApiVersion),
                Body = body,
                Method = RequestType.POST,
                Proxy = session.Proxy
            };
            request.Headers.Add("SOAPAction", operation.OperationType.ToString().ToLower());

            return request;
        }

        internal static HttpRequest GetCountRequest<T>(SalesforceSession session, Expression<Func<T, bool>> predicate = null)
        {
            string query = QueryBuilder.GenerateCountyQuery(predicate);
            return GetQueryRequest(query, session);
        }

        private static string GetSoapUrl(string instance, string apiVersion)
        {
	        return string.Format(
		        "{0}{1}{3}u/{2}",
		        instance,
		        SoapUrl,
		        !string.IsNullOrEmpty(apiVersion)
			        ? apiVersion
			        : DefaultApiVersion,
		        SoapUrl.EndsWith("/") ? "" : "/");
        }
    }
}