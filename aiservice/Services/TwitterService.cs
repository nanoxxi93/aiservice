using AIService.Entities;
using AIService.Logs;
using AIService.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Globalization;
using System.Net.Http;

namespace aiservice.Services
{
    public class TwitterService
    {
        private static string label = "Services";
        private static string className = "TwitterService";

        public static async Task<string> Bearer(AppSettings appSettings)
        {
            string methodName = "Bearer";
            try
            {
                string url = "oauth2/token";
                string auth = $"{appSettings.TwitterSettings.ConsumerKey}:{appSettings.TwitterSettings.ConsumerSecret}";
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["grant_type"] = "client_credentials";
                var response = await CommonService.HttpRequestContent(appSettings, appSettings.TwitterSettings.UrlApiBase, url, "POST", new FormUrlEncodedContent(parameters), null, "Basic", auth);
                response.EnsureSuccessStatusCode();
                string responseString = await response.Content.ReadAsStringAsync();
                JObject responseJson = CommonService.StringToJObject(responseString);
                return responseJson["access_token"].ToString();
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }
        public static string OAuth(AppSettings appSettings, string url, string method, Dictionary<string, string> queryparams)
        {
            DateTime DateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            int TimeStamp = (int)(DateTime.UtcNow - DateTime).TotalSeconds;
            double EpochTimeStamp = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            Uri Uri = new Uri(url);
            //string OauthTimestamp = Convert.ToInt64(EpochTimeStamp).ToString();
            string OauthTimestamp = TimeStamp.ToString();
            string OauthNonce = Guid.NewGuid().ToString(); //DateTime.Now.Ticks.ToString();
            string OauthSignatureMethod = "HMAC-SHA1";
            string OauthVersion = "1.0";
            Dictionary<string, string> SignatureParameters = new Dictionary<string, string>() {
                { "oauth_consumer_key", appSettings.TwitterSettings.ConsumerKey },
                { "oauth_signature_method", OauthSignatureMethod },
                { "oauth_token", appSettings.TwitterSettings.AccessToken },
                { "oauth_timestamp", OauthTimestamp },
                { "oauth_version", OauthVersion },
                { "oauth_nonce", OauthNonce }
            };
            if (queryparams != null)
            {
                queryparams.ToList().ForEach(x => SignatureParameters.Add(x.Key, x.Value));
            }
            string ResourceUrl = Uri.ToString().Contains("?") ? Uri.ToString().Substring(0, Uri.ToString().IndexOf("?")) : Uri.ToString();
            string BaseString = string.Join("&", SignatureParameters.OrderBy(KeyValuePair => KeyValuePair.Key).Select(KeyValuePair => $"{KeyValuePair.Key}={KeyValuePair.Value}"));
            BaseString = string.Concat(method.ToUpper(), "&", Uri.EscapeDataString(ResourceUrl), "&", Uri.EscapeDataString(BaseString));
            string OauthSignatureKey = string.Concat(Uri.EscapeDataString(appSettings.TwitterSettings.ConsumerSecret), "&", Uri.EscapeDataString(appSettings.TwitterSettings.AccessSecret));
            string OauthSignature;
            using (HMACSHA1 HMACSHA1 = new HMACSHA1(Encoding.ASCII.GetBytes(OauthSignatureKey)))
            {
                OauthSignature = Convert.ToBase64String(HMACSHA1.ComputeHash(Encoding.ASCII.GetBytes(BaseString)));
            }
            string HeaderFormat = "oauth_consumer_key=\"{3}\", oauth_nonce=\"{0}\", oauth_signature=\"{5}\", oauth_signature_method=\"{1}\", oauth_timestamp=\"{2}\", oauth_token=\"{4}\", oauth_version=\"{6}\"";
            string AuthorizationHeader = string.Format(HeaderFormat,
                Uri.EscapeDataString(OauthNonce),
                Uri.EscapeDataString(OauthSignatureMethod),
                Uri.EscapeDataString(OauthTimestamp),
                Uri.EscapeDataString(appSettings.TwitterSettings.ConsumerKey),
                Uri.EscapeDataString(appSettings.TwitterSettings.AccessToken),
                Uri.EscapeDataString(OauthSignature),
                Uri.EscapeDataString(OauthVersion));
            return AuthorizationHeader;
        }

        public static async Task<dynamic> TweetsSearchStandard(AppSettings appSettings, JObject requestBody)
        {
            string methodName = "TweetsSearch";
            try
            {
                // https://developer.twitter.com/en/docs/twitter-api/v1/tweets/search/api-reference/get-search-tweets
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                string version = "1.1";
                string url = "search/tweets.json";
                Dictionary<string, string> queryparams = new Dictionary<string, string>();
                queryparams["q"] = requestBody["query"].ToString();
                if (requestBody["latitude"] != null && requestBody["longitude"] != null)
                {
                    queryparams["geocode"] = $"{requestBody["latitude"].ToString()},{requestBody["longitude"].ToString()},{requestBody["radius"].ToString() ?? "1"}km";
                }
                if (requestBody["lang"] != null)
                {
                    queryparams["lang"] = requestBody["lang"]?.ToString() ?? "es";
                }
                queryparams["result_type"] = requestBody["result_type"]?.ToString() ?? "recent";
                queryparams["count"] = requestBody["count"]?.ToString() ?? "100";
                if (requestBody["until"] != null)
                {
                    queryparams["until"] = requestBody["until"]?.ToString() ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
                }
                if (requestBody["since_id"] != null)
                {
                    queryparams["since_id"] = requestBody["since_id"].ToString();
                }
                if (requestBody["max_id"] != null)
                {
                    queryparams["max_id"] = requestBody["max_id"].ToString();
                }
                queryparams["include_entities"] = requestBody["include_entities"]?.ToString() ?? "true";
                var response = await CommonService.HttpRequestContent(appSettings, $"{appSettings.TwitterSettings.UrlApiBase}/{version}/", url + QueryString.Create(queryparams), "GET", null, null, "Bearer", appSettings.TwitterSettings.BearerToken);
                string responseString = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(responseString);
                }
                JObject responseJson = CommonService.StringToJObject(responseString);
                CommonService.JArrayToListJObject(responseJson["statuses"]).ForEach(jo =>
                {
                    result.Add(BuildSearchReponse(jo));
                });
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static async Task<dynamic> TweetsSearchPremium(AppSettings appSettings, JObject requestBody)
        {
            string methodName = "TweetsSearchPremium";
            try
            {
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                string version = "1.1";
                string url = "tweets/search/30day/dev.json";
                Dictionary<string, string> queryparams = new Dictionary<string, string>();
                queryparams["query"] = requestBody["query"].ToString();
                queryparams["maxResults"] = requestBody["count"]?.ToString() ?? "15";
                if (requestBody["fromDate"] != null)
                {
                    queryparams["fromDate"] = requestBody["fromDate"]?.ToString() ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
                }
                if (requestBody["toDate"] != null)
                {
                    queryparams["toDate"] = requestBody["toDate"]?.ToString() ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
                }
                var response = await CommonService.HttpRequestContent(appSettings, $"{appSettings.TwitterSettings.UrlApiBase}/{version}/", url + QueryString.Create(queryparams), "GET", null, null, "Bearer", appSettings.TwitterSettings.BearerToken);
                string responseString = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(responseString);
                }
                JObject responseJson = CommonService.StringToJObject(responseString);
                CommonService.JArrayToListJObject(responseJson["statuses"]).ForEach(jo =>
                {
                    result.Add(BuildSearchReponse(jo));
                });
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static Dictionary<string, object> BuildSearchReponse(JObject jo)
        {
            Dictionary<string, object> line = new Dictionary<string, object>();
            line["createdat"] = jo["created_at"];
            //try
            //{
            //    line["createdat"] = DateTime.ParseExact(jo["created_at"].ToString(), "ddd MMM dd HH:mm:ss +0000 yyyy", CultureInfo.InvariantCulture);
            //}
            //catch (Exception)
            //{
            //    line["createdat"] = "";
            //}
            line["id"] = jo["id_str"];
            line["text"] = jo["text"];
            JToken entities = JObject.Parse(jo["entities"].ToString());
            line["text"] = DescompressEntities(JToken.FromObject(line["text"]), entities, "");

            line["originaltext"] = null;
            JToken jt = jo["retweeted_status"] != null ? jo["retweeted_status"] : null;
            if (jt != null)
            {
                line["originaltext"] = jt["text"];
                entities = JObject.Parse(jt["entities"].ToString());
                line["originaltext"] = DescompressEntities(JToken.FromObject(line["originaltext"]), entities, "");
            }
            
            line["quotedtext"] = null;
            jt = jt != null ? jt["quoted_status"] : jo["quoted_status"];
            if (jt != null)
            {
                line["quotedtext"] = jt["text"];
                entities = JObject.Parse(jt["entities"].ToString());
                line["quotedtext"] = DescompressEntities(JToken.FromObject(line["quotedtext"]), entities, "");
            }

            line["retweeted"] = jo["retweeted"];
            line["retweets"] = jo["retweet_count"];
            line["favorited"] = jo["favorited"];
            line["favorites"] = jo["favorite_count"];
            if (jo["entities"] != null)
            {
                line["hashtags"] = jo["entities"]["hashtags"].ToList().Select(b => b["text"]?.ToString()).ToList();
                line["symbols"] = jo["entities"]["symbols"].ToList().Select(b => b["text"]?.ToString()).ToList();
                List<Dictionary<string, object>> user_mentions = new List<Dictionary<string, object>>();
                foreach (var jo2 in jo["entities"]["user_mentions"])
                {
                    user_mentions.Add(new Dictionary<string, object>() {
                                { "id", jo2["id_str"].ToString()},
                                { "name", jo2["name"].ToString()},
                                { "screenname", jo2["screen_name"].ToString()}
                            });
                }
                line["usermentions"] = user_mentions;
                line["urls"] = jo["entities"]["urls"].ToList().Select(b => b["url"]?.ToString()).ToList();
            }
            if (jo["truncated"].ToString() == "true")
            {

            }
            Dictionary<string, object> user = new Dictionary<string, object>();
            user["id"] = jo["user"]["id_str"];
            user["name"] = jo["user"]["name"];
            user["screenname"] = jo["user"]["screen_name"];
            user["profileimageurl"] = jo["user"]["profile_image_url_https"];
            
            user["description"] = jo["user"]["description"];
            entities = JObject.Parse(jo["user"]["entities"].ToString());
            user["description"] = DescompressEntities(JToken.FromObject(user["description"]), entities, "description");
            
            user["location"] = jo["user"]["location"];
            user["createdat"] = jo["user"]["created_at"];
            
            user["url"] = jo["user"]["url"];
            entities = JObject.Parse(jo["user"]["entities"].ToString());
            user["url"] = DescompressEntities(JToken.FromObject(user["url"]), entities, "urls");
            
            user["friends"] = jo["user"]["friends_count"];
            user["followers"] = jo["user"]["followers_count"];
            line["user"] = user;
            return line;
        }

        public static JToken DescompressEntities(JToken jt, JToken entities, string label = "")
        {
            if (label != "")
            {
                entities = entities[label];
            }
            if (entities != null && entities["urls"] != null && entities["urls"].Count() != 0)
            {
                foreach (var eu in entities["urls"])
                {
                    jt = jt.ToString().Replace(eu["url"].ToString(), eu["expanded_url"].ToString());
                }
            }
            return jt;
        }
    }
}
