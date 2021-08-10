using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Octokit;

namespace TacarEZGithubAPI
{
   

    public class UtilityAPI
    {
        private GitHubClient _gitHubClient;
        private string gitHubAccount = Utility.GetEnvironmentVariable("GitHubAccount");
        public UtilityAPI(GitHubClient gitHubClient)
        {
            _gitHubClient = gitHubClient;
        }

        [FunctionName("UtilityAPI")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            //string gitHubAccount = Utility.GetEnvironmentVariable("GitHubAccount");
            //var repos = await _gitHubClient.Repository.GetAllForUser(gitHubAccount);

            //for (int i = 0; i < repos.Count; i++)
            //{
            //    await _gitHubClient.Repository.Delete(repos[i].Id);
            //    Console.WriteLine("Repo with name: " + repos[i].Name + " and Id: " + repos[i].Id  + " was deleted");
            //}

            return new OkObjectResult("Delete successful");
        }
    }
}
