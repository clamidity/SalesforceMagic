using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using FastMember;
using SalesforceMagic.Attributes;
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
            return node != null && Convert.ToBoolean(node.InnerText);
        }

        internal static int ReadCountResponse(string name, XmlDocument document)
        {
            XmlNode node = GetSingleNamedNodes(document, name);
            return node != null ? Convert.ToInt32(node.InnerText) : default(int);
        }

        internal static SalesforceResponse ReadSimpleResponse(XmlDocument document)
        {
            SalesforceResponse response = new SalesforceResponse();
            XmlNodeList results = GetNamedNodes(document, "result");

            if (results.Count > 1)
            {
                foreach (XmlNode node in results)
                {
                    RecordResult result = ReadSimpleResponse<RecordResult>(node, document);
                    response.Results.Add(result);

                    if (!result.Success)
                    {
                        response.Errors.Add(result.Message);
                    }
                }

                response.Success = response.Results.All(x => x.Success);
                if (!response.Success) response.Errors.Add("One or more records failed.");
            }
            else
            {
	            var result = ReadSimpleResponse<RecordResult>(results[0], document);

	            response.Result = result;
				response.Results.Add(result);

				response.Success = response.Result.Success;
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

            foreach (PropertyInfo property in type.FilterProperties<SalesforceIgnore>())
            {
                string value = null;
                Type propertyType = property.PropertyType;
                string propertyName = property.GetName();

                if (propertyName.Contains("."))
                {
                    string[] propertyNameSplit = propertyName.Split('.');

                    XmlNode referencedNode = null;
                    XmlNode currentNode = node;
                    for (int i = 0; i < (propertyNameSplit.Count() - 1); i++)
                    {
                        referencedNode = FindChildNode(ns ? string.Format("sf:{0}", propertyNameSplit[i]) : propertyNameSplit[i], currentNode);
                        if (referencedNode == null) 
                            break;

                        currentNode = referencedNode;
                    }

                    if (referencedNode != null)
                        value = referencedNode.GetValue(ns 
                            ? string.Format("sf:{0}", propertyNameSplit[(propertyNameSplit.Count() - 1)]) 
                            : propertyNameSplit[(propertyNameSplit.Count() - 1)]);
                }
                else value = node.GetValue(ns ? string.Format("sf:{0}", propertyName) : propertyName);

                if (value == null)
                {
                    IEnumerable<XElement> nodes = GetNamedNodes(node, propertyName);
                    if (nodes == null || !nodes.Any()) continue;
                    XElement child = nodes.FirstOrDefault();
                    if (child == null) continue;

                    value = child.Value;
                }

                // Constant
                if (propertyType.IsEnum) accessor[obj, property.Name] = Enum.Parse(propertyType, value);
                else if (propertyType == typeof(string)) accessor[obj, property.Name] = value;
                else if (propertyType == typeof(bool)) accessor[obj, property.Name] = value.ToBoolean();
                else if (propertyType == typeof(int)) accessor[obj, property.Name] = value.ToInt();
                else if (propertyType == typeof(double)) accessor[obj, property.Name] = value.ToDouble();
                else if (propertyType == typeof(decimal)) accessor[obj, property.Name] = value.ToDecimal();
                else if (propertyType == typeof(float)) accessor[obj, property.Name] = value.ToFloat();
                else if (propertyType == typeof(DateTime)) accessor[obj, property.Name] = value.ToDateTime();

                // Null Constant
                else if (propertyType == typeof(bool?)) accessor[obj, property.Name] = value.ToNullableBoolean();
                else if (propertyType == typeof(int?)) accessor[obj, property.Name] = value.ToNullableInt();
                else if (propertyType == typeof(double?)) accessor[obj, property.Name] = value.ToNullableDouble();
                else if (propertyType == typeof(decimal?)) accessor[obj, property.Name] = value.ToNullableDecimal();
                else if (propertyType == typeof(float?)) accessor[obj, property.Name] = value.ToNullableFloat();
                else if (propertyType == typeof(DateTime?)) accessor[obj, property.Name] = value.ToNullableDateTime();
            }

            return obj;
        }

        internal static T[] ReadArrayResponse<T>(XmlDocument document)
        {
            return (from XmlNode node in GetNamedNodes(document, "records") select ReadSimpleResponse<T>(node, document)).ToArray();
        }

        internal static T[] ReadRetriveResponse<T>(XmlDocument document) where T : SObject
        {
            return (from XmlNode node in GetNamedNodes(document, "result") select ReadSimpleResponse<T>(node, document)).Select(r => r.Id == null ? null : r).ToArray();
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

        internal static T[] ReadSearchResponse<T>(XmlDocument document)
        {
            return (from XmlNode node in GetNamedNodes(document, "record") select ReadSimpleResponse<T>(node, document)).ToArray();
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