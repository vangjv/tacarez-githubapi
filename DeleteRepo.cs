using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
//Delete a repo: make a Delete request 
// http://localhost:7071/api/deleterepo/Cool-new-repo
// where Cool-new-repo is the name of the repo

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TacarEZGithubAPI.Models;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace TacarEZGithubAPI
{
    public static class DeleteRepo
    {
        [FunctionName("DeleteRepo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "deleterepo/{repoName}")] HttpRequest req, string repoName,
            ILogger log)
        {
            if (repoName == null)
            {
                return new BadRequestObjectResult("Invalid repository name");
            }
            var client = new RestClient("https://api.github.com/repos/" + Utility.GetEnvironmentVariable("GitHubAccount") + "/" + repoName);
            client.Timeout = -1;
            var request = new RestRequest(Method.DELETE);
            request.AddHeader("Authorization", "token " + Utility.GetEnvironmentVariable("GithubPAT"));
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            return new OkObjectResult(response.Content);
        }
    }
}
