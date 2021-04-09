using AIService.Entities;
using AIService.Logs;
using AIService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aiservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PCloudController : ControllerBase
    {
        private static string label = "Controllers";
        private static string className = "PCloudController";
        private readonly AppSettings appSettings;
        IConfiguration configuration;

        public PCloudController(IOptionsSnapshot<AppSettings> appSettings, IConfiguration configuration)
        {
            this.appSettings = appSettings.Value;
            this.configuration = configuration;
        }

        [HttpPost("{resource}/{operation?}")]
        public async Task<IActionResult> ResourceOperation(string resource, string operation, [FromBody] JObject data)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string methodName = "ResourceOperation";
            ResponseDTO response = new ResponseDTO();
            try
            {
                Log.Write(appSettings, LogEnum.DEBUG.ToString(), label, className, methodName, $"REQUEST: {resource}: {JsonConvert.SerializeObject(data)}");
                switch (resource)
                {
                    case "login":
                        response.Result = CommonService.StringToJObject(await PCloudService.Login(appSettings, CommonService.JObjectToDictionaryString(data)));
                        break;
                    case "folder":
                        response.Result = CommonService.StringToJObject(await PCloudService.Folder(appSettings, CommonService.JObjectToDictionaryString(data), operation));
                        break;
                    case "file":
                        response.Result = CommonService.StringToJObject(await PCloudService.File(appSettings, CommonService.JObjectToDictionaryString(data), operation));
                        break;
                    default:
                        break;
                }
                response.Success = true;
                watch.Stop();
                Log.Write(appSettings, LogEnum.INFO.ToString(), label, className, methodName, $"RESULT: {resource} - {operation}: {JsonConvert.SerializeObject(response)} Execution Time: {watch.ElapsedMilliseconds} ms");
                return Ok(response);
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Msg = e.Message;
                watch.Stop();
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {resource} - {operation}: {JsonConvert.SerializeObject(data)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {resource} - {operation}: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                return BadRequest(response);
            }
        }
    }
}
