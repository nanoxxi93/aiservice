using AIService.Entities;
using AIService.Logs;
using AIService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrestashopController : ControllerBase
    {
        private static string label = "Controllers";
        private static string className = "PrestashopController";
        private readonly AppSettings appSettings;
        IConfiguration configuration;

        public PrestashopController(IOptionsSnapshot<AppSettings> appSettings, IConfiguration configuration)
        {
            this.appSettings = appSettings.Value;
            this.configuration = configuration;
        }

        [HttpPost("{resource}/{operation}")]
        public async Task<IActionResult> ResourceOperation(string resource, string operation, [FromBody] JObject data)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string methodName = "ResourceOperation";
            ResponseDTO response = new ResponseDTO();
            try
            {
                Log.Write(appSettings, LogEnum.DEBUG.ToString(), label, className, methodName, $"REQUEST: {resource} - {operation}: {JsonConvert.SerializeObject(data)}");
                switch (operation)
                {
                    case "get":
                        response.Result = await PrestashopService.GetResource(appSettings, resource, CommonService.JObjectToDictionary(data));
                        break;
                    case "post":
                        response.Result = await PrestashopService.PostResource(appSettings, resource, CommonService.JObjectToDictionary(data));
                        break;
                    case "put":
                        response.Result = await PrestashopService.PutResource(appSettings, resource, CommonService.JObjectToDictionary(data));
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
