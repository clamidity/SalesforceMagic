﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using SalesforceMagic.Abstract;
using SalesforceMagic.BulkApi;
using SalesforceMagic.BulkApi.Configuration;
using SalesforceMagic.BulkApi.Models;
using SalesforceMagic.Configuration;
using SalesforceMagic.Configuration.Abstract;
using SalesforceMagic.Entities;
using SalesforceMagic.Exceptions;
using SalesforceMagic.Extensions;
using SalesforceMagic.Http;
using SalesforceMagic.Http.Models;
using SalesforceMagic.Http.ResponseModels;
using SalesforceMagic.ORM;
using SalesforceMagic.SoapApi;
using SalesforceMagic.SoapApi.Enum;

namespace SalesforceMagic
{
    // TODO: Configure for multiple environments
    // TODO: Add ability to set or pass in session id
    // TODO: Map responses in a more meaningful manner
    public class SalesforceClient : ISalesforceClient
    {
        #region Private Fields
        private readonly ISessionStoreProvider _sessionStore;
        private SalesforceConfig _config;
        private static readonly object Lock = new object();
        #endregion

        public SalesforceClient(ISessionStoreProvider sessionStore, SalesforceConfig config, bool login = false)
        {
            _sessionStore = sessionStore;
            _config = config;
            if (login) Login();
        }

        public SalesforceClient(SalesforceConfig config, bool login = false)
        {
            _sessionStore = new MemoryCacheProvider();
            _config = config;
            if (login) Login();
        }

        public virtual void ChangeEnvironment(SalesforceConfig config, bool login = false)
        {
            _config = config;
            if (login) Login();
        }

        public virtual string GetSessionId()
        {
            return Login().SessionId;
        }

        public virtual SalesforceSession GetSession()
        {
            return Login();
        }

        public SalesforceSession Login()
        {
            lock (Lock)
            {
                SalesforceSession session;
                if (_config.UseSessionStore)
                {
                    session = _sessionStore.RetrieveSession(_config.Environment ?? "Default");
                    if (session != null) return session;
                }
            
                if (_config.Session != null) return _config.Session;

                using (HttpClient httpClient = new HttpClient())
                {
                    XmlDocument response = httpClient.PerformRequest(SoapRequestManager.GetLoginRequest(_config));
                    SimpleLogin result = ResponseReader.ReadGenericResponse<SimpleLogin>(response, "result");

                    Uri instanceUrl = new Uri(result.ServerUrl);
                    session = new SalesforceSession
                    {
                        Environment = _config.Environment,
                        ApiVersion = _config.ApiVersion,
                        IsSandbox = _config.IsSandbox,
                        LastLogin = DateTime.Now,
                        InstanceUrl = instanceUrl.Scheme + "://" + instanceUrl.Host,
                        SessionId = result.SessionId,
                        Proxy = _config.Proxy,
                        BatchSize = _config.BatchSize,
                    };

                    if (_config.UseSessionStore) _sessionStore.StoreSession(session);
                    _config.Session = session;

                    return session;
                }
            }
        }

        /// <summary>
        ///     Retrieve an object total count.
        ///      - Allows for filtering count totals 
        ///        using the conditional lambda.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public int Count<T>(Expression<Func<T, bool>> predicate = null) where T : SObject
        {
            var request = SoapRequestManager.GetCountRequest(Login(), predicate);
            return PerformCountRequest(request);
        }

        /// <summary>
        ///     Simple Query
        ///      - Query items based on generic object
        ///      - Generate query using predicate
        ///      - Limited by 200 records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> Query<T>(Expression<Func<T, bool>> predicate, int limit = 0) where T : SObject
        {
            return PerformArrayRequest<T>(SoapRequestManager.GetQueryRequest(predicate, limit, Login()));
        }

        /// <summary>
        ///     Simple Query
        ///      - Query items based on generic object
        ///      - Limited by 200 records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="limit"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> Query<T>(int limit = 0) where T : SObject
        {
            return PerformArrayRequest<T>(SoapRequestManager.GetQueryAllRequest<T>(limit, Login()));
        }

        /// <summary>
        ///     Simple Query
        ///      - Query items based on generic object
        ///      - Utilize included raw query
        ///      - Limited by 200 records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> Query<T>(string query) where T : SObject
        {
            // TODO: Validate query
            return PerformArrayRequest<T>(SoapRequestManager.GetQueryRequest(query, Login()));
        }

        /// <summary>
        ///     Advanced Query
        ///      - Query items based on generic object
        ///      - Returns query locator, and done status which
        ///        can be used to bypass the 200 record limit.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="limit"></param>
        /// <returns></returns>
        public QueryResult<T> AdvancedQuery<T>(int limit = 0) where T : SObject
        {
            return PerformQueryRequest<T>(SoapRequestManager.GetQueryAllRequest<T>(limit, Login()));
        }

        /// <summary>
        ///     Advanced Query
        ///      - Query items based on generic object
        ///      - Generate query using predicate
        ///      - Returns query locator, and done status which
        ///        can be used to bypass the 200 record limit.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public QueryResult<T> AdvancedQuery<T>(Expression<Func<T, bool>> predicate, int limit = 0) where T : SObject
        {
            return PerformQueryRequest<T>(SoapRequestManager.GetQueryRequest(predicate, limit, Login()));
        }

