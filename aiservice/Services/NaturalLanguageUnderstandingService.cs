using Google.Cloud.Language.V1;
using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Watson.NaturalLanguageUnderstanding.v1.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using AIService.Entities;
using AIService.Logs;

namespace AIService.Services
{
    public class NaturalLanguageUnderstandingRequest
    {
        public string Apikey { get; set; }
        public string Endpoint { get; set; }
        public string Text { get; set; }
        public string Language { get; set; }
        public bool? ReturnAnalyzedText { get; set; }
        public long? LimitTextCharacters { get; set; }
        public Concepts Concepts { get; set; }
        public Emotion Emotion { get; set; }
        public Entities Entities { get; set; }
        public Keywords Keywords { get; set; }
        public SemanticRoles SemanticRoles { get; set; }
        public Sentiment Sentiment { get; set; }
        public Categories Categories { get; set; }
    }

    public class Concepts
    {
        public bool? Enable { get; set; }
        public long? Limit { get; set; }
    }

    public class Emotion
    {
        public bool? Enable { get; set; }
        public bool? Document { get; set; }
        public List<string> Targets { get; set; }
    }

    public class Entities
    {
        public bool? Enable { get; set; }
        public long? Limit { get; set; }
        public bool? Mentions { get; set; }
        public string Model { get; set; }
        public bool? Sentiment { get; set; }
        public bool? Emotion { get; set; }
    }

    public class Keywords
    {
        public bool? Enable { get; set; }
        public long? Limit { get; set; }
        public bool? Sentiment { get; set; }
        public bool? Emotion { get; set; }
    }

    public class SemanticRoles
    {
        public bool? Enable { get; set; }
        public long? Limit { get; set; }
        public bool? Keywords { get; set; }
        public bool? Entities { get; set; }
    }

    public class Sentiment
    {
        public bool? Enable { get; set; }
        public bool? Document { get; set; }
        public List<string> Targets { get; set; }
    }

    public class Categories
    {
        public bool? Enable { get; set; }
        public bool? Explanation { get; set; }
        public long? Limit { get; set; }
        public string Model { get; set; }
    }

