using System;
using SalesforceMagic.Attributes;
using SalesforceMagic.BulkApi.Enum;
using SalesforceMagic.Entities;

namespace SalesforceMagic.BulkApi.Models
{
    [SalesforceName("jobInfo")]
    public class JobInfo
    {
        [SalesforceName("id")]
        public string Id { get; set; }

        [SalesforceName("operation")]
        public string Operation { get; set; }

        [SalesforceName("object")]
        public string Object { get; set; }

        [SalesforceName("createdById")]
        public string CreatedById { get; set; }

        [SalesforceName("createdDate")]
        public DateTime CreatedDate { get; set; }

        [SalesforceName("state")]
        public JobState State { get; set; }

        [SalesforceName("concurrencyMode")]
        public ConcurrencyMode ConcurrencyMode { get; set; }

        [SalesforceName("contentType")]
        public string ContentType { get; set; }

        [SalesforceName("numberBatchedQueued")]
        public int NumberBatchedQueued { get; set; }

        [SalesforceName("numberBatchedInProgress")]
        public int NumberBatchedInProgress { get; set; }

        [SalesforceName("numberBatchedCompleted")]
        public int NumberBatchedCompleted { get; set; }

        [SalesforceName("numberBatchedFailed")]
        public int NumberBatchedFailed { get; set; }

        [SalesforceName("numberBatchedTotal")]
        public int NumberBatchedTotal { get; set; }

        [SalesforceName("numberBatchedProcessed")]
        public int NumberBatchedProcessed { get; set; }

        [SalesforceName("numberRetries")]
        public int NumberRetries { get; set; }

        [SalesforceName("apiVersion")]
        public string ApiVersion { get; set; }
    }
}