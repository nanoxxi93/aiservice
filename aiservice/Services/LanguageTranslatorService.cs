using IBM.Cloud.SDK.Core.Authentication.Iam;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIService.Entities;
using AIService.Logs;
using System.Dynamic;
using Newtonsoft.Json;
using Google.Cloud.Translation.V2;

namespace AIService.Services
{
    public class LanguageTranslatorRequest
    {
        public string Apikey { get; set; }
        public string Endpoint { get; set; }
        public string Text { get; set; }
        public string ModelId { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
    }

    public class LanguageTranslatorService
    {
        private static string label = "Services";
        private static string className = "LanguageTranslatorService";
        public static dynamic LanguageTranslator(AppSettings appSettings, LanguageTranslatorRequest requestBody)
        {
            string methodName = "LanguageTranslator";
            dynamic result = new ExpandoObject();
            try
            {
                WatsonSettings settings = appSettings.WatsonServices.LanguageTranslator;
                IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{requestBody.Apikey}");
                IBM.Watson.LanguageTranslator.v3.LanguageTranslatorService languageTranslator = new IBM.Watson.LanguageTranslator.v3.LanguageTranslatorService($"{settings.Version}", authenticator);
                languageTranslator.SetServiceUrl($"{requestBody.Endpoint}");
                List<string> text = new List<string>();
                text.Add(requestBody.Text);
                result = languageTranslator.Translate(
                text: text,
                modelId: requestBody.ModelId,
                source: requestBody.Source,
                target: requestBody.Target != null ? requestBody.Target : "en"
                ).Result;
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static async Task<dynamic> GoogleLanguageTranslator(AppSettings appSettings, LanguageTranslatorRequest requestBody)
        {
            string methodName = "GoogleLanguageTranslator";
            dynamic result = new ExpandoObject();
            try
            {
                TranslationClient client = TranslationClient.CreateFromApiKey(requestBody.Apikey);
                //Detection detection = await client.DetectLanguageAsync(requestBody);
                TranslationResult googleTranslationResult = await client.TranslateTextAsync(
                text: requestBody.Text,
                targetLanguage: requestBody.Target,
                sourceLanguage: requestBody.Source);
                result = googleTranslationResult.TranslatedText;
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
