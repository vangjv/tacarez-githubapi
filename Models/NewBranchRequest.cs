using System;
using System.Collections.Generic;
using System.Text;

namespace TacarEZGithubAPI.Models
{
    public class NewBranchRequest
    {
        public string RepoName { get; set; }
        public string BranchName { get; set; }
    }
}
