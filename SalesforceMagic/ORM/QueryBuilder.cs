using System;
using System.Linq;
using System.Linq.Expressions;
using SalesforceMagic.Entities;
using SalesforceMagic.Extensions;
using SalesforceMagic.LinqProvider;

namespace SalesforceMagic.ORM
{
    public static class QueryBuilder
    {
        internal static string GenerateQuery<T>(int limit = default(int))
            where T : SObject
        {
            Type type = typeof(T);
            string query = CompileSelectStatements(type);
            if (limit != default(int)) AddQueryLimit(ref query, limit);

            return query;
        }

        internal static string GenerateQuery<T>(Expression<Func<T, bool>> predicate, int limit = default(int))
            where T : SObject
        {
            Type type = typeof(T);
            string query = CompileSelectStatements(type);
            if (predicate != null) AddConditionsSet(ref query, predicate);
            if (limit != default(int)) AddQueryLimit(ref query, limit);

            return query;
        }

        internal static string GenerateCountyQuery<T>(Expression<Func<T, bool>> predicate)
        {
            Type type = typeof(T);
            string query = string.Format("SELECT COUNT() FROM {0}", type.GetName());
            if (predicate != null) AddConditionsSet(ref query, predicate);

            return query;
        }

        internal static string GenerateSearchQuery<T>(string searchQuery, string fieldType)
        {
            Type type = typeof(T);
            return string.Format("FIND {0} IN {1} FIELDS RETURNING {2}({3})", "{" + searchQuery + "}", fieldType, type.GetName(), string.Join(", ", type.GetPropertyNames(true)));
        }

        private static string CompileSelectStatements(Type type)
        {
            return string.Format("SELECT {0} FROM {1}", string.Join(", ", type.GetPropertyNames(true).Where(name => name != "fieldsToNull")), type.GetName());
        }

        private static void AddConditionsSet<T>(ref string query, Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
                query = query + " WHERE " + SOQLVisitor.ConvertToSOQL(predicate);
        }

        private static void AddQueryLimit(ref string query, int limit)
        {
            query = string.Format("{0} LIMIT {1}", query, limit);
        }
    }
}