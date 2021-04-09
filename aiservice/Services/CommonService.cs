using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AIService.Entities;

namespace AIService.Services
{
    public class CommonService
    {
        private static HttpClient httpClient = new HttpClient();

        // Get AppSettings External Platforms
        public static ExternalPlatforms GetExternalPlatforms(AppSettings appSettings)
        {
            return appSettings.Environments.Find(x => x.Label == appSettings.Environment).ExternalPlatforms;
        }

        public static async Task<HttpResponseMessage> HttpRequestGetUri(AppSettings appSettings, string uri, Dictionary<string, string> headers, string authtype = null, string auth = null, string guidRequest = "")
        {
            try
            {
                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.Timeout = TimeSpan.FromMinutes(30);
                HttpRequestSetHeader(headers);
                HttpRequestSetAuth(authtype, auth);
                return await httpClient.GetAsync(uri);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Http Request Json
        public static async Task<HttpResponseMessage> HttpRequestJson(AppSettings appSettings, string baseaddress, string url, string method, string content, Dictionary<string, string> headers, string authtype = null, string auth = null, string guidRequest = "")
        {
            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                httpClient = new HttpClient(clientHandler);
                httpClient.BaseAddress = new Uri($"{baseaddress}");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.Timeout = TimeSpan.FromMinutes(30);
                HttpRequestSetHeader(headers);
                HttpRequestSetAuth(authtype, auth);
                switch (method.ToUpper())
                {
                    case "GET":
                        return await httpClient.GetAsync(url);
                    case "POST":
                        return await httpClient.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
                    case "PUT":
                        return await httpClient.PutAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
                    default:
                        return await httpClient.GetAsync(url);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Http Request Json
        public static async Task<HttpResponseMessage> HttpRequestXML(AppSettings appSettings, string baseaddress, string url, string method, string content, Dictionary<string, string> headers, string authtype = null, string auth = null, string guidRequest = "")
        {
            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                httpClient = new HttpClient(clientHandler);
                httpClient.BaseAddress = new Uri($"{baseaddress}");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.Timeout = TimeSpan.FromMinutes(30);
                HttpRequestSetHeader(headers);
                HttpRequestSetAuth(authtype, auth);
                switch (method.ToUpper())
                {
                    case "GET":
                        return await httpClient.GetAsync(url);
                    case "POST":
                        return await httpClient.PostAsync(url, new StringContent(content, Encoding.UTF8, "text/xml"));
                    case "PUT":
                        return await httpClient.PutAsync(url, new StringContent(content, Encoding.UTF8, "text/xml"));
                    default:
                        return await httpClient.GetAsync(url);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Http Request Http Content
        public static async Task<HttpResponseMessage> HttpRequestContent(AppSettings appSettings, string baseaddress, string url, string method, HttpContent content, Dictionary<string, string> headers, string authtype = null, string auth = null, string guidRequest = "")
        {
            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                httpClient = new HttpClient(clientHandler);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.Timeout = TimeSpan.FromMinutes(30);
                HttpRequestSetHeader(headers);
                HttpRequestSetAuth(authtype, auth);
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
                httpRequestMessage.RequestUri = new Uri(baseUri: new Uri(baseaddress), relativeUri: url);
                switch (method.ToUpper())
                {
                    case "GET":
                        httpRequestMessage.Method = HttpMethod.Get;
                        break;
                    case "POST":
                        httpRequestMessage.Method = HttpMethod.Post;
                        break;
                    case "PUT":
                        httpRequestMessage.Method = HttpMethod.Put;
                        break;
                    case "DELETE":
                        httpRequestMessage.Method = HttpMethod.Delete;
                        break;
                    case "HEAD":
                        httpRequestMessage.Method = HttpMethod.Head;
                        break;
                    default:
                        break;
                }
                if (content != null)
                {
                    httpRequestMessage.Content = content;
                }
                return await httpClient.SendAsync(httpRequestMessage);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Http Request Set Header
        public static void HttpRequestSetHeader(Dictionary<string, string> headers)
        {
            try
            {
                if (headers != null)
                {
                    foreach (var h in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(h.Key, h.Value);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Http Request Set Auth
        public static void HttpRequestSetAuth(string authtype, string auth)
        {
            try
            {
                if (authtype != null)
                {
                    switch (authtype.ToUpper())
                    {
                        case "BASIC":
                            if (auth.Contains(":"))
                            {
                                byte[] AuthorizationArray = Encoding.ASCII.GetBytes(auth);
                                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(AuthorizationArray));
                            }
                            else
                            {
                                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                            }
                            break;
                        case "BEARER":
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth);
                            break;
                        case "OAUTH":
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", auth);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Set Google Credentials
        public static void SetGoogleCredentials(string filepath)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filepath);
        }

        // Replace Unicode to UTF-8
        public static string ReplaceUnicode(string data)
        {
            data = data.Replace("\\u00c1", "Á");
            data = data.Replace("\\u00c9", "É");
            data = data.Replace("\\u00cd", "Í");
            data = data.Replace("\\u00d3", "Ó");
            data = data.Replace("\\u00da", "Ú");
            data = data.Replace("\\u00e1", "á");
            data = data.Replace("\\u00e9", "é");
            data = data.Replace("\\u00ed", "í");
            data = data.Replace("\\u00f3", "ó");
            data = data.Replace("\\u00fa", "ú");
            data = data.Replace("\\u00f1", "ñ");
            data = data.Replace("\\u00d1", "Ñ");
            data = data.Replace("u00c1", "Á");
            data = data.Replace("u00c9", "É");
            data = data.Replace("u00cd", "Í");
            data = data.Replace("u00d3", "Ó");
            data = data.Replace("u00da", "Ú");
            data = data.Replace("u00e1", "á");
            data = data.Replace("u00e9", "é");
            data = data.Replace("u00ed", "í");
            data = data.Replace("u00f3", "ó");
            data = data.Replace("u00fa", "ú");
            data = data.Replace("u00f1", "ñ");
            data = data.Replace("u00d1", "Ñ");
            return data;
        }

        // XML To JObject
        public static JObject XMLToJObject(string xmlString)
        {
            if (xmlString == null) return new JObject();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlString);
            string jsonString = JsonConvert.SerializeXmlNode(xmlDocument);
            JObject jObject = JObject.Parse(jsonString);
            return jObject;
        }

        // String To JObject
        public static JObject StringToJObject(string data)
        {
            if (data == null) return new JObject();
            if (data != "" && data != "[]")
            {
                try
                {
                    return JObject.Parse(data);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                return new JObject();
            }
        }

        // JObject To Dictionary
        public static Dictionary<string, object> JObjectToDictionary(dynamic data)
        {
            if (data == null) return new Dictionary<string, object>();
            try
            {
                return new Dictionary<string, object>(data.ToObject<IDictionary<string, object>>(), StringComparer.InvariantCultureIgnoreCase);
            }
            catch (Exception)
            {
                return new Dictionary<string, object>();
            }
        }

        // JObject To Dictionary
        public static Dictionary<string, string> JObjectToDictionaryString(dynamic data)
        {
            if (data == null) return new Dictionary<string, string>();
            try
            {
                return new Dictionary<string, string>(data.ToObject<IDictionary<string, string>>(), StringComparer.InvariantCultureIgnoreCase);
            }
            catch (Exception)
            {
                return new Dictionary<string, string>();
            }
        }

        // Jarray To List<Dictionary>
        public static List<Dictionary<string, object>> JArrayToListDictionary(dynamic data)
        {
            try
            {
                List<Dictionary<string, object>> dicts = new List<Dictionary<string, object>>();
                foreach (JObject jo in data)
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dicts.Add(JObjectToDictionary(jo));
                }
                return dicts;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Jarray To List<JObject>
        public static List<JObject> JArrayToListJObject(dynamic data)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<JObject>>(data.ToString()); ;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Jarray To Dictionary
        public static Dictionary<string, object> JArrayToDictionary(JArray jArray, string key, string value)
        {
            try
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                foreach (JObject ja in jArray)
                {
                    dict[ja[key].ToString()] = ja[value];
                }
                return dict;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Convert JToken to Type
        public static dynamic ConvertJTokenToType(JToken jToken)
        {
            switch (jToken.Type.ToString())
            {
                case "Integer":
                    return jToken.Value<int>();
                default:
                    break;
            }
            return jToken;
        }

        // Generate random String
        public static string GenerateString(int lengthcharacters)
        {
            char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            Random rand = new Random();
            string result = "";
            for (int i = 1; i <= lengthcharacters; i++)
            {
                result += letters[rand.Next(0, letters.Length - 1)];
            }
            return result;
        }

        // Get File Last Modificate Date
        public static DateTime GetFileLastDate(string filepath)
        {
            try
            {
                return File.GetLastWriteTime(filepath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Save json file
        public static void SaveJson(string filename, dynamic data)
        {
            using (StreamWriter file = File.CreateText(string.Concat(filename + ".json")))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, data);
            }
        }

        // Load json file
        public static async Task<string> LoadJson(string filename)
        {
            try
            {
                using (StreamReader r = new StreamReader(string.Concat(filename + ".json")))
                {
                    return await r.ReadToEndAsync();
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        // Encriptation Methods
        public static string DecryptStringAES(AppSettings appSettings, string guidRequest, string cipherText, string keyString = "")
        {
            string keyStringVariable = keyString;
            string dateKey = (String.Format("{0:yyyyMMdd}", DateTime.UtcNow) + String.Format("{0:yyyyMMdd}", DateTime.UtcNow));
            if (keyStringVariable == "")
            {
                keyStringVariable = dateKey;
            }
            var keybytes = Encoding.UTF8.GetBytes(keyStringVariable);
            var iv = Encoding.UTF8.GetBytes(keyStringVariable);
            var encrypted = Convert.FromBase64String(cipherText);
            var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);
            if (decriptedFromJavascript == "keyError")
            {
                keyStringVariable = (String.Format("{0:yyyyMMdd}", DateTime.UtcNow.AddDays(-1)) + String.Format("{0:yyyyMMdd}", DateTime.UtcNow.AddDays(-1)));
                keybytes = Encoding.UTF8.GetBytes(keyStringVariable);
                iv = Encoding.UTF8.GetBytes(keyStringVariable);
                encrypted = Convert.FromBase64String(cipherText);
                decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);
            }
            if (decriptedFromJavascript == "keyError")
            {
                keyStringVariable = (String.Format("{0:yyyyMMdd}", DateTime.UtcNow.AddDays(1)) + String.Format("{0:yyyyMMdd}", DateTime.UtcNow.AddDays(1)));
                keybytes = Encoding.UTF8.GetBytes(keyStringVariable);
                iv = Encoding.UTF8.GetBytes(keyStringVariable);
                encrypted = Convert.FromBase64String(cipherText);
                decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);
                return decriptedFromJavascript;
            }
            else
            {
                return decriptedFromJavascript;
            }
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.  
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                try
                {
                    using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {

                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    plaintext = "keyError";
                }
            }
            return plaintext.ToString();
        }

        public static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        // Text to List JObject
        public static async Task<List<JObject>> TransformTextToListJObject(AppSettings appSettings, string filePath, string filterString, int startRow)
        {
            List<string> rows = await FileToListString(appSettings, filePath, filterString);
            string header = rows[startRow].Split("#Fields: ")[1];
            List<string> headers = new List<string>();
            headers = header.Split(" ").ToList();
            rows = rows.FindAll(x => !x.StartsWith("#"));
            rows = rows.FindAll(x => x != "");
            List<JObject> jObjects = new List<JObject>();
            rows.ForEach(row =>
            {
                List<string> columns = new List<string>();
                columns = row.Split(" ").ToList();
                JObject jObject = new JObject();
                for (int i = 0; i < columns.Count; i++)
                {
                    try
                    {
                        jObject.Add(i <= headers.Count ? headers[i] : "", new JValue(columns[i]));
                    }
                    catch (Exception) { }
                }
                jObjects.Add(jObject);
            });
            return jObjects;
        }

        // File to string
        public static async Task<List<string>> FileToListString(AppSettings appSettings, string filePath, string filterString)
        {
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                List<string> text = new List<string>();
                string line;
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    if (text.Count < 5)
                    {
                        text.Add(line);
                    }
                    else if (line.ToLower().Contains(filterString.ToLower()))
                    {
                        text.Add(line);
                    }
                }
                return text;
            }
        }

        public static async Task<List<string>> FormDataToListString(IFormCollection collection)
        {
            List<string> result = new List<string>();
            if (collection.Files.Count > 0)
            {
                using (StreamReader sr = new StreamReader(collection.Files[0].OpenReadStream()))
                {
                    string line;
                    while ((line = await sr.ReadLineAsync()) != null)
                    {
                        result.Add(line);
                    }
                }
            }
            return result;
        }

        public static async Task<JObject> FormDataToJObject(IFormCollection collection)
        {
            return await Task.Run(() => {
                JObject result = new JObject();
                if (collection.Keys.Count > 0)
                {
                    foreach (var k in collection.Keys)
                    {
                        try
                        {
                            result[k] = JObject.Parse(collection[k]);
                        }
                        catch
                        {
                            result[k] = new JValue(collection[k]);
                        }
                    }
                }
                return result;
            });
        }

        public async static Task<List<string>> AudioToWav(AppSettings appSettings, string url)
        {
            var speechToTextUrl = CommonService.GetExternalPlatforms(appSettings).AudioToWavUrl;
            HttpClient client = new HttpClient();
            PythonSpeechToText speechToText = new PythonSpeechToText();
            speechToText.Path = CommonService.GetExternalPlatforms(appSettings).STTAudioFilePath;
            speechToText.Tipo = "";
            speechToText.Url = url;
            speechToText.Mensaje = "";
            speechToText.Respuesta = "";
            StringContent body = new StringContent(JsonConvert.SerializeObject(speechToText), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(speechToTextUrl, body);
            string result = await response.Content.ReadAsStringAsync();
            result = result.Substring(2, result.Length - 5).Replace("\\\\", "\\").Replace("\\\"", "\"").Replace("\n", "").Replace("\"{", "{").Replace("}\"", "}");
            result = CommonService.ReplaceUnicode(result);
            return new List<string>(result.Split("\",\""));
        }
    }
}
