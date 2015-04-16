using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SalesforceMagic.BulkApi.Configuration;
using SalesforceMagic.BulkApi.Models;
using SalesforceMagic.Configuration;
using SalesforceMagic.Entities;

namespace SalesforceMagic.Abstract
{
    public interface ISalesforceClient : IDisposable
    {
        #region Utility Methods

        void ChangeEnvironment(SalesforceConfig config, bool login = false);
        string GetSessionId();
        SalesforceSession GetSession();
        SalesforceSession Login();

        #endregion

        #region Count Methods

        /// <summary>
        ///     Retrieve an object total count.
        ///      - Allows for filtering count totals 
        ///        using the conditional lambda.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        int Count<T>(Expression<Func<T, bool>> predicate = null) where T : SObject;

        #endregion

        #region Query Methods

        /// <summary>
        ///     Simple Query
        ///      - Query items based on generic object
        ///      - Limited by 200 records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="limit"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(int limit = 0) where T : SObject;

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
        IEnumerable<T> Query<T>(Expression<Func<T, bool>> predicate, int limit = 0) where T : SObject;

        /// <summary>
        ///     Simple Query
        ///      - Query items based on generic object
        ///      - Utilize included raw query
        ///      - Limited by 200 records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string query) where T : SObject;

        /// <summary>
        ///     Advanced Query
        ///      - Query items based on generic object
        ///      - Returns query locator, and done status which
        ///        can be used to bypass the 200 record limit.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="limit"></param>
        /// <returns></returns>
        QueryResult<T> AdvancedQuery<T>(int limit = 0) where T : SObject;

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
        QueryResult<T> AdvancedQuery<T>(Expression<Func<T, bool>> predicate, int limit = 0) where T : SObject;

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
        QueryResult<T> AdvancedQuery<T>(string query) where T : SObject;

        /// <summary>
        ///     Query More
        ///      - Used to retrieve the next set of records
        ///        available in a query using the queryLocator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryLocator"></param>
        /// <returns></returns>
        QueryResult<T> QueryMore<T>(string queryLocator) where T : SObject;

        /// <summary>
        ///     Query Single
        ///      - Used to retrieve a single record
        ///        using filter criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        T QuerySingle<T>(Expression<Func<T, bool>> predicate) where T : SObject;

        /// <summary>
        ///     Query Single
        ///      - Used to retrieve a single record
        ///        using a raw string query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        T QuerySingle<T>(string query) where T : SObject;

        #endregion

        #region Crud Methods

        SalesforceResponse Crud<T>(CrudOperation<T> operation) where T : SObject;
        SalesforceResponse Insert<T>(IEnumerable<T> items) where T : SObject;
        SalesforceResponse Insert<T>(T item) where T : SObject;
        SalesforceResponse Upsert<T>(IEnumerable<T> items, string externalIdField) where T : SObject;
        SalesforceResponse Upsert<T>(T item, string externalIdField) where T : SObject;
        SalesforceResponse Update<T>(IEnumerable<T> items) where T : SObject;
        SalesforceResponse Update<T>(T item) where T : SObject;
        SalesforceResponse Delete<T>(IEnumerable<T> items) where T : SObject;
        SalesforceResponse Delete<T>(T item) where T : SObject;

        #endregion

        #region Bulk Data Methods

        JobInfo CreateBulkJob<T>(JobConfig config);
        JobInfo QueryBulkJob(string jobId);
        BatchInfo AddBatch<T>(IEnumerable<T> items, string jobId, int limit = 10000) where T : SObject;
        SalesforceResponse CloseBulkJob(string jobId);

        #endregion

    }
}