using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Threading.Tasks;
using AIService.Entities;
using AIService.Logs;
using AIService.Services;
using AIService.Persistence;

namespace AIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class QueryController : ControllerBase
    {
        private static string label = "Controllers";
        private static string className = "QueryController";
        private readonly AppSettings appSettings;
        IConfiguration configuration;

        public QueryController(IOptionsSnapshot<AppSettings> appSettings, IConfiguration configuration)
        {
            this.appSettings = appSettings.Value;
            this.configuration = configuration;
        }

        [HttpPost]
        [Route("GetCollection")]
        public async Task<IActionResult> GetCollection([FromBody] QueryBody queryBody)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string methodName = "GetCollection";
            ResponseDTO response = new ResponseDTO();
            Log.Write(appSettings, LogEnum.DEBUG.ToString(), label, className, methodName, $"REQUEST {Request.Headers["guid"].ToString()}: {JsonConvert.SerializeObject(queryBody)}");
            try
            {
                response.Result = await QueryService.GetCollection_SP_Dic(appSettings, Request.Headers["guid"].ToString(), queryBody);
                response.Success = true;
                watch.Stop();
                Log.Write(appSettings, LogEnum.INFO.ToString(), label, className, methodName, $"RESULT {Request.Headers["guid"].ToString()}: {JsonConvert.SerializeObject(response)} Execution Time: {watch.ElapsedMilliseconds} ms");
                return Ok(response);
            }
            catch (Exception e)
            {
                response.Success = false;
                if (((Npgsql.PostgresException)e).MessageText != "")
                    response.Msg = ((Npgsql.PostgresException)e).MessageText;
                else
                    response.Msg = "Hubo un problema vuelva a intentarlo.";
                watch.Stop();
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, MethodBase.GetCurrentMethod().DeclaringType.ReflectedType.Name, methodName, $"ERROR {Request.Headers["guid"].ToString()}: {JsonConvert.SerializeObject(queryBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, MethodBase.GetCurrentMethod().DeclaringType.ReflectedType.Name, methodName, $"ERROR {Request.Headers["guid"].ToString()}: {e.Source + System.Environment.NewLine + e.Message} Execution Time: {watch.ElapsedMilliseconds} ms");
                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("Get")]
        public async Task<IActionResult> Get([FromBody] QueryBody queryBody)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string methodName = "Get";
            ResponseDTO response = new ResponseDTO();
            Log.Write(appSettings, LogEnum.DEBUG.ToString(), label, className, methodName, $"REQUEST {Request.Headers["guid"].ToString()}: {JsonConvert.SerializeObject(queryBody)}");
            try
            {
                response.Result = await QueryService.Get_SP_Dic(appSettings, Request.Headers["guid"].ToString(), queryBody);
                response.Success = true;
                watch.Stop();
                Log.Write(appSettings, LogEnum.INFO.ToString(), label, className, methodName, $"RESULT {Request.Headers["guid"].ToString()}: {JsonConvert.SerializeObject(response)} Execution Time: {watch.ElapsedMilliseconds} ms");
                return Ok(response);
            }
            catch (Exception e)
            {
                response.Success = false;
                if (((Npgsql.PostgresException)e).MessageText != "")
                    response.Msg = ((Npgsql.PostgresException)e).MessageText;
                else
                    response.Msg = "Hubo un problema vuelva a intentarlo.";
                watch.Stop();
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, MethodBase.GetCurrentMethod().DeclaringType.ReflectedType.Name, methodName, $"ERROR {Request.Headers["guid"].ToString()}: {JsonConvert.SerializeObject(queryBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, MethodBase.GetCurrentMethod().DeclaringType.ReflectedType.Name, methodName, $"ERROR {Request.Headers["guid"].ToString()}: {e.Source + System.Environment.NewLine + e.Message} Execution Time: {watch.ElapsedMilliseconds} ms");
                return BadRequest(response);
            }
        }
    }
}
