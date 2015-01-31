﻿using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using FastMember;
using SalesforceMagic.Entities.Abstract;
using SalesforceMagic.Extensions;
using SalesforceMagic.ORM;
using SalesforceMagic.ORM.BaseRequestTemplates;

namespace SalesforceMagic.Entities
{
    public abstract class SObject : ISalesforceObject, IXmlSerializable
    {
        public string Id { get; set; }

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
            Type type = GetType();
            TypeAccessor accessor = ObjectHydrator.GetAccessor(type);
            writer.WriteElementString("type", type.GetName());

            foreach (PropertyInfo info in type.GetProperties())
            {
                object value = accessor[this, info.Name];
                if (value == null) continue;

                string xmlValue = value is DateTime
                    ? ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ssZ")
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

                writer.WriteElementString(info.GetName(), SalesforceNamespaces.SObject, xmlValue);
            }
        }

        internal string ToCsv()
        {
            Type type = GetType();
            TypeAccessor accessor = ObjectHydrator.GetAccessor(type);
            string[] values = type.GetProperties().Select(x => GetCsvValue(x, accessor)).ToArray();

            return String.Join(",", values);
        }

        private string GetCsvValue(PropertyInfo info, TypeAccessor accessor)
        {
            Type propertyType = info.PropertyType;
            if (propertyType == typeof(string)) return string.Format("\"{0}\"", accessor[this, info.Name]);

            return propertyType == typeof(DateTime)
                ? string.Format("\"{0}\"", GetDate((DateTime)accessor[this, info.Name]))
                : string.Format("{0}", accessor[this, info.Name]);
        }

        private string GetDate(DateTime date)
        {
            return date == DateTime.MinValue ? null : date.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
    }
}