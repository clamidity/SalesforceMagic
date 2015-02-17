using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LINQtoCSV;
using SalesforceMagic.BulkApi.Configuration;
using SalesforceMagic.BulkApi.Enum;
using SalesforceMagic.BulkApi.RequestTemplates;
using SalesforceMagic.Entities;
using SalesforceMagic.Extensions;
using SalesforceMagic.Http;
using SalesforceMagic.ORM.BaseRequestTemplates;

namespace SalesforceMagic.BulkApi
{
    internal static class BulkCommands
    {
        internal static string CreateBatch<T>(T[] items) where T : SObject
        {
            return GetCsvString(items);
        }

        internal static string CreateJob(JobConfig config, string objectName)
        {
            return XmlRequestGenerator.GenerateRequest(new JobTemplate
            {
                ContentType = "CSV",
                Object = objectName,
                Operation = config.Operation.ToString().ToLower(),
                ExternalIdFieldName = config.Operation == BulkOperations.Upsert ? config.ExternalIdFieldName : null,
                ConcurrencyMode = config.ConcurrencyMode.ToString()
            });
        }

        internal static string CloseJob()
        {
            return XmlRequestGenerator.GenerateRequest(new JobTemplate
            {
                State = "Closed"
            });
        }

        private static string GetCsvString<T>(T[] objects) where T : SObject
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(objects.GetCsvHeaders());
            foreach (T item in objects.Where(item => item != null))
            {
                builder.AppendLine(item.ToCsv());
            }

            return builder.ToString();
        }
    }
}
