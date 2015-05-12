using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using FastMember;
using SalesforceMagic.Attributes;
using SalesforceMagic.Entities.Abstract;
using SalesforceMagic.Extensions;
using SalesforceMagic.ORM;
using SalesforceMagic.ORM.BaseRequestTemplates;
using SalesforceMagic.SoapApi.Enum;

namespace SalesforceMagic.Entities
{
	public abstract class SObject : ISalesforceObject, IXmlSerializable
	{
		protected SObject()
		{
			FieldsToNull = new List<string>();
		}

		public virtual string Id { get; set; }

		[SalesforceIgnore, SalesforceReadonly]
		public virtual IList<string> FieldsToNull { get; set; }

		[SalesforceIgnore, SalesforceReadonly]
		internal CrudOperations OperationType { get; set; }

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
		}

		public void WriteXml(XmlWriter writer)
		{
			// TODO: Implement more robust serialization
			var type = GetType();
			var accessor = ObjectHydrator.GetAccessor(type);
			writer.WriteElementString("type", type.GetName());

			IList<string> fieldsToNull = new List<string>();
			foreach (var info in from info in type.FilterProperties<SalesforceReadonly>()
								 let ignoreAttribute = info.GetCustomAttribute<SalesforceIgnore>()
								 where ignoreAttribute == null || (!ignoreAttribute.IfEmpty)
								 select info)
			{
				var value = accessor[this, info.Name];
				var salesforceName = info.GetName();

				// Need to be able to dynamically control nullable fields
				if (value == null)
				{
					if ((FieldsToNull.Count == 0) || (!FieldsToNull.Contains(info.Name)))
					{
						fieldsToNull.Add(salesforceName);
					}

					continue;
				}

				var xmlValue = value is DateTime
					? ((DateTime)value).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") // Contributed by: Murillo.Mike - Salesforce requires UTC dates
					: value.ToString();

				//Added additional routine for when value is Byte[] ---bnewbold 22OCT2014
				if ((value as byte[]) != null)
				{
					//When value is passed in a byte array, as when uploading a filestream file, we need to read the value in rather than cast it to a string.
					byte[] byteArray = (byte[])value; //Cast value as byte array into temp variable
					writer.WriteStartElement(info.GetName()); //Not using WriteElementsString so need to preface with the XML Tag
					writer.WriteBase64(byteArray, 0, byteArray.Length); //Just use base64 XML Writer
					writer.WriteEndElement(); //Close the xml tag
					continue;
				}

				writer.WriteElementString(salesforceName, SalesforceNamespaces.SObject, xmlValue);
			}

			if (OperationType != CrudOperations.Insert)
			{
				foreach (string field in fieldsToNull)
					writer.WriteElementString("fieldsToNull", SalesforceNamespaces.SObject, field);
			}
		}

		internal string ToCsv()
		{
			Type type = GetType();
			TypeAccessor accessor = ObjectHydrator.GetAccessor(type);
			string[] values = type.FilterProperties<SalesforceReadonly, SalesforceIgnore>().Select(x => GetCsvValue(x, accessor)).ToArray();

			return String.Join(",", values);
		}

		private string GetCsvValue(PropertyInfo info, TypeAccessor accessor)
		{
			Type propertyType = info.PropertyType;
			if (propertyType == typeof(string))
			{
				string value = (string)accessor[this, info.Name];
				if (!string.IsNullOrEmpty(value)) return string.Format("\"{0}\"", PrepareCsvValue(value));
			}

			if (propertyType == typeof(DateTime))
				return string.Format("\"{0}\"", GetDate((DateTime)accessor[this, info.Name]));
			if (propertyType == typeof(DateTime?))
				return string.Format("\"{0}\"", GetNullableDate((DateTime?)accessor[this, info.Name]));

			return string.Format("{0}", accessor[this, info.Name]);
		}

		private string GetDate(DateTime date)
		{
			return date == DateTime.MinValue ? null : date.ToString("yyyy-MM-ddTHH:mm:ssZ");
		}

		private string GetNullableDate(DateTime? date)
		{
			return date == null ? null : ((DateTime)date).ToString("yyyy-MM-ddTHH:mm:ssZ");
		}

		private string PrepareCsvValue(string value)
		{
			return value.Replace("\"", "\"\"");
		}
	}
}