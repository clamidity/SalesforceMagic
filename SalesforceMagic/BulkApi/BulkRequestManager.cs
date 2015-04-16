using System;
using SalesforceMagic.BulkApi.Configuration;
using SalesforceMagic.Configuration;
using SalesforceMagic.Entities;
using SalesforceMagic.Extensions;
using SalesforceMagic.Http.Enums;
using SalesforceMagic.Http.Models;

namespace SalesforceMagic.BulkApi
{
    internal static class BulkRequestManager
    {
        internal static string DefaultApiVersion = "30.0";
        internal static string BulkApiUrl = "/services/async/";

        public static HttpRequest GetStartJobRequest<T>(JobConfig config, SalesforceSession session)
        {
            HttpRequest request = new HttpRequest
            {
                Url = GetBulkUrl(session.InstanceUrl, session.ApiVersion),
                Body = BulkCommands.CreateJob(config, typeof(T).GetName()),
                Method = RequestType.POST,
                ContentType = "application/xml"
            };
            request.Headers.Add("X-SFDC-Session", session.SessionId);

            return request;
        }

        public static HttpRequest GetCloseJobRequest(string jobId, SalesforceSession session)
        {
            HttpRequest request = new HttpRequest
            {
                Url = GetBulkUrl(session.InstanceUrl, session.ApiVersion) + "/" + jobId,
                Body = BulkCommands.CloseJob(),
                Method = RequestType.POST,
                ContentType = "application/xml"
            };
            request.Headers.Add("X-SFDC-Session", session.SessionId);

            return request;
        }

        public static HttpRequest GetQueryJobRequest(string jobId, SalesforceSession session)
        {
            HttpRequest request = new HttpRequest
            {
                Url = GetBulkUrl(session.InstanceUrl, session.ApiVersion) + "/" + jobId,
                Body = null,
                Method = RequestType.GET,
                ContentType = "application/xml"
            };
            request.Headers.Add("X-SFDC-Session", session.SessionId);

            return request;
        }

        public static HttpRequest GetBatchRequest<T>(T[] items, string jobId, SalesforceSession session) where T : SObject
        {
            HttpRequest request = new HttpRequest
            {
                Url = GetBulkUrl(session.InstanceUrl, session.ApiVersion) + "/" + jobId + "/batch",
                Body = BulkCommands.CreateBatch(items),
                Method = RequestType.POST,
                ContentType = "text/csv; charset=UTF-8"
            };
            request.Headers.Add("X-SFDC-Session", session.SessionId);

            return request;
        }

        private static string GetBulkUrl(string instanceUrl, string apiVersion)
        {
            return string.Format("{0}{1}/{2}/job", instanceUrl, BulkApiUrl, !string.IsNullOrEmpty(apiVersion) 
                ? apiVersion 
                : DefaultApiVersion);
        }
    }
}