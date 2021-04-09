using AIService.Entities;
using AIService.Logs;
using IBM.Cloud.SDK.Core.Authentication.Iam;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace aiservice.Services
{
    public class DiscoveryRequest
    {
        public string Apikey { get; set; }
        public string ApiUrl { get; set; }
        public string Operation { get; set; }
        public string EnvironmentId { get; set; }
        public string CollectionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Size { get; set; }
        public string ConfigurationId { get; set; }
        public string Language { get; set; }
        public string Filter { get; set; }
        public string Query { get; set; }
        public string NaturalLanguageQuery { get; set; }
        public bool? Passages { get; set; }
        public string Aggregation { get; set; }
        public long? Count { get; set; }
        public string _return { get; set; }
        public long? Offset { get; set; }
        public string Sort { get; set; }
        public bool? Highlight { get; set; }
        public string PassagesFields { get; set; }
        public long? PassagesCount { get; set; }
        public long? PassagesCharacters { get; set; }
        public bool? Deduplicate { get; set; }
        public string DeduplicateField { get; set; }
        public bool? Similar { get; set; }
        public string SimilarDocumentsIds { get; set; }
        public string SimilarFields { get; set; }
        public string Bias { get; set; }
        public bool? SpellingSuggestions { get; set; }
        public bool? XWatsonLoggingOptOut { get; set; }
    }

    public class DiscoveryService
    {
        private static string label = "Services";
        private static string className = "DiscoveryService";
        public static dynamic Environments(AppSettings appSettings, DiscoveryRequest requestBody)
        {
            string methodName = "Environments";
            dynamic result = new ExpandoObject();
            try
            {
                WatsonSettings settings = appSettings.WatsonServices.Discovery;
                IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{requestBody.Apikey}");
                IBM.Watson.Discovery.v1.DiscoveryService discovery = new IBM.Watson.Discovery.v1.DiscoveryService($"{settings.Version}", authenticator);
                discovery.SetServiceUrl($"{requestBody.ApiUrl}");
                result = discovery.ListEnvironments().Result;
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static dynamic Collections(AppSettings appSettings, DiscoveryRequest requestBody)
        {
            string methodName = "Collections";
            dynamic result = new ExpandoObject();
            try
            {
                WatsonSettings settings = appSettings.WatsonServices.Discovery;
                IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{requestBody.Apikey}");
                IBM.Watson.Discovery.v1.DiscoveryService discovery = new IBM.Watson.Discovery.v1.DiscoveryService($"{settings.Version}", authenticator);
                discovery.SetServiceUrl($"{requestBody.ApiUrl}");
                result = discovery.ListCollections(requestBody.EnvironmentId).Result;
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static dynamic CollectionFields(AppSettings appSettings, DiscoveryRequest requestBody)
        {
            string methodName = "CollectionFields";
            dynamic result = new ExpandoObject();
            try
            {
                WatsonSettings settings = appSettings.WatsonServices.Discovery;
                IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{requestBody.Apikey}");
                IBM.Watson.Discovery.v1.DiscoveryService discovery = new IBM.Watson.Discovery.v1.DiscoveryService($"{settings.Version}", authenticator);
                discovery.SetServiceUrl($"{requestBody.ApiUrl}");
                result = discovery.ListCollectionFields(requestBody.EnvironmentId, requestBody.CollectionId).Result;
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static dynamic Fields(AppSettings appSettings, DiscoveryRequest requestBody)
        {
            string methodName = "Fields";
            dynamic result = new ExpandoObject();
            try
            {
                WatsonSettings settings = appSettings.WatsonServices.Discovery;
                IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{requestBody.Apikey}");
                IBM.Watson.Discovery.v1.DiscoveryService discovery = new IBM.Watson.Discovery.v1.DiscoveryService($"{settings.Version}", authenticator);
                discovery.SetServiceUrl($"{requestBody.ApiUrl}");
                result = discovery.ListFields(requestBody.EnvironmentId, new List<string>() { requestBody.CollectionId }).Result;
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static dynamic QueryCollection(AppSettings appSettings, DiscoveryRequest requestBody)
        {
            string methodName = "QueryCollection";
            dynamic result = new ExpandoObject();
            try
            {
                WatsonSettings settings = appSettings.WatsonServices.Discovery;
                IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{requestBody.Apikey}");
                IBM.Watson.Discovery.v1.DiscoveryService discovery = new IBM.Watson.Discovery.v1.DiscoveryService($"{settings.Version}", authenticator);
                discovery.SetServiceUrl($"{requestBody.ApiUrl}");

                string[] text = requestBody.Query.Split(":");

                result = discovery.Query(
                    environmentId: requestBody.EnvironmentId,
                    collectionId: requestBody.CollectionId,
                    filter: requestBody.Filter,
                    query: requestBody.Query,
                    naturalLanguageQuery: text[text.Length - 1],
                    aggregation: requestBody.Aggregation,
                    passages: requestBody.Passages ?? false,
                    passagesFields: requestBody.PassagesFields ?? "text",
                    passagesCount: requestBody.PassagesCount ?? 5,
                    highlight: requestBody.Highlight ?? false,
                    count: requestBody.Count ?? 5,
                    offset: requestBody.Offset ?? 0,
                    _return: requestBody._return
                    ).Result;

                if (requestBody.Highlight == true)
                {
                    ((IBM.Watson.Discovery.v1.Model.QueryResponse)result).Results.ForEach(x =>
                    {
                        if (x.AdditionalProperties["highlight"]["subtitle"] != null)
                        {
                            List<string> subtitles = x.AdditionalProperties["highlight"]["subtitle"].ToObject<List<string>>();
                            for (int i = 0; i < subtitles.Count; i++)
                            {
                                subtitles[i] = subtitles[i].Replace("<em>", "").Replace("</em>", "");
                                subtitles[i] = Regex.Replace(subtitles[i], @"[$][_][{].{1,2}[}][$]", "");
                            }
                            x.AdditionalProperties["highlight"]["subtitle"] = JToken.FromObject(subtitles);
                        }
                        if (x.AdditionalProperties["highlight"]["text"] != null)
                        {
                            List<string> texts = x.AdditionalProperties["highlight"]["text"].ToObject<List<string>>();
                            for (int i = 0; i < texts.Count; i++)
                            {
                                texts[i] = texts[i].Replace("<em>", "").Replace("</em>", "");
                                texts[i] = Regex.Replace(texts[i], @"[$][_][{].{1,2}[}][$]", "");
                            }
                            x.AdditionalProperties["highlight"]["text"] = JToken.FromObject(texts);
                        }
                        else
                        {
                            List<string> texts = new List<string>();
                            string txt = x.AdditionalProperties["text"].ToString();
                            txt = Regex.Replace(txt, @"[$][_][{].{1,2}[}][$]", "");
                            texts.Add(txt);
                            x.AdditionalProperties["highlight"]["text"] = JToken.FromObject(texts);
                        }
                    });
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }
    }
}