        /// <summary>
        ///     Advanced Query
        ///      - Query items based on generic object
        ///      - Utilize included raw query
        ///      - Returns query locator, and done status which
        ///        can be used to bypass the 200 record limit.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryResult<T> AdvancedQuery<T>(string query) where T : SObject
        {
            return PerformQueryRequest<T>(SoapRequestManager.GetQueryRequest(query, Login()));
        }

        /// <summary>
        ///     Query More
        ///      - Used to retrieve the next set of records
        ///        available in a query using the queryLocator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryLocator"></param>
        /// <returns></returns>
        public QueryResult<T> QueryMore<T>(string queryLocator) where T : SObject
        {
            return PerformQueryRequest<T>(SoapRequestManager.GetQueryMoreRequest(queryLocator, Login()));
        }

        /// <summary>
        ///     Query Single
        ///      - Used to retrieve a single record
        ///        using filter criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual T QuerySingle<T>(Expression<Func<T, bool>> predicate) where T : SObject
        {
            return Query(predicate).FirstOrDefault();
        }

        /// <summary>
        ///     Query Single
        ///      - Used to retrieve a single record
        ///        using a raw string query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual T QuerySingle<T>(string query) where T : SObject
        {
            return Query<T>(query).FirstOrDefault();
        }

        /// <summary>
        ///     Search
        ///      - Used for searching on all field types of a single sObject type
        /// </summary>
        /// <typeparam name="T">The sObject type to search.</typeparam>
        /// <param name="searchQuery">The search query to search with. See http://www.salesforce.com/us/developer/docs/soql_sosl/Content/sforce_api_calls_sosl_find.htm for more information.</param>
        /// <returns></returns>
        public IEnumerable<T> Search<T>(string searchQuery) where T : SObject
        {
            return PerformSearchRequest<T>(SoapRequestManager.GetSearchRequest<T>(searchQuery, "ALL", Login()));
        }

        /// <summary>
        ///     Search
        ///      - Used for searching on fields of a given type of a single sObject type
        /// </summary>
        /// <typeparam name="T">The sObject type to search.</typeparam>
        /// <param name="searchQuery">The search query to search with. See http://www.salesforce.com/us/developer/docs/soql_sosl/Content/sforce_api_calls_sosl_find.htm for more information.</param>
        /// <param name="fieldType">The field type. See http://www.salesforce.com/us/developer/docs/soql_sosl/Content/sforce_api_calls_sosl_in.htm for more information.</param>
        /// <returns></returns>
        public IEnumerable<T> Search<T>(string searchQuery, string fieldType) where T : SObject
        {
            return PerformSearchRequest<T>(SoapRequestManager.GetSearchRequest<T>(searchQuery, fieldType, Login()));
        }

        public virtual SalesforceResponse Crud<T>(CrudOperation<T> operation) where T : SObject
        {
            if (operation.Items.Count() > 200)
                throw new SalesforceRequestException("The SOAP api only allows the modification of up tp 200 records. " +
                                                     "Please use the Bulk api methods for any operation that requires " +
                                                     "higher limits.");

            if (operation.OperationType == CrudOperations.Upsert && string.IsNullOrEmpty(operation.ExternalIdField))
                throw new SalesforceRequestException("Upsert requests required an external ID name field to be specified.");

            return PerformSimpleRequest(SoapRequestManager.GetCrudRequest(operation, Login()));
        }

        /// <summary>
        ///     Retrieve
        ///      - Used to retrieve a single record by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">the Id of the record to retrieve</param>
        /// <returns></returns>
        public virtual T Retrieve<T>(string id) where T : SObject
        {
            return Retrieve<T>(new[] { id }).FirstOrDefault();
        }

        /// <summary>
        ///     Retrieve
        ///      - Used to retrieve multiple records by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids">the ids of the records to retrieve</param>
        /// <returns></returns>
        public virtual IEnumerable<T> Retrieve<T>(string[] ids) where T : SObject
        {
            return PerformRetrieveRequest<T>(SoapRequestManager.GetRetrieveRequest<T>(ids, Login()));
        }

        public virtual SalesforceResponse Insert<T>(IEnumerable<T> items) where T : SObject
        {
            return Crud(new CrudOperation<T>
            {
                Items = items,
                OperationType = CrudOperations.Insert
            });
        }

        public virtual SalesforceResponse Insert<T>(T item) where T : SObject
        {
            return Insert<T>(new[] { item });
        }

        public virtual SalesforceResponse Upsert<T>(IEnumerable<T> items, string externalIdField) where T : SObject
        {
            return Crud(new CrudOperation<T>
            {
                Items = items,
                OperationType = CrudOperations.Upsert,
                ExternalIdField = externalIdField
            });
        }

        public virtual SalesforceResponse Upsert<T>(T item, string externalIdField) where T : SObject
        {
            return Upsert<T>(new[] { item }, externalIdField);
        }

