using System;

namespace SalesforceMagic.Attributes
{
	[Flags]
	public enum SalesforceFilterOption : short
	{
		None = 0,
		Ignore = 1,
		AllowRead = 1 << 1,
		SaveOnInsertOnly = 1 << 2,
		IgnoreIfNull = 1 << 3
	}
}