using System;
using System.Collections.Generic;
using System.Text;

namespace TacarEZGithubAPI.Models
{
    public class GetContentsResponse
    {
        public string name { get; set; }
        public string path { get; set; }
        public string sha { get; set; }
        public long size { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string git_url { get; set; }
        public string download_url { get; set; }
        public string type { get; set; }
    }
}
