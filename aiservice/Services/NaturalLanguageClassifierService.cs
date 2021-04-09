using IBM.Cloud.SDK.Core.Authentication.Iam;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AIService.Entities;
using AIService.Logs;
using Newtonsoft.Json;

namespace AIService.Services
{
    public class NaturalLanguageClassifierRequest
    {
        public string Apikey { get; set; }
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public string ModelId { get; set; }
        public string Text { get; set; }
    }

    public class NaturalLanguageClassifierService
    {
        private static string label = "Services";
        private static string className = "NaturalLanguageClassifierService";
        public static dynamic NaturalLanguageClassifier(AppSettings appSettings, NaturalLanguageClassifierRequest requestBody)
        {
            string methodName = "NaturalLanguageClassifier";
            dynamic result = new ExpandoObject();
            try
            {
                WatsonSettings settings = appSettings.WatsonServices.NaturalLanguageClassifier;
                IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{requestBody.Apikey}");
                IBM.Watson.NaturalLanguageClassifier.v1.NaturalLanguageClassifierService naturalLanguageClassifier = new IBM.Watson.NaturalLanguageClassifier.v1.NaturalLanguageClassifierService(authenticator);
                naturalLanguageClassifier.SetServiceUrl($"{requestBody.Endpoint}");

                // Preprocesando el texto
                string normalizedtext = requestBody.Text;
                normalizedtext = Regex.Replace(normalizedtext, @"[:][\\]", " ");
                normalizedtext = Regex.Replace(normalizedtext, @"[.\!?,\'/():<>|][\s]", " ");
                normalizedtext = Regex.Replace(normalizedtext, @"[.\!?,\'/():<>|]", " ");
                normalizedtext = Regex.Replace(normalizedtext, @"[\\][\s]", " ");
                normalizedtext = normalizedtext.Replace("\r\n", "\n");
                normalizedtext = Regex.Replace(normalizedtext, @"[\s]{2,}", " ");
                normalizedtext = normalizedtext.Replace("\"", "");
                normalizedtext = normalizedtext.ToLower();
                normalizedtext = Regex.Replace(normalizedtext, @"á", "a");
                normalizedtext = Regex.Replace(normalizedtext, @"é", "e");
                normalizedtext = Regex.Replace(normalizedtext, @"í", "i");
                normalizedtext = Regex.Replace(normalizedtext, @"ó", "o");
                normalizedtext = Regex.Replace(normalizedtext, @"ú", "u");
                normalizedtext = Regex.Replace(normalizedtext, @"à", "a");
                normalizedtext = Regex.Replace(normalizedtext, @"è", "e");
                normalizedtext = Regex.Replace(normalizedtext, @"ì", "i");
                normalizedtext = Regex.Replace(normalizedtext, @"ò", "o");
                normalizedtext = Regex.Replace(normalizedtext, @"ù", "u");
                normalizedtext = Regex.Replace(normalizedtext, @"â", "a");
                normalizedtext = Regex.Replace(normalizedtext, @"ê", "e");
                normalizedtext = Regex.Replace(normalizedtext, @"î", "i");
                normalizedtext = Regex.Replace(normalizedtext, @"ô", "o");
                normalizedtext = Regex.Replace(normalizedtext, @"û", "u");
                result = naturalLanguageClassifier.Classify(
                classifierId: requestBody.ModelId,
                text: normalizedtext
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

        public static dynamic NaturalLanguageClassifierTraining(AppSettings appSettings, IFormFile trainingDataFile, IFormCollection requestBody)
        {
            string methodName = "NaturalLanguageClassifierTraining";
            dynamic result = new ExpandoObject();
            try
            {
                WatsonSettings settings = appSettings.WatsonServices.NaturalLanguageClassifier;
                IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{requestBody["apikey"].ToString()}");
                IBM.Watson.NaturalLanguageClassifier.v1.NaturalLanguageClassifierService naturalLanguageClassifier = new IBM.Watson.NaturalLanguageClassifier.v1.NaturalLanguageClassifierService(authenticator);
                naturalLanguageClassifier.SetServiceUrl($"{requestBody["endpoint"].ToString()}");
                JObject metadatajson = JObject.FromObject(new
                {
                    language = "es",
                    name = requestBody["modelname"].ToString() ?? "NLCModel"
                });
                using (MemoryStream trainingData = new MemoryStream(), metadata = new MemoryStream(Encoding.Default.GetBytes(metadatajson.ToString())))
                {
                    trainingDataFile.CopyTo(trainingData);
                    result = naturalLanguageClassifier.CreateClassifier(
                        trainingMetadata: metadata,
                        trainingData: trainingData
                        ).Result;
                    return result;
                }
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {trainingDataFile.FileName}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static dynamic NaturalLanguageClassifierList(AppSettings appSettings, NaturalLanguageClassifierRequest requestBody)
        {
            string methodName = "NaturalLanguageClassifierList";
            dynamic result = new ExpandoObject();
            try
            {
                WatsonSettings settings = appSettings.WatsonServices.NaturalLanguageClassifier;
                IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{requestBody.Apikey}");
                IBM.Watson.NaturalLanguageClassifier.v1.NaturalLanguageClassifierService naturalLanguageClassifier = new IBM.Watson.NaturalLanguageClassifier.v1.NaturalLanguageClassifierService(authenticator);
                naturalLanguageClassifier.SetServiceUrl($"{requestBody.Endpoint}");
                result = naturalLanguageClassifier.ListClassifiers().Result;
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static dynamic NaturalLanguageClassifierDetail(AppSettings appSettings, NaturalLanguageClassifierRequest requestBody)
        {
            string methodName = "NaturalLanguageClassifierDetail";
            dynamic result = new ExpandoObject();
            try
            {
                WatsonSettings settings = appSettings.WatsonServices.NaturalLanguageClassifier;
                IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{requestBody.Apikey}");
                IBM.Watson.NaturalLanguageClassifier.v1.NaturalLanguageClassifierService naturalLanguageClassifier = new IBM.Watson.NaturalLanguageClassifier.v1.NaturalLanguageClassifierService(authenticator);
                naturalLanguageClassifier.SetServiceUrl($"{requestBody.Endpoint}");
                result = naturalLanguageClassifier.GetClassifier(
                    classifierId: requestBody.ModelId
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

        public static dynamic NaturalLanguageClassifierDelete(AppSettings appSettings, NaturalLanguageClassifierRequest requestBody)
        {
            string methodName = "NaturalLanguageClassifierDetail";
            dynamic result = new ExpandoObject();
            try
            {
                WatsonSettings settings = appSettings.WatsonServices.NaturalLanguageClassifier;
                IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{requestBody.Apikey}");
                IBM.Watson.NaturalLanguageClassifier.v1.NaturalLanguageClassifierService naturalLanguageClassifier = new IBM.Watson.NaturalLanguageClassifier.v1.NaturalLanguageClassifierService(authenticator);
                naturalLanguageClassifier.SetServiceUrl($"{requestBody.Endpoint}");
                result = naturalLanguageClassifier.DeleteClassifier(
                    classifierId: requestBody.ModelId
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
    }
}
