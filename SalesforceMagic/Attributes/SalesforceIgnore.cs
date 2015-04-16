using System;
using SalesforceMagic.Enums;

namespace SalesforceMagic.Attributes
{
    /// <summary>
    ///     Attribute that can be used to ignore a property
    ///     when building the salesforce query.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SalesforceIgnore : Attribute
    {
        public bool IfEmpty { get; set; }
//        public ApiType ApiType { get; set; }
//
//        public SalesforceIgnore(/*ApiType type = ApiType.Both*/)
//        {
//            ApiType = type;
//        }
    }
}