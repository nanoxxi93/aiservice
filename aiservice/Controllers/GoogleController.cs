using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using AIService.Entities;
using AIService.Logs;
using AIService.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using System.Security.Cryptography;
using Org.BouncyCastle.Security;
using aiservice.Services;

namespace AIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleController : ControllerBase
    {
        private static string label = "Controllers";
        private static string className = "GoogleController";
        private readonly AppSettings appSettings;
        IConfiguration configuration;

        public GoogleController(IOptionsSnapshot<AppSettings> appSettings, IConfiguration configuration)
        {
            this.appSettings = appSettings.Value;
            this.configuration = configuration;
        }

        [HttpPost("naturallanguageunderstanding")]
        public async Task<IActionResult> NaturalLanguageUnderstanding([FromBody] string request)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string methodName = "NaturalLanguageUnderstanding";
            ResponseDTO response = new ResponseDTO();
            try
            {
                Log.Write(appSettings, LogEnum.DEBUG.ToString(), label, className, methodName, $"REQUEST: {JsonConvert.SerializeObject(request)}");
                response.Result = await NaturalLanguageUnderstandingService.GoogleNaturalLanguageUnderstanding(appSettings, request);
                response.Success = true;
                watch.Stop();
                Log.Write(appSettings, LogEnum.INFO.ToString(), label, className, methodName, $"RESULT: {JsonConvert.SerializeObject(response)} Execution Time: {watch.ElapsedMilliseconds} ms");
                return Ok(response);
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Msg = e.Message;
                watch.Stop();
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(request)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                return BadRequest(response);
            }
        }

        [HttpPost("languagetranslator")]
        public async Task<IActionResult> LanguageTranslator([FromBody] JObject request)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string methodName = "LanguageTranslator";
            ResponseDTO response = new ResponseDTO();
            try
            {
                Log.Write(appSettings, LogEnum.DEBUG.ToString(), label, className, methodName, $"REQUEST: {JsonConvert.SerializeObject(request)}");
                LanguageTranslatorRequest requestBody = request.ToObject<LanguageTranslatorRequest>();
                response.Result = await LanguageTranslatorService.GoogleLanguageTranslator(appSettings, requestBody);
                response.Success = true;
                watch.Stop();
                Log.Write(appSettings, LogEnum.INFO.ToString(), label, className, methodName, $"RESULT: {JsonConvert.SerializeObject(response)} Execution Time: {watch.ElapsedMilliseconds} ms");
                return Ok(response);
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Msg = e.Message;
                watch.Stop();
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(request)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                return BadRequest(response);
            }
        }

        [HttpGet("jwt")]
        public async Task<IActionResult> JWT()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string methodName = "JWT";
            ResponseDTO response = new ResponseDTO();
            try
            {
                string googlejson = "GooglePlayStore.json";
                string googledata = "";
                using (StreamReader r = new StreamReader(googlejson))
                {
                    googledata = await r.ReadToEndAsync();
                }
                JObject googleCreds = JObject.Parse(googledata);
                string iss = googleCreds["client_email"].ToString();
                string aud = googleCreds["token_uri"].ToString();
                string scope = "https://www.googleapis.com/auth/androidpublisher";

                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                using (var sr = new StringReader(googleCreds["private_key"].ToString()))
                {
                    PemReader pr = new PemReader(sr);
                    RsaPrivateCrtKeyParameters keyPair = (RsaPrivateCrtKeyParameters)pr.ReadObject();
                    RSAParameters rsaParams = DotNetUtilities.ToRSAParameters(keyPair);
                    rsa.ImportParameters(rsaParams);
                }

                Claim[] claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Iss, iss),
                    new Claim("scope", scope),
                    new Claim(JwtRegisteredClaimNames.Aud, aud),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                    new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow.AddMinutes(20)).ToUnixTimeSeconds().ToString()),
                };

                RsaSecurityKey rsakey = new RsaSecurityKey(rsa);
                var creds = new SigningCredentials(rsakey, SecurityAlgorithms.RsaSha256);
                JwtSecurityToken jwt = new JwtSecurityToken(
                    claims: claims,
                    signingCredentials: creds
                );

                return Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Msg = e.Message;
                watch.Stop();
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                return BadRequest(response);
            }
        }

        [HttpPost("youtube")]
        public async Task<IActionResult> Youtube([FromBody] JObject request)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string methodName = "Youtube";
            ResponseDTO response = new ResponseDTO();
            try
            {
                Log.Write(appSettings, LogEnum.DEBUG.ToString(), label, className, methodName, $"REQUEST: {JsonConvert.SerializeObject(request)}");
                switch (request["operation"].ToString())
                {
                    case "search":
                        response.Result = await YoutubeService.Search(appSettings, request);
                        break;
                    case "commentThreads":
                        response.Result = await YoutubeService.CommentThreads(appSettings, request);
                        break;
                    default:
                        break;
                }
                response.Success = true;
                watch.Stop();
                Log.Write(appSettings, LogEnum.INFO.ToString(), label, className, methodName, $"RESULT: {JsonConvert.SerializeObject(response)} Execution Time: {watch.ElapsedMilliseconds} ms");
                return Ok(response);
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Msg = e.Message;
                watch.Stop();
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(request)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                return BadRequest(response);
            }
        }
    }
}
