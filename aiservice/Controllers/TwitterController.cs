using aiservice.Services;
using AIService.Entities;
using AIService.Logs;
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
    public class TwitterController : ControllerBase
    {
        private static string label = "Controllers";
        private static string className = "TwitterController";
        private readonly AppSettings appSettings;
        IConfiguration configuration;
        public TwitterController(IOptionsSnapshot<AppSettings> appSettings, IConfiguration configuration)
        {
            this.appSettings = appSettings.Value;
            this.configuration = configuration;
        }

        [HttpGet("bearer")]
        public async Task<IActionResult> Bearer()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string methodName = "TweetsSearch";
            ResponseDTO response = new ResponseDTO();
            try
            {
                Log.Write(appSettings, LogEnum.DEBUG.ToString(), label, className, methodName, $"REQUEST: ");
                response.Result = await TwitterService.Bearer(appSettings);
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
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                return BadRequest(response);
            }
        }

        [HttpPost("tweets/search")]
        public async Task<IActionResult> TweetsSearchStandard([FromBody] JObject request)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string methodName = "TweetsSearchStandard";
            ResponseDTO response = new ResponseDTO();
            try
            {
                Log.Write(appSettings, LogEnum.DEBUG.ToString(), label, className, methodName, $"REQUEST: {JsonConvert.SerializeObject(request)}");
                response.Result = await TwitterService.TweetsSearchStandard(appSettings, request);
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

        [HttpPost("tweets/search/premium")]
        public async Task<IActionResult> TweetsSearchPremium([FromBody] JObject request)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string methodName = "TweetsSearchPremium";
            ResponseDTO response = new ResponseDTO();
            try
            {
                Log.Write(appSettings, LogEnum.DEBUG.ToString(), label, className, methodName, $"REQUEST: {JsonConvert.SerializeObject(request)}");
                response.Result = await TwitterService.TweetsSearchPremium(appSettings, request);
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
