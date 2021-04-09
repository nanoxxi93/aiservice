using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIService.Entities
{
    public class PythonSpeechToText
    {
        [JsonProperty(PropertyName = "tipo")]
        public string Tipo { get; set; }
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
        [JsonProperty(PropertyName = "mensaje")]
        public string Mensaje { get; set; }
        [JsonProperty(PropertyName = "respuesta")]
        public string Respuesta { get; set; }
        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }
    }
}
