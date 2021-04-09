using AIService.Entities;
using AIService.Logs;
using AIService.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AIService.Services
{
    public class PCloudService
    {
        private static string label = "Services";
        private static string className = "PCloudService";

        public static async Task<Dictionary<string, string>> SetAuth(AppSettings appSettings, Dictionary<string, string> query_params)
        {
            if (!query_params.ContainsKey("auth"))
            {
                JObject login = CommonService.StringToJObject(await Login(appSettings, new Dictionary<string, string>()));
                query_params["auth"] = login["auth"].ToString();
            }
            return query_params;
        }

        public static Dictionary<string, string> ValidateFolder(Dictionary<string, string> query_params)
        {
            if (!query_params.ContainsKey("path") && !query_params.ContainsKey("folderid"))
            {
                query_params["path"] = "/";
            }
            return query_params;
        }

        #region PCloudLogin
        const string loginURL = "userinfo";
        public static async Task<string> Login(AppSettings appSettings, Dictionary<string, string> query_params)
        {
            query_params["getauth"] = "1";
            query_params["logout"] = "1";
            query_params["username"] = query_params.ContainsKey("username") ? query_params["username"] : appSettings.PCloudSettings.Username;
            query_params["password"] = query_params.ContainsKey("password") ? query_params["password"] : appSettings.PCloudSettings.Password;
            return await (await CommonService.HttpRequestContent(appSettings, appSettings.PCloudSettings.UrlApiBase, loginURL + QueryString.Create(query_params), "GET", null, null)).Content.ReadAsStringAsync();
        }
        #endregion
        
        #region PCloudFolder
        public static async Task<string> Folder(AppSettings appSettings, Dictionary<string, string> query_params, string operation)
        {
            query_params = await SetAuth(appSettings, query_params);
            string url = "";
            switch (operation.ToLower())
            {
                case "create":
                    url = "createfolderifnotexists";
                    break;
                case "list":
                    url = "listfolder";
                    break;
                case "rename":
                    url = "renamefolder";
                    break;
                case "delete":
                    url = "deletefolder";
                    break;
                case "copy":
                    url = "copyfolder";
                    break;
                default:
                    break;
            }
            query_params = ValidateFolder(query_params);
            return await (await CommonService.HttpRequestContent(appSettings, appSettings.PCloudSettings.UrlApiBase, url + QueryString.Create(query_params), "GET", null, null)).Content.ReadAsStringAsync();
        }
        #endregion

        #region PCloudFolder
        public static async Task<string> FileUpload(AppSettings appSettings, IFormCollection requestBody)
        {
            string methodName = "FileUpload";
            try
            {
                string url = "uploadfile";
                HttpContent streamContent;
                if (requestBody.Files[0].ContentType.Contains("text/"))
                {
                    streamContent = new StringContent(await new StreamReader(requestBody.Files[0].OpenReadStream()).ReadToEndAsync(), Encoding.UTF8);
                }
                else
                {
                    streamContent = new StreamContent(requestBody.Files[0].OpenReadStream());
                }
                Dictionary<string, string> query_params = requestBody.Keys.ToDictionary(k => k, v => requestBody[v].ToString());
                query_params = await SetAuth(appSettings, query_params);
                query_params = ValidateFolder(query_params);
                return await (await CommonService.HttpRequestContent(appSettings, appSettings.PCloudSettings.UrlApiBase, url + QueryString.Create(query_params), "POST", streamContent, null)).Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static async Task<string> File(AppSettings appSettings, Dictionary<string, string> query_params, string operation)
        {
            query_params = await SetAuth(appSettings, query_params);
            string url = "";
            switch (operation.ToLower())
            {
                case "uploadprogress":
                    url = "uploadprogress";
                    break;
                case "download":
                    url = "downloadfileasync";
                    break;
                case "copy":
                    url = "copyfile";
                    break;
                case "delete":
                    url = "deletefile";
                    break;
                case "rename":
                    url = "renamefile";
                    break;
                case "detail":
                    url = "stat";
                    break;
                default:
                    break;
            }
            query_params = ValidateFolder(query_params);
            return await (await CommonService.HttpRequestContent(appSettings, appSettings.PCloudSettings.UrlApiBase, url + QueryString.Create(query_params), "GET", null, null)).Content.ReadAsStringAsync();
        }
        #endregion
    }
}