    public class NaturalLanguageUnderstandingService
    {
        private static string label = "Services";
        private static string className = "NaturalLanguageUnderstandingService";
        public static dynamic NaturalLanguageUnderstanding(AppSettings appSettings, NaturalLanguageUnderstandingRequest requestBody, bool personalized = false)
        {
            string methodName = "NaturalLanguageUnderstanding";
            dynamic result = new ExpandoObject();
            try
            {
                WatsonSettings settings = appSettings.WatsonServices.NaturalLanguageUnderstanding;
                IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{requestBody.Apikey}");
                IBM.Watson.NaturalLanguageUnderstanding.v1.NaturalLanguageUnderstandingService naturalLanguageUnderstanding = new IBM.Watson.NaturalLanguageUnderstanding.v1.NaturalLanguageUnderstandingService($"{settings.Version}", authenticator);
                naturalLanguageUnderstanding.SetServiceUrl($"{requestBody.Endpoint}");
                Features features = new Features();
                if (requestBody.Concepts != null)
                {
                    features.Concepts = new ConceptsOptions();
                    features.Concepts.Limit = requestBody.Concepts.Limit | 5;
                }
                if (requestBody.Emotion != null)
                {
                    features.Emotion = new EmotionOptions();
                }
                if (requestBody.Entities != null)
                {
                    features.Entities = new EntitiesOptions();
                    features.Entities.Model = personalized ? requestBody.Entities.Model : null;
                    features.Entities.Limit = requestBody.Entities.Limit | 5;
                    features.Entities.Sentiment = requestBody.Entities.Sentiment | true;
                    features.Entities.Emotion = requestBody.Entities.Emotion | true;
                }
                if (requestBody.Keywords != null)
                {
                    features.Keywords = new KeywordsOptions();
                    features.Keywords.Limit = requestBody.Keywords.Limit | 5;
                    features.Keywords.Sentiment = requestBody.Keywords.Sentiment | true;
                    features.Keywords.Emotion = requestBody.Keywords.Emotion | true;
                }
                if (requestBody.SemanticRoles != null)
                {
                    features.SemanticRoles = new SemanticRolesOptions();
                    features.SemanticRoles.Limit = requestBody.SemanticRoles.Limit | 5;
                    features.SemanticRoles.Keywords = requestBody.SemanticRoles.Keywords | true;
                    features.SemanticRoles.Entities = requestBody.SemanticRoles.Entities | true;
                }
                if (requestBody.Sentiment != null)
                {
                    features.Sentiment = new SentimentOptions();
                }
                if (requestBody.Categories != null)
                {
                    features.Categories = new CategoriesOptions();
                    features.Categories.Explanation = requestBody.Categories.Explanation | true;
                    features.Categories.Limit = requestBody.Categories.Limit | 5;
                }
                result = naturalLanguageUnderstanding.Analyze(
                    features: features,
                    text: requestBody.Text,
                    returnAnalyzedText: requestBody.ReturnAnalyzedText,
                    language: requestBody.Language
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

        public async static Task<Dictionary<string, object>> GoogleNaturalLanguageUnderstanding(AppSettings appSettings, string text)
        {
            string methodName = "GoogleNaturalLanguageUnderstanding";
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                CommonService.SetGoogleCredentials(@"googleAI-Harima.json");
                var client = LanguageServiceClient.Create();
                AnalyzeSentimentResponse analyzeSentimentResponse = new AnalyzeSentimentResponse();
                try
                {
                    analyzeSentimentResponse = await client.AnalyzeSentimentAsync(new Document()
                    {
                        Content = text,
                        Type = Document.Types.Type.PlainText
                    });
                }
                catch (Exception e)
                {
                    Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {text}");
                    Log.Write(appSettings, LogEnum.ERROR.ToString(), $"02{Assembly.GetExecutingAssembly().GetName().Name}", MethodBase.GetCurrentMethod().ReflectedType.Name, methodName, $"Watson error: {e.Source + Environment.NewLine + e.Message}");
                }

                AnalyzeEntitiesResponse analyzeEntitiesResponse = new AnalyzeEntitiesResponse();
                try
                {
                    analyzeEntitiesResponse = await client.AnalyzeEntitiesAsync(new Document()
                    {
                        Content = text,
                        Type = Document.Types.Type.PlainText
                    });
                }
                catch (Exception e)
                {
                    Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {text}");
                    Log.Write(appSettings, LogEnum.ERROR.ToString(), $"02{Assembly.GetExecutingAssembly().GetName().Name}", MethodBase.GetCurrentMethod().ReflectedType.Name, methodName, $"Watson error: {e.Source + Environment.NewLine + e.Message}");
                }

                ClassifyTextResponse classifyTextResponse = new ClassifyTextResponse();
                try
                {
                    LanguageTranslatorRequest languageTranslatorRequest = new LanguageTranslatorRequest();
                    languageTranslatorRequest.Text = text;
                    languageTranslatorRequest.Source = "es";
                    languageTranslatorRequest.Target = "en";
                    string translation = await LanguageTranslatorService.GoogleLanguageTranslator(appSettings, languageTranslatorRequest);
                    classifyTextResponse = await client.ClassifyTextAsync(new Document()
                    {
                        Content = translation,
                        Type = Document.Types.Type.PlainText
                    });
                }
                catch (Exception e)
                {
                    Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {text}");
                    Log.Write(appSettings, LogEnum.ERROR.ToString(), $"02{Assembly.GetExecutingAssembly().GetName().Name}", MethodBase.GetCurrentMethod().ReflectedType.Name, methodName, $"Watson error: {e.Source + Environment.NewLine + e.Message}");
                }
                result["sentiment"] = analyzeSentimentResponse;
                result["entities"] = analyzeEntitiesResponse;
                result["clasifiy"] = classifyTextResponse;
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {text}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }
    }
}
