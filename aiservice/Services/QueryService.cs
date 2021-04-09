using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIService.Entities;
using AIService.Logs;
using AIService.Persistence;
using System.Data;
using System.Linq;

namespace AIService.Services
{
    public class QueryService
    {
        private static string label = "Services";
        private static string className = "QueryService";
        public async static Task<JObject> Get_SP_Dic(AppSettings appSettings, string guidRequest, QueryBody queryBody)
        {
            string methodName = "Get_SP_Dic";
            try
            {
                var result = await DapperPostgresHelper.ExecuteSP_SingleDictionary<dynamic>(appSettings, guidRequest, queryBody.method, queryBody.parameters);
                return DapperPostgresHelper.ToJObject(result);
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(queryBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public async static Task<List<JObject>> GetCollection_SP_Dic(AppSettings appSettings, string guidRequest, QueryBody queryBody)
        {
            string methodName = "GetCollection_SP_Dic";
            try
            {
                var result = await DapperPostgresHelper.ExecuteSP_MultipleDictionary<dynamic>(appSettings, guidRequest, queryBody.method, queryBody.parameters);
                return DapperPostgresHelper.ToJObjectList(result);
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(queryBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public async static Task<JObject> Get_Text_Dic(AppSettings appSettings, string guidRequest, QueryBody queryBody)
        {
            string methodName = "Get_Text_Dic";
            try
            {
                var result = await DapperPostgresHelper.ExecuteText_SingleDictionary<dynamic>(appSettings, guidRequest, queryBody.method, queryBody.parameters);
                return DapperPostgresHelper.ToJObject(result);
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(queryBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public async static Task<List<JObject>> GetCollection_Text_Dic(AppSettings appSettings, string guidRequest, QueryBody queryBody)
        {
            string methodName = "GetCollection_Text_Dic";
            try
            {
                var result = await DapperPostgresHelper.ExecuteText_MultipleDictionary<dynamic>(appSettings, guidRequest, queryBody.method, queryBody.parameters);
                return DapperPostgresHelper.ToJObjectList(result);
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(queryBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public async static Task<List<JObject>> GetCollection_SP_Dic_Free(AppSettings appSettings, string guidRequest, QueryBody queryBody)
        {
            string methodName = "GetCollection_SP_Dic_Free";
            try
            {
                var result = await DapperPostgresHelper.ExecuteSP_MultipleDictionaryFree<dynamic>(appSettings, guidRequest, queryBody.method, queryBody.parameters);
                return DapperPostgresHelper.ToJObjectList(result);
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(queryBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public async static Task<JObject> GetTable_SP_Dic(AppSettings appSettings, string guidRequest, QueryBody queryBody)
        {
            string methodName = "GetTable_SP_Dic";
            try
            {
                DataTable table = new DataTable();
                if (queryBody.table.Count == 0)
                    return DapperPostgresHelper.ToJObject(null);
                var columnNames = queryBody.table.SelectMany(dict => dict.Keys).Distinct();
                table.Columns.AddRange(columnNames.Select(c => new DataColumn(c)).ToArray());
                queryBody.table.ForEach(x =>
                {
                    var row = table.NewRow();
                    foreach (var k in x.Keys)
                    {
                        row[k] = x[k];
                    }
                    table.Rows.Add(row);
                });
                queryBody.parameters.Add("p_table", JsonConvert.SerializeObject(table));
                var result = await DapperPostgresHelper.ExecuteSP_SingleDictionary<dynamic>(appSettings, guidRequest, queryBody.method, queryBody.parameters);
                return DapperPostgresHelper.ToJObject(result);
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(queryBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }
    }
}
