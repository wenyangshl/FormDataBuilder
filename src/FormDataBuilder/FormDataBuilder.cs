using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace FormDataBuilder
{
    public static class FormDataBuilder
    {
        private static Dictionary<string, string> CreateFormData(object data, ref Dictionary<string, string> formData, string parentKey)
        {
            const string REPLACEKEY = "%^&*";
            if (data == null)
                return formData;
            PropertyInfo[] properties = data.GetType().GetProperties();
            foreach (var p in properties)
            {
                if (p.GetValue(data) == null)
                    continue;
                object[] attrs = p.GetCustomAttributes(true);
                string name = "";
                bool isRequired = false;
                foreach (object attr in attrs)
                {
                    if (attr.GetType() == typeof(RequiredAttribute))
                    {
                        if (p.GetValue(data) == null)
                        {
                            throw new Exception((attr as RequiredAttribute)?.ErrorMessage);
                        }
                    }
                    else if (attr.GetType() == typeof(JsonPropertyAttribute))
                    {
                        name = (attr as JsonPropertyAttribute)?.PropertyName;
                    }
                }

                if (string.IsNullOrEmpty(name))
                {
                    name = p.Name.ToLower();
                }

                if (p.PropertyType.IsPrimitive
                    || p.PropertyType.IsValueType
                    || p.PropertyType == typeof(String))
                {
                    string value = p.GetValue(data).ToString();
                    if (!string.IsNullOrEmpty(parentKey))
                    {
                        name = parentKey.Replace(REPLACEKEY, name);
                    }
                    formData.Add(name, value);
                }
                else if (p.PropertyType == typeof(Dictionary<string, string>))
                {
                    foreach (KeyValuePair<string, string> kvp in (Dictionary<string, string>)p.GetValue(data))
                    {
                        string key = string.IsNullOrEmpty(parentKey) ?
                            $"{name}[{kvp.Key}]" :
                            parentKey.Replace(REPLACEKEY, $"{name}[{kvp.Key}]");
                        if (!formData.ContainsKey(key))
                        {
                            formData.Add(key, kvp.Value);
                        }
                    }
                }
                else if (!p.PropertyType.IsValueType)
                {
                    string newParentKey = "";
                    if (string.IsNullOrEmpty(parentKey))
                    {
                        newParentKey = $"{name}[{REPLACEKEY}]";
                    }
                    else
                    {
                        newParentKey = parentKey.Replace(REPLACEKEY, $"{name}[{REPLACEKEY}]");
                    }
                    CreateFormData(p.GetValue(data), ref formData, newParentKey);
                }
            }

            return formData;
        }
    }
}
