using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using AIService.Entities;
using System.Collections.Generic;
using AIService.Logs;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using aiservice.api;
using AIService.Services;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace AIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private static string label = "Controllers";
        private static string className = "ValuesController";
        private readonly AppSettings appSettings;
        IConfiguration configuration;

        public ValuesController(IOptionsSnapshot<AppSettings> appSettings, IConfiguration configuration)
        {
            this.appSettings = appSettings.Value;
            this.configuration = configuration;
        }

        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            watch.Stop();
            return Ok();
        }

        // GET api/values
        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            watch.Stop();
            return Ok(new { result = $"API v2.5.25 .Net Core 3.1 is working Execution Time: {watch.ElapsedMilliseconds} ms" });
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string methodName = "Post";
            try
            {
                var form = new Dictionary<string, object>();
                foreach (var key in Request.Form.Keys)
                {
                    var value = Request.Form[key][0];
                    form.Add(key, value);
                }
                Log.Write(appSettings, LogEnum.INFO.ToString(), label, className, methodName, $"RESULT: {JsonConvert.SerializeObject(form)} Execution Time: {watch.ElapsedMilliseconds} ms");
                watch.Stop();
            }
            catch (Exception)
            {

            }
            return Ok();
        }

        [HttpPost("uploadftp")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFTP()
        {
            var formdata = await Request.ReadFormAsync();
            var form = new Dictionary<string, object>();
            foreach (var key in formdata.Keys)
            {
                var value = Request.Form[key][0];
                form.Add(key, value);
            }
            IFormFile file = formdata.Files[0];
            using var fileStream = file.OpenReadStream();
            byte[] bytes = new byte[file.Length];
            fileStream.Read(bytes, 0, (int)file.Length);


            Stream stream = file.OpenReadStream();
            string traceIdentifier = HttpContext.TraceIdentifier;
            Startup.Progress.Add(traceIdentifier, 0);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string methodName = "UploadFTP";
            ResponseDTO response = new ResponseDTO();
            try
            {
                Task[] tasks = new[]
                {
                    Task.Run(() => AttachmentService.UploadFtp(appSettings, watch, traceIdentifier, form, file, bytes))
                };
                
                return Ok($"Request submitted successfully: {traceIdentifier}");
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Msg = e.Message;
                watch.Stop();
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(form)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                return BadRequest(response);
            }
        }

        [HttpGet("uploadftp/progress/{traceIdentifier}")]
        public ActionResult Progress(string traceIdentifier)
        {
            if (Startup.Progress.ContainsKey(traceIdentifier))
            {
                return Ok(Startup.Progress[traceIdentifier].ToString());
            }
            else
            {
                return Ok();
            }
        }

        [HttpGet("linkedin/callback")]
        public ActionResult Linkedin()
        {
            return Ok();
        }
    }
}
