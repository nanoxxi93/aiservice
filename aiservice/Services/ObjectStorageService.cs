using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AIService.Entities;
using AIService.Logs;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Xml;

namespace AIService.Services
{
    public class ObjectStorageService
    {
        private static string label = "Services";
        private static string className = "ObjectStorageService";
        public static async Task<Dictionary<string, object>> IamToken(AppSettings appSettings, string apiKey)
        {
            string methodName = "IamToken";
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                string url = "https://iam.cloud.ibm.com/oidc/token";
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["apikey"] = apiKey;
                parameters["response_type"] = "cloud_iam";
                parameters["grant_type"] = "urn:ibm:params:oauth:grant-type:apikey";
                HttpResponseMessage response = await CommonService.HttpRequestContent(appSettings, url, "", "POST", new FormUrlEncodedContent(parameters), null);
                JObject jsonResult = JObject.Parse(await response.Content.ReadAsStringAsync());
                result = CommonService.JObjectToDictionary(jsonResult);
                result["_statuscode"] = ((int)response.StatusCode).ToString() + response.StatusCode;
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {apiKey}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static async Task<Dictionary<string, object>> Options(AppSettings appSettings, Dictionary<string, object> requestBody)
        {
            string methodName = "Options";
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                Dictionary<string, object> iamToken = await IamToken(appSettings, requestBody["apikey"].ToString());
                if (iamToken["_statuscode"].ToString().StartsWith("4"))
                {
                    throw new Exception(iamToken["errorMessage"].ToString());
                }
                string url = requestBody["url"].ToString();
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("ibm-service-instance-id", requestBody["resourceid"].ToString());
                Dictionary<string, string> queryparams = new Dictionary<string, string>();
                HttpResponseMessage response = new HttpResponseMessage();
                switch (requestBody["operation"].ToString().ToLower())
                {
                    case "getbuckets":
                        response = await CommonService.HttpRequestJson(appSettings, url, "", "GET", "", headers, "Bearer", iamToken["access_token"].ToString());
                        break;
                    case "getbucket":
                        response = await CommonService.HttpRequestJson(appSettings, url, (string)requestBody["bucket"], "GET", "", headers, "Bearer", iamToken["access_token"].ToString());
                        break;
                    case "getmultipart":
                        response = await CommonService.HttpRequestJson(appSettings, url, (string)requestBody["bucket"] + "?uploads=", "GET", "", headers, "Bearer", iamToken["access_token"].ToString());
                        break;
                    case "cancelmultipart":
                        queryparams = new Dictionary<string, string>()
                        {
                            {"uploadId", (string)requestBody["uploadid"]}
                        };
                        response = await CommonService.HttpRequestJson(appSettings, url, (string)requestBody["bucket"] + "/" + requestBody["objectkey"] + QueryString.Create(queryparams), "DELETE", "", headers, "Bearer", iamToken["access_token"].ToString());
                        break;
                    default:
                        break;
                }
                result = CommonService.JObjectToDictionary(CommonService.XMLToJObject(await response.Content.ReadAsStringAsync()));
                result["_statuscode"] = response.StatusCode + ((int)response.StatusCode).ToString();
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static async Task<Dictionary<string, object>> Upload(AppSettings appSettings, IFormCollection requestBody)
        {
            string methodName = "Upload";
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                Dictionary<string, object> iamToken = await IamToken(appSettings, requestBody["apikey"].ToString());
                if (iamToken["_statuscode"].ToString().StartsWith("4"))
                {
                    throw new Exception(iamToken["errorMessage"].ToString());
                }
                string url = requestBody["url"];
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("ibm-service-instance-id", requestBody["resourceid"].ToString());
                HttpResponseMessage response = new HttpResponseMessage();
                HttpContent streamContent;
                streamContent = new StreamContent(requestBody.Files[0].OpenReadStream());
                response = await CommonService.HttpRequestContent(appSettings, url, requestBody["bucket"] + "/asdasd/" + requestBody["objectkey"], "PUT", streamContent, headers, "Bearer", iamToken["access_token"].ToString());
                result["result"] = await response.Content.ReadAsStringAsync();
                result["_statuscode"] = ((int)response.StatusCode).ToString() + response.StatusCode;
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static async Task<Dictionary<string, object>> MultipartUpload(AppSettings appSettings, IFormCollection requestBody)
        {
            string methodName = "MultipartUpload";
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                Dictionary<string, object> iamToken = await IamToken(appSettings, requestBody["apikey"].ToString());
                if (iamToken["_statuscode"].ToString().StartsWith("4"))
                {
                    throw new Exception(iamToken["errorMessage"].ToString());
                }
                string url = requestBody["url"];
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("ibm-service-instance-id", requestBody["resourceid"].ToString());
                HttpResponseMessage response = new HttpResponseMessage();
                response = await CommonService.HttpRequestContent(appSettings, url, requestBody["bucket"] + "/" + requestBody["objectkey"] + "?uploads=", "POST", null, headers, "Bearer", iamToken["access_token"].ToString());
                string responseString = await response.Content.ReadAsStringAsync();
                JObject responseJSON = CommonService.XMLToJObject(responseString);
                string uploadId = responseJSON["InitiateMultipartUploadResult"]["UploadId"].ToString();

                const int MAX_BUFFER = 20*1024*1024; //20MB this is the chunk size read from file
                byte[] buffer = new byte[MAX_BUFFER];
                long bytes = requestBody.Files[0].Length;
                double parts = Math.Ceiling((double)bytes / (double)MAX_BUFFER);

                Dictionary<string, string> queryparams = new Dictionary<string, string>();
                XmlDocument xmlDocument = new XmlDocument();
                XmlElement completeMultipartUpload = xmlDocument.CreateElement("CompleteMultipartUpload");
                HttpContent streamContent;
                using (Stream stream = requestBody.Files[0].OpenReadStream())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int partNumber = 1;
                        for (int i = 0; i < parts; i++)
                        {
                            int read = stream.Read(buffer, 0, buffer.Length); //read each chunk
                            ms.Write(buffer, 0, read); //write chunk to [wherever]
                            queryparams = new Dictionary<string, string>
                            {
                                { "partNumber", partNumber.ToString() },
                                { "uploadId", uploadId }
                            };
                            if (requestBody.Files[0].ContentType.Contains("text/"))
                            {
                                streamContent = new StringContent(await new StreamReader(new MemoryStream(buffer)).ReadToEndAsync(), Encoding.UTF8);
                            }
                            else
                            {
                                streamContent = new StreamContent(new MemoryStream(buffer));
                            }
                            response = await CommonService.HttpRequestContent(appSettings, url, requestBody["bucket"] + "/" + requestBody["objectkey"] + QueryString.Create(queryparams), "PUT", streamContent, headers, "Bearer", iamToken["access_token"].ToString());
                            XmlElement part = xmlDocument.CreateElement("Part");
                            XmlElement partnumber = xmlDocument.CreateElement("PartNumber");
                            partnumber.InnerText = partNumber.ToString();
                            XmlElement etag = xmlDocument.CreateElement("ETag");
                            etag.InnerText = response.Headers.ETag.Tag.Replace("\"", "");
                            part.AppendChild(partnumber);
                            part.AppendChild(etag);
                            completeMultipartUpload.AppendChild(part);
                            partNumber += 1;
                        }
                    }
                }
                xmlDocument.AppendChild(completeMultipartUpload);
                queryparams = new Dictionary<string, string>
                {
                    { "uploadId", uploadId }
                };
                response = await CommonService.HttpRequestXML(appSettings, url, requestBody["bucket"] + "/" + requestBody["objectkey"] + QueryString.Create(queryparams), "POST", xmlDocument.InnerXml, headers, "Bearer", iamToken["access_token"].ToString());
                result["result"] = await response.Content.ReadAsStringAsync();
                result["_statuscode"] = ((int)response.StatusCode).ToString() + response.StatusCode;
                return result;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static async Task<byte[]> Download(AppSettings appSettings, Dictionary<string, object> requestBody)
        {
            string methodName = "Download";
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                Dictionary<string, object> iamToken = await IamToken(appSettings, requestBody["apikey"].ToString());
                string url = requestBody["url"].ToString();
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("ibm-service-instance-id", requestBody["resourceid"].ToString());
                HttpResponseMessage response = new HttpResponseMessage();
                response = await CommonService.HttpRequestJson(appSettings, url, requestBody["bucket"] + "/" + requestBody["objectkey"], "GET", "", headers, "Bearer", iamToken["access_token"].ToString());
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static async Task<byte[]> DownloadHMAC(AppSettings appSettings, Dictionary<string, object> requestBody)
        {
            string methodName = "DownloadHMAC";
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                string accessKey = requestBody["accessKey"].ToString();
                string secretKey = requestBody["secretKey"].ToString();
                string host = requestBody["host"].ToString();
                string region = "";
                string endpoint = "https://" + host;
                DateTime time = DateTime.UtcNow;
                string datestamp = time.ToString("yyyyMMdd");
                string timestamp = datestamp + "T" + time.ToString("HHmmss") + "Z";
                int expiration = 86400;
                string standardizedQuerystring = "X-Amz-Algorithm=AWS4-HMAC-SHA256" +
                "&X-Amz-Credential=" + Uri.EscapeDataString(accessKey + "/" + datestamp + "/" + region + "/s3/aws4_request") +
                "&X-Amz-Date=" + timestamp +
                "&X-Amz-Expires=" + expiration.ToString() +
                "&X-Amz-SignedHeaders=host";

                string standardizedResource = "/" + requestBody["bucket"] + "/" + requestBody["objectkey"];

                string payloadHash = "UNSIGNED-PAYLOAD";
                string standardizedHeaders = "host:" + host;
                string signedHeaders = "host";

                string standardizedRequest = HttpMethod.Get + "\n" +
                    standardizedResource + "\n" +
                    standardizedQuerystring + "\n" +
                    standardizedHeaders + "\n" +
                    "\n" +
                    signedHeaders + "\n" +
                    payloadHash;

                // assemble string-to-sign
                string hashingAlgorithm = "AWS4-HMAC-SHA256";
                string credentialScope = datestamp + "/" + region + "/" + "s3" + "/" + "aws4_request";
                string sts = hashingAlgorithm + "\n" +
                    timestamp + "\n" +
                    credentialScope + "\n" +
                    hashHex(standardizedRequest);

                // generate the signature
                byte[] signatureKey = createSignatureKey(secretKey, datestamp, region, "s3");
                string signature = hmacHex(signatureKey, sts);

                // create and send the request
                string requestUrl = endpoint + "/" +
                    requestBody["bucket"] + "/" +
                    requestBody["objectkey"] + "?" +
                    standardizedQuerystring +
                    "&X-Amz-Signature=" +
                    signature;

                HttpResponseMessage response = await CommonService.HttpRequestGetUri(appSettings, requestUrl, null);
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        private static string toHexString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] hash(byte[] key, string msg)
        {
            try
            {
                HMACSHA256 hmac = new HMACSHA256(key);
                hmac.Initialize();
                byte[] bytes = Encoding.UTF8.GetBytes(msg);
                byte[] rawHmac = hmac.ComputeHash(bytes);
                return rawHmac;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string hmacHex(byte[] key, string msg)
        {
            try
            {
                return toHexString(hash(key, msg));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string hashHex(string msg)
        {
            try
            {
                SHA256 sha1 = SHA256.Create();
                byte[] encodedhash = sha1.ComputeHash(Encoding.UTF8.GetBytes(msg));
                return toHexString(encodedhash);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // region is a wildcard value that takes the place of the AWS region value
        // as COS doesn"t use the same conventions for regions, this parameter can accept any string
        public static byte[] createSignatureKey(string key, string datestamp, string region, string service)
        {
            try
            {
                byte[] keyDate = hash(Encoding.UTF8.GetBytes("AWS4" + key), datestamp);
                byte[] keyString = hash(keyDate, region);
                byte[] keyService = hash(keyString, service);
                byte[] keySigning = hash(keyService, "aws4_request");
                return keySigning;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string createHexSignatureKey(string key, string datestamp, string region, string service)
        {
            try
            {
                return toHexString(createSignatureKey(key, datestamp, region, service));
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
