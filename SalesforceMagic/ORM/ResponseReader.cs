using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using FastMember;
using SalesforceMagic.Entities;
using SalesforceMagic.Extensions;
using SalesforceMagic.SoapApi.Responses;

namespace SalesforceMagic.ORM
{
	internal static class ResponseReader
	{
		internal static string ReadStringResponse(string name, XmlDocument document)
		{
			XmlNode node = GetSingleNamedNodes(document, name);
			return node != null ? node.InnerText : null;
		}

		internal static bool ReadBoolResponse(string name, XmlDocument document)
		{
			XmlNode node = GetSingleNamedNodes(document, name);
			return node != null ? Convert.ToBoolean(node.InnerText) : default(bool);
		}

		internal static int ReadCountResponse(string name, XmlDocument document)
		{
			XmlNode node = GetSingleNamedNodes(document, name);
			return node != null ? Convert.ToInt32(node.InnerText) : default(int);
		}

		internal static SalesforceResponse ReadSimpleResponse(XmlDocument document)
		{
			var response = new SalesforceResponse();
			var results = GetNamedNodes(document, "result");

			// ensure accurate iteration over all available responses
			foreach (var result in from XmlNode node in results select ReadSimpleResponse<RecordResult>(node, document))
			{
				response.Results.Add(result);

				if (result.Success) continue;

				response.Errors.Add(result.Message);
			}

			response.Success = response.Results.All(x => x.Success);

			if (results.Count > 1)
			{
				if (!response.Success) response.Errors.Add("One or more records failed.");
			}
			else
			{
				response.Result = response.Results[0];

				if (!response.Success) response.Errors.Add(response.Result.Message);
			}

			return response;
		}

		internal static T ReadGenericResponse<T>(XmlDocument document, string rootName)
		{
			return ReadSimpleResponse<T>(GetNamedNodes(document, rootName)[0], document);
		}

		internal static T ReadSimpleResponse<T>(XmlNode node, XmlDocument document)
		{
			Type type = typeof (T);
			bool ns = type.BaseType ==  typeof (SObject);
			T obj = Activator.CreateInstance<T>();
			TypeAccessor accessor = ObjectHydrator.GetAccessor(type);

			foreach (PropertyInfo property in type.GetProperties())
			{
				string value;
				Type propertyType = property.PropertyType;
				string name = ns ? "sf:" + property.GetName() : property.GetName();

				if (name.Contains("."))
				{
					string referencedProperty = name.Substring(name.LastIndexOf(".", StringComparison.Ordinal) + 1);
					referencedProperty = ns ? "sf:" + referencedProperty : referencedProperty;
					name = name.Substring(0, name.LastIndexOf(".", StringComparison.Ordinal));
					XmlNode referencedNode = FindChildNode(name, node);
					if (referencedNode == null) continue;

					value = referencedNode.GetValue(referencedProperty);
				}
				else value = node.GetValue(name);

				if (value == null)
				{
					IEnumerable<XElement> nodes = GetNamedNodes(node, name);
					if (nodes == null || !nodes.Any()) continue;
					XElement child = nodes.FirstOrDefault();
					if (child == null) continue;

					value = child.Value;
				}

				// Constant
				if (propertyType == typeof(string)) accessor[obj, property.Name] = value;
				if (propertyType == typeof(bool)) accessor[obj, property.Name] = value.ToBoolean();
				if (propertyType == typeof(int)) accessor[obj, property.Name] = value.ToInt();
				if (propertyType == typeof(double)) accessor[obj, property.Name] = value.ToDouble();
				if (propertyType == typeof(decimal)) accessor[obj, property.Name] = value.ToDecimal();
				if (propertyType == typeof(float)) accessor[obj, property.Name] = value.ToFloat();
				if (propertyType == typeof(DateTime)) accessor[obj, property.Name] = value.ToDateTime();

				// Null Constant
				if (propertyType == typeof(bool?)) accessor[obj, property.Name] = value.ToNullableBoolean();
				if (propertyType == typeof(int?)) accessor[obj, property.Name] = value.ToNullableInt();
				if (propertyType == typeof(double?)) accessor[obj, property.Name] = value.ToNullableDouble();
				if (propertyType == typeof(decimal?)) accessor[obj, property.Name] = value.ToNullableDecimal();
				if (propertyType == typeof(float?)) accessor[obj, property.Name] = value.ToNullableFloat();
				if (propertyType == typeof(DateTime?)) accessor[obj, property.Name] = value.ToNullableDateTime();
			}

			return obj;
		}

		internal static T[] ReadArrayResponse<T>(XmlDocument document)
		{
			return (from XmlNode node in GetNamedNodes(document, "records") select ReadSimpleResponse<T>(node, document)).ToArray();
		}
		

		public static QueryResult<T> ReadQueryResponse<T>(XmlDocument document)
		{
			return new QueryResult<T>
			{
				QueryLocator = ReadStringResponse("queryLocator", document),
				Done = ReadBoolResponse("done", document),
				Records = (from XmlNode node in GetNamedNodes(document, "records") select ReadSimpleResponse<T>(node, document)).ToArray()
			};
		}

		private static XmlNodeList GetNamedNodes(XmlDocument document, string name)
		{
			return document.GetElementsByTagName(name);
		}

		private static XmlNode GetSingleNamedNodes(XmlDocument document, string name)
		{
			XmlNodeList list = document.GetElementsByTagName(name);
			return list.Count == 0 ? null : list[0];
		}

		private static XElement[] GetNamedNodes(XmlNode node, string name)
		{
			XDocument document = XDocument.Parse(node.OuterXml);
			return document.Descendants().Where(x => x.Name.LocalName == name).ToArray();
		}

		private static XmlNode FindChildNode(string name, XmlNode node)
		{
			return node.ChildNodes.Cast<XmlNode>().Where(x => x.Name == name).ToArray().FirstOrDefault();
		}
	}
}