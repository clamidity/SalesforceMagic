using System;

namespace SalesforceMagic.Attributes
{
    /// <summary>
    ///     Attribute that can be used to ignore a property
    ///     when building the salesforce query.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	[Obsolete("Use the SalesforceFilterAttribute instead")]
    public class SalesforceIgnore : Attribute
    {
	    public bool IfEmpty { get; set; }
	}
}