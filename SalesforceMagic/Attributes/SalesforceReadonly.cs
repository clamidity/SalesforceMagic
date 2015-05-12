using System;

namespace SalesforceMagic.Attributes
{
    /// <summary>
    ///     Used to specify a property that can be read
    ///     but never pushed to Salesforce
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	[Obsolete("Use the SalesforceFilterAttribute instead")]
    public class SalesforceReadonly : Attribute
    {
    }
}