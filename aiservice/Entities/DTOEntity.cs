using System.Collections.Generic;

namespace AIService.Entities
{
    public class ResponseDTO
    {
        public bool Success { get; set; }
        public string Msg { get; set; }
        public dynamic Result { get; set; }
        public ResponseDTO()
        {
            Success = false;
            Msg = "";
        }
    }

    public class HttpRequestBody
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public string PostFormat { get; set; }
        public dynamic Authorization { get; set; }
        public dynamic Headers { get; set; }
        public dynamic Body { get; set; }
        public dynamic Parameters { get; set; }
        public dynamic Outputs { get; set; }
        public string Response { get; set; }
        public string ResponseCode { get; set; }
        public dynamic ResponseHeaders { get; set; }
        public string VariableValue { get; set; }
    }

    public class QueryBody
    {
        public string method { get; set; }
        public Dictionary<string, object> parameters { get; set; }
        public List<Dictionary<string, object>> table { get; set; }
        public bool @continue { get; set; }
    }

    public class QUeryBodyString
    {
        public string method { get; set; }
        public string parameters { get; set; }
        public string table { get; set; }
    }

    public class TableParameters
    {
        public string Where { get; set; }
        public string Order { get; set; }
        public long Skip { get; set; }
        public long Take { get; set; }
    }
}
