using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Watson.Assistant.v1;
using IBM.Watson.Assistant.v1.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AIService.Entities;
using Log = AIService.Logs.Log;
using System.Collections.Generic;

namespace AIService.Services
{
    public class WatsonAssistantService
    {
        private static string label = "Services";
        private static string className = "WatsonAssistantService";
        public static MessageResponse MessageRequestV1(AppSettings appSettings, JObject requestBody)
        {
            string methodName = "MessageRequestV1";
            MessageResponse result = new MessageResponse();
            try
            {
                WatsonAssistant settings = appSettings.WatsonServices.WatsonAssistant;
                IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{requestBody["apikey"]}");
                AssistantService assistant = new AssistantService($"{settings.Version}", authenticator);
                assistant.SetServiceUrl($"{requestBody["endpoint"].ToString()}");

                string message = requestBody["message"]?.ToString();
                message = Regex.Replace(message, @"\s+", " ");

                JObject context = requestBody["context"] != null ? requestBody["context"] as JObject : new JObject();
                Context wcontext = new Context();

                wcontext.ConversationId = context["conversation_id"] != null ? context["conversation_id"].ToString() : "";

                JObject system = requestBody["system"] != null ? requestBody["system"] as JObject : new JObject();
                Dictionary<string, object> systemResponse = new Dictionary<string, object>();
                systemResponse.Add("initialized", system["initialized"] != null ? system["initialized"] : new object { });
                systemResponse.Add("dialog_stack", system["dialog_stack"] != null ? system["dialog_stack"] : new object { });
                wcontext.System = systemResponse;

                JObject metadata = requestBody["metadata"] != null ? requestBody["metadata"] as JObject : new JObject();
                MessageContextMetadata wmetadata = new MessageContextMetadata();
                wmetadata.UserId = metadata["userid"]?.ToString();
                wcontext.Metadata = wmetadata;

                foreach (var item in context)
                {
                    if (!(item.Key == "conversation_id" || item.Key == "system" || item.Key == "metadata"))
                    {
                        wcontext.Add(item.Key, item.Value != null ? item.Value : "");
                    }
                }
                //List<RuntimeEntity> entities = new List<RuntimeEntity>();
                //entities.Add(new RuntimeEntity() { Entity = "name", Confidence = float.Parse("0.4") });
                result = assistant.Message(
                    workspaceId: $"{requestBody["workspaceid"]}",
                    input: new MessageInput()
                    {
                        Text = message
                    },
                    //entities: entities,
                    context: wcontext
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
