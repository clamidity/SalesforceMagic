using System;

namespace SalesforceMagic.Attributes
{
	/// <summary>
	/// Attribute class that uses flags to denote various read/write capabilities of a field
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class SalesforceFilterAttribute : Attribute
	{
		public SalesforceFilterAttribute()
		{
			// default option is to ignore read and write access
			Options = SalesforceFilterOption.Ignore;
		}

		internal SalesforceFilterAttribute(bool forInternalRead)
		{
			ForInternalRead = forInternalRead;
		}

		/// <summary>
		/// Options that will (dis)allow read/write access
		/// </summary>
		public SalesforceFilterOption Options { get; set; }

		/// <summary>
		/// Used by internal library functions for serialization purposes
		/// </summary>
		internal bool ForInternalRead { get; set; }
	}
}
