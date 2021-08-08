using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TacarEZGithubAPI.Models
{
    public class NewRepoRequest
    {
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("description")]
        public string description { get; set; }
        public string fileName { get; set; } = "data.geojson";
        public string message { get; set; }
        public string content { get; set; }
    }
}
