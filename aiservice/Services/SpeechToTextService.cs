using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Watson.SpeechToText.v1.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;
using AIService.Entities;
using AIService.Logs;
using System.IO;

namespace AIService.Services
{
    public class SpeechToTextRequest
    {
        public string Apikey { get; set; }
        public string Endpoint { get; set; }
        public string Url { get; set; }
        public string Model { get; set; }
        public List<string> Keywords { get; set; }
        public float? KeywordsThreshold { get; set; }
    }

    public class SpeechRecognitionResultsDTO
    {
        public string Text { get; set; }
        public List<SpeechRecognitionResults> SpeechRecognitionResults { get; set; }
    }

    public class SpeechToTextService
    {
        private static string label = "Services";
        private static string className = "SpeechToTextService";
        public static async Task<dynamic> SpeechToText(AppSettings appSettings, SpeechToTextRequest requestBody)
        {
            string methodName = "SpeechToText";
            dynamic result = new ExpandoObject();
            try
            {
                WatsonSettings settings = appSettings.WatsonServices.SpeechToText;
                IamAuthenticator authenticator = new IamAuthenticator(apikey: $"{requestBody.Apikey}");
                IBM.Watson.SpeechToText.v1.SpeechToTextService speechToText = new IBM.Watson.SpeechToText.v1.SpeechToTextService(authenticator);
                speechToText.SetServiceUrl($"{requestBody.Endpoint}");
                List<string> audioWavList = await CommonService.AudioToWav(appSettings, requestBody.Url);
                List<SpeechRecognitionResults> speechRecognitionResultsList = new List<SpeechRecognitionResults>();
                string text = "";
                foreach (var audioWav in audioWavList)
                {
                    string audioWavTemp = CommonService.GetExternalPlatforms(appSettings).STTAudioFileUrl + audioWav;
                    HttpClient client = new HttpClient();
                    byte[] audio = client.GetByteArrayAsync(audioWavTemp).Result;
                    SpeechRecognitionResults speechRecognitionResults = new SpeechRecognitionResults();
                    //HttpClient httpClient = new HttpClient();
                    //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/wav"));
                    //httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", $"apikey:{requestBody.Apikey}");
                    //HttpContent httpContent = new ByteArrayContent(audio);
                    //var r = await httpClient.PostAsync("https://stream.watsonplatform.net/speech-to-text/api/v1/recognize", httpContent);
                    try
                    {
                        speechRecognitionResults = speechToText.Recognize(
                            audio: new MemoryStream(audio),
                            contentType: "audio/wav",
                            model: requestBody.Model != null ? requestBody.Model : "es-ES_NarrowbandModel"
                            ).Result;
                        speechRecognitionResultsList.Add(speechRecognitionResults);
                        foreach (var item in speechRecognitionResults.Results)
                        {
                            text += item.Alternatives[0].Transcript;
                        }
                        SpeechRecognitionResultsDTO speechRecognitionResultsDTO = new SpeechRecognitionResultsDTO();
                        speechRecognitionResultsDTO.Text = text;
                        speechRecognitionResultsDTO.SpeechRecognitionResults = speechRecognitionResultsList;
                        result = speechRecognitionResultsDTO;
                    }
                    catch (Exception e)
                    {
                        Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                        Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                        result = e.Message;
                    }
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