        public virtual SalesforceResponse Update<T>(IEnumerable<T> items) where T : SObject
        {
            return Crud(new CrudOperation<T>
            {
                Items = items,
                OperationType = CrudOperations.Update
            });
        }

        public virtual SalesforceResponse Update<T>(T item) where T : SObject
        {
            return Update<T>(new[] { item });
        }

        public virtual SalesforceResponse Delete<T>(IEnumerable<T> items) where T : SObject
        {
            return Crud(new CrudOperation<T>
            {
                Items = items,
                OperationType = CrudOperations.Delete
            });
        }

        public virtual SalesforceResponse Delete<T>(T item) where T : SObject
        {
            return Delete<T>(new[] { item });
        }

        /// <summary>
        ///     Delete
        ///      - Used to delete a single record by id
        /// </summary>
        /// <param name="id">the Id of the record to delete</param>
        /// <returns></returns>
        public virtual SalesforceResponse Delete(string id)
        {
            return Delete(new[] { id });
        }

        /// <summary>
        ///     Delete
        ///      - Used to delete multiple records by id
        /// </summary>
        /// <param name="ids">the ids of the records to delete</param>
        /// <returns></returns>
        public virtual SalesforceResponse Delete(string[] ids)
        {
            return PerformSimpleRequest(SoapRequestManager.GetDeleteRequest(ids, Login()));
        }

        public virtual JobInfo CreateBulkJob<T>(JobConfig config)
        {
            return PerformGenericRequest<JobInfo>(BulkRequestManager.GetStartJobRequest<T>(config, Login()), "jobInfo");
        }

        public virtual JobInfo QueryBulkJob(string jobId)
        {
            return PerformGenericRequest<JobInfo>(BulkRequestManager.GetQueryJobRequest(jobId, Login()), "jobInfo");
        }

        public virtual BatchInfo AddBatch<T>(IEnumerable<T> items, string jobId, int limit = 10000) where T : SObject
        {
            int count = items.Count();

            // TODO: We need a better way of adding multiple batches
            if (count > limit)
            {
                IEnumerable<IEnumerable<T>> batches = items.Chunk(limit);
                BatchInfo response = new BatchInfo();
                foreach (BatchInfo info in batches.Select(batch => PerformGenericRequest<BatchInfo>(
                    BulkRequestManager.GetBatchRequest(batch.ToArray(), jobId, Login()), "batchInfo")))
                {
                    response.NumberRecordsProcessed += info.NumberRecordsProcessed;
                    response.JobId = info.JobId;
                    response.Id = info.Id;
                }

                return response;
            }

            return PerformGenericRequest<BatchInfo>(BulkRequestManager.GetBatchRequest(items.ToArray(), jobId, Login()), "batchInfo");
        }

        public virtual SalesforceResponse CloseBulkJob(string jobId)
        {
            return PerformGenericRequest<SalesforceResponse>(BulkRequestManager.GetCloseJobRequest(jobId, Login()), "jobInfo");
        }

        #region Private Methods

        private T PerformGenericRequest<T>(HttpRequest request, string rootName = "result")
        {
            using (HttpClient httpClient = new HttpClient())
            {
                XmlDocument response = httpClient.PerformRequest(request);
                return ResponseReader.ReadGenericResponse<T>(response, rootName);
            }
        }

        private int PerformCountRequest(HttpRequest request)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                XmlDocument response = httpClient.PerformRequest(request);
                return ResponseReader.ReadCountResponse("size", response);
            }
        }

        private SalesforceResponse PerformSimpleRequest(HttpRequest request)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                XmlDocument response = httpClient.PerformRequest(request);
                return ResponseReader.ReadSimpleResponse(response);
            }
        }

        private IEnumerable<T> PerformArrayRequest<T>(HttpRequest request) where T : SObject
        {
            using (HttpClient httpClient = new HttpClient())
            {
                XmlDocument response = httpClient.PerformRequest(request);
                return ResponseReader.ReadArrayResponse<T>(response);
            }
        }

        private IEnumerable<T> PerformSearchRequest<T>(HttpRequest request) where T : SObject
        {
            using (HttpClient httpClient = new HttpClient())
            {
                XmlDocument response = httpClient.PerformRequest(request);
                return ResponseReader.ReadSearchResponse<T>(response);
            }
        }

        private IEnumerable<T> PerformRetrieveRequest<T>(HttpRequest request) where T : SObject
        {
            using (HttpClient httpClient = new HttpClient())
            {
                XmlDocument response = httpClient.PerformRequest(request);
                return ResponseReader.ReadRetriveResponse<T>(response);
            }
        }

        private QueryResult<T> PerformQueryRequest<T>(HttpRequest request) where T : SObject
        {
            using (HttpClient httpClient = new HttpClient())
            {
                XmlDocument response = httpClient.PerformRequest(request);
                return ResponseReader.ReadQueryResponse<T>(response);
            }
        }

        #endregion

        #region Implementation of IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool safe)
        {
            if (safe && _config.LogoutOnDisposal)
            {
                // TODO: Logout
            }
        }

        ~SalesforceClient()
        {
            Dispose(false); // TODO: Logout anyway?
        }
        #endregion
    }
}