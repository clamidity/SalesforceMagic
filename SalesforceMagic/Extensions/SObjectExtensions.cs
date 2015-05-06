using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SalesforceMagic.Attributes;
using SalesforceMagic.Entities;
using SalesforceMagic.ORM;

namespace SalesforceMagic.Extensions
{
	/// <summary>
	/// Extension class to help
	/// </summary>
	public static class SObjectExtensions
	{
		/// <summary>
		/// Helper to determine if an object parameter is null
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		private static bool IsNullable<T>(T obj)
		{
			if (obj == null) return true; // obvious

			Type type = typeof(T);

			return (!type.IsValueType) || (Nullable.GetUnderlyingType(type) != null);
		}

		/// <summary>
		/// Auto-fill "FieldsToNull" attribute on SObject
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="item"></param>
		/// <returns></returns>
		public static T GenerateFieldsToNull<T>(this T item) where T : SObject
		{
			var type = typeof(T);
			var accessor = ObjectHydrator.GetAccessor(type);
			var fieldsToNull = new HashSet<string>();

			// iterate over all READ-ONLY fields and if the value is null, add it to the list of fields to null
			foreach (var info in
				type.GetProperties().Where(info =>
					(info.GetCustomAttribute<SalesforceReadonly>() == null) &&
					(accessor[item, info.Name] == null) &&
					(!fieldsToNull.Contains(info.Name))))
			{
				fieldsToNull.Add(info.Name);
			}

			return item;
		}
	}
}
