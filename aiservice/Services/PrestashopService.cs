using AIService.Entities;
using AIService.Logs;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace AIService.Services
{
    public class PrestashopService
    {
        private static string label = "Services";
        private static string className = "PrestashopService";

        public static string GetAuthKey(AppSettings appSettings)
        {
            byte[] plainTextBytes = Encoding.ASCII.GetBytes(appSettings.PrestashopSettings.ApiKey + ":");
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string GetResourcePath(string resource)
        {
            switch (resource)
            {
                case "addresses": return "address";
                case "categories": return "category";
                case "countries": return "country";
                case "customers": return "customer";
                case "products": return "product";
                case "stock_availables": return "stock_available";
                case "stores": return "store";
                case "taxes": return "tax";
                case "zones": return "zone";
                default: return "";
            }
        }

        //public static string GetAssociationsPath(string association)
        //{
        //    switch (association)
        //    {
        //        case "categories": return "category";
        //        case "images": return "image";
        //        case "combinations": return "combination";
        //        case "product_option_values": return "product_option_value";
        //        case "product_features": return "product_feature";
        //        case "tags": return "tag";
        //        case "stock_availables": return "stock_available";
        //        case "accessories": return "product";
        //        case "product_bundle": return "product";
        //        case "groups": return "group";
        //        default: return "";
        //    }
        //}

        public static JObject CreateBasicElement(string resourcepath, JObject jObject, Dictionary<string, object> data)
        {
            switch (resourcepath)
            {
                case "address":
                    jObject["prestashop"][resourcepath]["id_country"] = data["id_country"].ToString(); // Id
                    jObject["prestashop"][resourcepath]["alias"] = data["alias"].ToString(); // String
                    jObject["prestashop"][resourcepath]["lastname"] = data["lastname"].ToString(); // String
                    jObject["prestashop"][resourcepath]["firstname"] = data["firstname"].ToString(); // String
                    jObject["prestashop"][resourcepath]["address1"] = data["address1"].ToString(); // String
                    jObject["prestashop"][resourcepath]["city"] = data["city"].ToString(); // String
                    break;
                case "category":
                    jObject["prestashop"][resourcepath]["active"] = data["active"].ToString(); // Bool
                    jObject["prestashop"][resourcepath]["name"] = data["name"].ToString(); // String
                    jObject["prestashop"][resourcepath]["link_rewrite"] = data["link_rewrite"].ToString(); // String
                    break;
                case "country":
                    jObject["prestashop"][resourcepath]["id_zone"] = data["id_zone"].ToString(); // Id
                    jObject["prestashop"][resourcepath]["iso_code"] = data["iso_code"].ToString(); // String
                    jObject["prestashop"][resourcepath]["contains_states"] = data["contains_states"].ToString(); // Bool
                    jObject["prestashop"][resourcepath]["need_identification_number"] = data["need_identification_number"].ToString(); // Bool
                    jObject["prestashop"][resourcepath]["display_tax_label"] = data["display_tax_label"].ToString(); // Bool
                    jObject["prestashop"][resourcepath]["name"] = data["name"].ToString(); // String
                    break;
                case "customer":
                    jObject["prestashop"][resourcepath]["passwd"] = data["passwd"].ToString(); // String
                    jObject["prestashop"][resourcepath]["lastname"] = data["lastname"].ToString(); // String
                    jObject["prestashop"][resourcepath]["firstname"] = data["firstname"].ToString(); // String
                    jObject["prestashop"][resourcepath]["email"] = data["email"].ToString(); // String
                    break;
                case "product":
                    jObject["prestashop"][resourcepath]["price"] = data["price"].ToString(); // Float
                    break;
                case "stock_available":
                    jObject["prestashop"][resourcepath]["id_product"] = data["id_product"].ToString(); // Id
                    jObject["prestashop"][resourcepath]["id_product_attribute"] = data["id_product_attribute"].ToString(); // Id
                    jObject["prestashop"][resourcepath]["quantity"] = data["quantity"].ToString(); // Int
                    jObject["prestashop"][resourcepath]["depends_on_stock"] = data["depends_on_stock"].ToString(); // Bool
                    jObject["prestashop"][resourcepath]["out_of_stock"] = data["out_of_stock"].ToString(); // Int
                    break;
                case "store":
                    jObject["prestashop"][resourcepath]["id_country"] = data["id_country"].ToString(); // Id
                    jObject["prestashop"][resourcepath]["city"] = data["city"].ToString(); // String
                    jObject["prestashop"][resourcepath]["active"] = data["active"].ToString(); // Bool
                    jObject["prestashop"][resourcepath]["name"] = data["name"].ToString(); // String
                    jObject["prestashop"][resourcepath]["address1"] = data["address1"].ToString(); // String
                    break;
                case "tax":
                    jObject["prestashop"][resourcepath]["rate"] = data["rate"].ToString(); // Float
                    jObject["prestashop"][resourcepath]["name"] = data["name"].ToString(); // String
                    break;
                case "zone":
                    jObject["prestashop"][resourcepath]["name"] = data["name"].ToString(); // String
                    break;
                default:
                    return new JObject();
            }
            jObject["prestashop"][resourcepath]["state"] = "1"; // Bool
            jObject["prestashop"][resourcepath]["active"] = "1"; // Bool
            return jObject;
        }

        public static JObject CheckValidsKey(string resourcepath, JObject jObject)
        {
            switch (resourcepath)
            {
                case "product":
                    if (jObject["prestashop"][resourcepath]["manufacturer_name"] != null)
                        ((JObject)jObject["prestashop"][resourcepath]).Property("manufacturer_name").Remove();
                    if (jObject["prestashop"][resourcepath]["quantity"] != null)
                        ((JObject)jObject["prestashop"][resourcepath]).Property("quantity").Remove();
                    return jObject;
                case "category":
                    if (jObject["prestashop"][resourcepath]["level_depth"] != null)
                        ((JObject)jObject["prestashop"][resourcepath]).Property("level_depth").Remove();
                    if (jObject["prestashop"][resourcepath]["nb_products_recursive"] != null)
                        ((JObject)jObject["prestashop"][resourcepath]).Property("nb_products_recursive").Remove();
                    return jObject;
                case "customer":
                    if (jObject["prestashop"][resourcepath]["last_passwd_gen"] != null)
                        ((JObject)jObject["prestashop"][resourcepath]).Property("last_passwd_gen").Remove();
                    if (jObject["prestashop"][resourcepath]["secure_key"] != null)
                        ((JObject)jObject["prestashop"][resourcepath]).Property("secure_key").Remove();
                    return jObject;
                default: return new JObject();
            }
        }

        public static async Task<JObject> GetResource(AppSettings appSettings, string resource, Dictionary<string, object> data)
        {
            string methodName = "GetResource";
            JObject result = new JObject();
            try
            {
                // Basic
                string url = appSettings.PrestashopSettings.UrlApiBase;
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers["Output-Format"] = "JSON";

                // Query Params
                Dictionary<string, string> queryparams = new Dictionary<string, string>();
                if (data.ContainsKey("zyxme"))
                {
                    foreach (var d in CommonService.JObjectToDictionary(JObject.FromObject(data["zyxme"])))
                    {
                        if (d.Value.GetType().Name.Contains("string", StringComparison.InvariantCultureIgnoreCase)
                            || d.Value.GetType().Name.Contains("int", StringComparison.InvariantCultureIgnoreCase)
                            || d.Value.GetType().Name.Contains("double", StringComparison.InvariantCultureIgnoreCase)
                            )
                        {
                            queryparams.Add(d.Key, d.Value.ToString());
                        }
                    }
                }

                // Get Element
                HttpResponseMessage getresponse = new HttpResponseMessage();
                if (data.ContainsKey("id"))
                {
                    getresponse = await CommonService.HttpRequestContent(appSettings, url, resource + "/" + data["id"] + QueryString.Create(queryparams), "GET", null, headers, "Basic", GetAuthKey(appSettings));
                }
                else
                {
                    getresponse = await CommonService.HttpRequestContent(appSettings, url, resource + QueryString.Create(queryparams), "GET", null, headers, "Basic", GetAuthKey(appSettings));
                }
                result = CommonService.StringToJObject(await getresponse.Content.ReadAsStringAsync());
                result["_statuscode"] = ((int)getresponse.StatusCode).ToString() + getresponse.StatusCode;
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {resource}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static async Task<JObject> PostResource(AppSettings appSettings, string resource, Dictionary<string, object> data)
        {
            string methodName = "PostResource";
            JObject result = new JObject();
            try
            {
                // Basic
                string url = appSettings.PrestashopSettings.UrlApiBase;
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers["Output-Format"] = "JSON";

                Dictionary<string, object> search = new Dictionary<string, object>();
                search["zyxme"] = new Dictionary<string, object>();
                switch (resource)
                {
                    case "addresses":
                        search["zyxme"] = new Dictionary<string, object>() {
                            { "filter[id_country]", data["id_country"] },
                            { "filter[alias]", data["alias"] },
                            { "filter[lastname]", data["lastname"] },
                            { "filter[firstname]", data["firstname"] },
                            { "filter[city]", data["city"] },
                            { "filter[address1]", data["address1"] }
                        };
                        break;
                    case "categories":
                        search["zyxme"] = new Dictionary<string, object>() {
                            { "filter[name]", data["name"] }
                        };
                        break;
                    case "countries":
                        search["zyxme"] = new Dictionary<string, object>() {
                            { "filter[id_zone]", data["id_zone"] },
                            { "filter[iso_code]", data["iso_code"] },
                            { "filter[name]", data["name"] }
                        };
                        break;
                    case "customers":
                        search["zyxme"] = new Dictionary<string, object>() {
                            { "filter[email]", data["email"] }
                        };
                        break;
                    case "products":
                        search["zyxme"] = new Dictionary<string, object>() {
                            { "filter[name]", data["name"] }
                        };
                        break;
                    case "stores":
                        search["zyxme"] = new Dictionary<string, object>() {
                            { "filter[id_country]", data["id_country"] },
                            { "filter[city]", data["city"] },
                            { "filter[name]", data["name"] },
                            { "filter[address1]", data["address1"] }
                        };
                        break;
                    case "taxes":
                        search["zyxme"] = new Dictionary<string, object>() {
                            { "filter[name]", data["name"] }
                        };
                        break;
                    case "zones":
                        search["zyxme"] = new Dictionary<string, object>() {
                            { "filter[name]", data["name"] }
                        };
                        break;
                    default:
                        break;
                }
                JObject searchresult = await GetResource(appSettings, resource, search);
                if (searchresult[resource] != null && searchresult[resource].HasValues)
                {
                    data["id"] = searchresult[resource][0]["id"].ToString();
                    result = await PutResource(appSettings, resource, data);
                }
                else
                {
                    // New Element
                    Dictionary<string, string> queryparams = new Dictionary<string, string>();
                    queryparams.Add("schema", "blank");
                    HttpResponseMessage blankresponse = await CommonService.HttpRequestContent(appSettings, url, resource + QueryString.Create(queryparams), "GET", null, null, "Basic", GetAuthKey(appSettings));
                    JObject jObject = CommonService.XMLToJObject(await blankresponse.Content.ReadAsStringAsync());
                    string resourcepath = GetResourcePath(resource);
                    jObject = CreateBasicElement(resourcepath, jObject, data);
                    HttpResponseMessage newresponse = await CommonService.HttpRequestXML(appSettings, url, resource, "POST", JsonConvert.DeserializeXmlNode(jObject.ToString()).InnerXml, headers, "Basic", GetAuthKey(appSettings));
                    jObject = CommonService.StringToJObject(await newresponse.Content.ReadAsStringAsync());

                    // Get Basic Element Created
                    HttpResponseMessage getresponse = await CommonService.HttpRequestContent(appSettings, url, resource + "/" + jObject[resourcepath]["id"].ToString(), "GET", null, null, "Basic", GetAuthKey(appSettings));
                    var r = await getresponse.Content.ReadAsStringAsync();
                    r = Regex.Replace(r, "<!\\[CDATA\\[", "");
                    r = Regex.Replace(r, "]]>", "");
                    jObject = CommonService.XMLToJObject(r);

                    // Update Element
                    foreach (var d in data)
                    {
                        if (!d.Key.Contains("zyxme") && !d.Key.Contains("associations"))
                        {
                            if (d.Value.GetType().Name == "JObject")
                            {
                                jObject["prestashop"][resourcepath][d.Key] = JObject.Parse(d.Value.ToString());
                            }
                            else
                            {
                                jObject["prestashop"][resourcepath][d.Key] = new JValue(d.Value);
                            }
                        }
                    }
                    jObject = CheckValidsKey(resourcepath, jObject);

                    XmlDocument xmlDocument = JsonConvert.DeserializeXmlNode(jObject.ToString());

                    if (data.ContainsKey("associations"))
                    {
                        foreach (var d in JObject.FromObject(data["associations"]))
                        {
                            if (xmlDocument.SelectSingleNode($"//prestashop/{resourcepath}/associations/{d.Key}") == null)
                            {
                                XmlNode fatherElem = xmlDocument.CreateNode("element", d.Key, "");
                                xmlDocument.SelectSingleNode($"//prestashop/{resourcepath}/associations").AppendChild(fatherElem);
                            }
                            xmlDocument.SelectSingleNode($"//prestashop/{resourcepath}/associations/{d.Key}").RemoveAll();

                            foreach (JObject d2 in CommonService.JArrayToListJObject(d.Value))
                            {
                                foreach (var d3 in CommonService.JObjectToDictionary(d2))
                                {
                                    // Create a new element node.
                                    XmlElement idElem = xmlDocument.CreateElement("id");
                                    idElem.InnerText = d3.Value.ToString();
                                    XmlElement childElem = xmlDocument.CreateElement(d3.Key);
                                    childElem.AppendChild(idElem);
                                    xmlDocument.SelectSingleNode($"//prestashop/{resourcepath}/associations/{d.Key}").AppendChild(childElem);
                                }
                            }
                        }
                    }

                    HttpResponseMessage updateresponse = await CommonService.HttpRequestXML(appSettings, url, resource, "PUT", JsonConvert.DeserializeXmlNode(jObject.ToString()).InnerXml, headers, "Basic", GetAuthKey(appSettings));
                    result = CommonService.StringToJObject(await updateresponse.Content.ReadAsStringAsync());

                    if (resource == "customers")
                    {
                        Dictionary<string, string> loginparams = new Dictionary<string, string>();
                        loginparams.Add("back", "my-account");
                        loginparams.Add("email", xmlDocument["prestashop"][resourcepath]["email"].InnerText);
                        loginparams.Add("password", xmlDocument["prestashop"][resourcepath]["passwd"].InnerText);
                        loginparams.Add("submitLogin", "1");
                        result["accessurl"] = appSettings.PrestashopSettings.LoginUrl + QueryString.Create(loginparams);
                    }

                    result["_statuscode"] = ((int)updateresponse.StatusCode).ToString() + updateresponse.StatusCode;
                }
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {resource}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static async Task<JObject> PutResource(AppSettings appSettings, string resource, Dictionary<string, object> data)
        {
            string methodName = "PutResource";
            JObject result = new JObject();
            try
            {
                // Basic
                string url = appSettings.PrestashopSettings.UrlApiBase;
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers["Output-Format"] = "JSON";

                // Get Element
                HttpResponseMessage getresponse = await CommonService.HttpRequestContent(appSettings, url, resource + "/" + data["id"].ToString(), "GET", null, null, "Basic", GetAuthKey(appSettings));
                var r = await getresponse.Content.ReadAsStringAsync();
                r = Regex.Replace(r, "<!\\[CDATA\\[", "");
                r = Regex.Replace(r, "]]>", "");
                JObject jObject = CommonService.XMLToJObject(r);
                string resourcepath = GetResourcePath(resource);

                // Update Element
                foreach (var d in data)
                {
                    if (!d.Key.Contains("zyxme") && !d.Key.Contains("associations"))
                    {
                        if (d.Value.GetType().Name == "JObject")
                        {
                            jObject["prestashop"][resourcepath][d.Key] = JObject.Parse(d.Value.ToString());
                        }
                        else
                        {
                            jObject["prestashop"][resourcepath][d.Key] = new JValue(d.Value);
                        }
                    }
                }
                jObject = CheckValidsKey(resourcepath, jObject);

                XmlDocument xmlDocument = JsonConvert.DeserializeXmlNode(jObject.ToString());

                if (data.ContainsKey("associations"))
                {
                    foreach (var d in JObject.FromObject(data["associations"]))
                    {
                        if (xmlDocument.SelectSingleNode($"//prestashop/{resourcepath}/associations/{d.Key}") == null)
                        {
                            XmlNode fatherElem = xmlDocument.CreateNode("element", d.Key, "");
                            xmlDocument.SelectSingleNode($"//prestashop/{resourcepath}/associations").AppendChild(fatherElem);
                        }
                        xmlDocument.SelectSingleNode($"//prestashop/{resourcepath}/associations/{d.Key}").RemoveAll();

                        foreach (JObject d2 in CommonService.JArrayToListJObject(d.Value))
                        {
                            foreach (var d3 in CommonService.JObjectToDictionary(d2))
                            {
                                // Create a new element node.
                                XmlElement idElem = xmlDocument.CreateElement("id");
                                idElem.InnerText = d3.Value.ToString();
                                XmlElement childElem = xmlDocument.CreateElement(d3.Key);
                                childElem.AppendChild(idElem);
                                xmlDocument.SelectSingleNode($"//prestashop/{resourcepath}/associations/{d.Key}").AppendChild(childElem);
                            }
                        }
                    }
                }

                HttpResponseMessage updateresponse = await CommonService.HttpRequestXML(appSettings, url, resource, "PUT", xmlDocument.InnerXml, headers, "Basic", GetAuthKey(appSettings));
                result = CommonService.StringToJObject(await updateresponse.Content.ReadAsStringAsync());

                if (resource == "customers")
                {
                    Dictionary<string, string> loginparams = new Dictionary<string, string>();
                    loginparams.Add("back", "my-account");
                    loginparams.Add("email", xmlDocument["prestashop"][resourcepath]["email"].InnerText);
                    loginparams.Add("password", xmlDocument["prestashop"][resourcepath]["passwd"].InnerText);
                    loginparams.Add("submitLogin", "1");
                    result["accessurl"] = appSettings.PrestashopSettings.LoginUrl + QueryString.Create(loginparams);
                }

                result["_statuscode"] = ((int)updateresponse.StatusCode).ToString() + updateresponse.StatusCode;
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {resource}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }
    }
}
